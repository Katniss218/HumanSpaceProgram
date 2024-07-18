using HSP.ControlSystems.Controls;
using HSP.ControlSystems;
using System;
using UnityEngine;
using HSP.Core;
using System.Runtime.CompilerServices;
using UnityPlus.Serialization;
using HSP.Vessels;
using HSP.Time;

namespace HSP.Components
{
	/// <summary>
	/// Stability Assist (SAS) module.
	/// </summary>
	public class FAttitudeAvionics : MonoBehaviour
	{
		private Vessel _vessel;

        // error in pitch, roll, yaw, in [Rad]
        [field: SerializeField]
        private Vector3Dbl _error0 = Vector3Dbl.zero;
		private Vector3Dbl _error1 = new Vector3Dbl( double.NaN, double.NaN, double.NaN );

		// max angular acceleration
		private Vector3 _maxAlpha = Vector3.zero;

		// max angular rotation
		private Vector3Dbl _maxOmega = Vector3Dbl.zero;
		private Vector3Dbl _omega0 = new Vector3Dbl( double.NaN, double.NaN, double.NaN );
		private Vector3Dbl _targetOmega = Vector3Dbl.zero;
		private Vector3Dbl _targetTorque = Vector3Dbl.zero;
		private Vector3Dbl _actuation = Vector3Dbl.zero;

		// error
		private double _errorTotal;

		private const double EPS = 2.2204e-16;

		private double PosKp = 1.98;
		private double PosDeadband = 0.002;
		private double VelN = 84.1994541201249;
		private double VelB = 0.994;
		private double VelC = 0.0185;
		private double VelKp = 10;
		private double VelKi = 20;
		private double VelKd = 0.425;
		private double VelDeadband = 0.0001;
		private bool VelClegg;
		private double VelSmoothIn = 1;
		private double VelSmoothOut = 1;
		private double PosSmoothIn = 1;
		private double MaxStoppingTime = 2.0;
		private double MinFlipTime = 120;
		private double RollControlRange = 5;
		private bool UseStoppingTime = true;
		private bool UseControlRange = true;
		private bool UseFlipTime = true;
		
		private readonly PIDLoop[] _pid = { new PIDLoop(), new PIDLoop(), new PIDLoop() };

		private Vector3 Ac_torque = Vector3.one;
		private Vector3 Ac_AxisControl = Vector3.one;
		private Vector3 Ac_ActuationControl = Vector3.one;
		private Vector3 Ac_OmegaTarget = new Vector3( float.NaN, float.NaN, float.NaN );

        [field: SerializeField]
        private Vector3Dbl outDeltaEuler;
		private Vector3Dbl outActuation;

		/// <summary>
		/// Desired vessel-space pitch, yaw, roll, in [-Inf..Inf].
		/// </summary>
		[NamedControl( "Attitude" )]
		public ControllerOutput<Vector3> OnSetAttitude = new();

		public Quaternion TargetOrientation { get; set; } = Quaternion.identity; // this needs to be updated by the other control things.

		private static double ClampRadiansTwoPi( double angle )
		{
			angle %= (2 * Math.PI);
			if( angle < 0 )
				return angle + (2 * Math.PI);
			return angle;
		}

		private static double ClampRadiansPi( double angle )
		{
			angle = ClampRadiansTwoPi( angle );
			if( angle > Math.PI )
				return angle - (2 * Math.PI);
			return angle;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsFinite( float f )
			=> !float.IsNaN( f ) && !float.IsInfinity( f );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsFinite( Vector3Dbl vec )
			=> !double.IsNaN( vec.x ) && !double.IsInfinity( vec.x )
			&& !double.IsNaN( vec.y ) && !double.IsInfinity( vec.y )
			&& !double.IsNaN( vec.z ) && !double.IsInfinity( vec.z );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsFinite( Vector3 vec )
			=> !float.IsNaN( vec.x ) && !float.IsInfinity( vec.x )
			&& !float.IsNaN( vec.y ) && !float.IsInfinity( vec.y )
			&& !float.IsNaN( vec.z ) && !float.IsInfinity( vec.z );

		void OnEnable()
		{
			_vessel = this.transform.GetVessel();
			if( _vessel == null )
				return;
			TargetOrientation = _vessel.ReferenceTransform.rotation;
		}

		void FixedUpdate()
		{
			if( _vessel == null )
				return;

			UpdatePredictionPI();

			outDeltaEuler = -_error0;

			for( int i = 0; i < 3; i++ )
			{
				if( Math.Abs( _actuation[i] ) < EPS || double.IsNaN( _actuation[i] ) )
				{
					_actuation[i] = 0;
				}
			}
			outActuation = _actuation;

			OnSetAttitude.TrySendSignal( (Vector3)outActuation * 5f );
		}

		private void UpdatePredictionPI()
		{
			_omega0 = _vessel.PhysicsObject.AngularVelocity;

			UpdateError();

			// lowpass filter on the error input
			_error0 = IsFinite( _error1 )
				? _error1 + PosSmoothIn * (_error0 - _error1)
				: _error0;

			Vector3 controlTorque = Ac_torque;

			// needed to stop wiggling at higher phys warp
			float timeScale = TimeManager.TimeScale;

			// see https://archive.is/NqoUm and the "Alt Hold Controller", the acceleration PID is not implemented so we only
			// have the first two PIDs in the cascade.
			for( int i = 0; i < _pid.Length; i++ )
			{
				double error = _error0[i];

				if( Math.Abs( error ) < PosDeadband )
					error = 0;
				else
					error -= Math.Sign( error ) * PosDeadband; // Subtract the deadband (shifts the error by the deadband amount).

				_maxAlpha[i] = controlTorque[i] / _vessel.PhysicsObject.MomentsOfInertia[i];

				if( _maxAlpha[i] == 0 )
					_maxAlpha[i] = 1;

				if( IsFinite( Ac_OmegaTarget[i] ) )
				{
					_targetOmega[i] = Ac_OmegaTarget[i];
				}
				else
				{
					double posKp = PosKp / timeScale;
					double effLD = _maxAlpha[i] / (2 * posKp * posKp);

					if( Math.Abs( error ) <= 2 * effLD )
					{
						// linear ramp down of acceleration
						_targetOmega[i] = -posKp * error;
					}
					else
					{
						// v = - sqrt(2 * F * x / m) is target stopping velocity based on distance
						_targetOmega[i] = -Math.Sqrt( 2 * _maxAlpha[i] * (Math.Abs( error ) - effLD) ) * Math.Sign( error );
					}

					if( UseStoppingTime )
					{
						_maxOmega[i] = _maxAlpha[i] * MaxStoppingTime;
						if( UseFlipTime )
						{
							_maxOmega[i] = Math.Max( _maxOmega[i], Math.PI / MinFlipTime );
						}
						_targetOmega[i] = Math.Clamp( _targetOmega[i], -_maxOmega[i], _maxOmega[i] );
					}

					if( UseControlRange && _errorTotal * Mathf.Rad2Deg > RollControlRange )
					{
						_targetOmega[1] = 0;
					}
				}

				_pid[i].Kp = VelKp / (_maxAlpha[i] * timeScale);
				_pid[i].Ki = VelKi / (_maxAlpha[i] * timeScale * timeScale);
				_pid[i].Kd = VelKd / _maxAlpha[i];
				_pid[i].N = VelN;
				_pid[i].B = VelB;
				_pid[i].C = VelC;
				_pid[i].Ts = TimeManager.FixedDeltaTime;
				_pid[i].SmoothIn = Math.Clamp( VelSmoothIn, 0, 1 );
				_pid[i].SmoothOut = Math.Clamp( VelSmoothOut, 0, 1 );
				_pid[i].MinOutput = -1;
				_pid[i].MaxOutput = 1;
				_pid[i].Deadband = VelDeadband;
				_pid[i].Clegg = VelClegg;

				// need the negative from the pid due to KSP's orientation of actuation
				_actuation[i] = -_pid[i].Update( _targetOmega[i], _omega0[i] );

				if( Math.Abs( _actuation[i] ) < EPS || double.IsNaN( _actuation[i] ) )
				{
					_actuation[i] = 0;
				}

				_targetTorque[i] = _actuation[i] * Ac_torque[i];

				if( Ac_ActuationControl[i] == 0 )
				{
					ResetPID( i );
				}
			}

			_error1 = _error0;
		}

		private void UpdateError()
		{
			// 1. The Euler(-90) here is because the unity transform puts "up" as the pointy end, which is wrong.  The rotation means that
			// "forward" becomes the pointy end, and "up" and "right" correctly define e.g. AoA/pitch and AoS/yaw.  This is just KSP being KSP.
			// 2. We then use the inverse ship rotation to transform the requested attitude into the ship frame (we do everything in the ship frame
			// first, and then negate the error to get the error in the target reference frame at the end).
			Quaternion deltaRotation = Quaternion.Inverse( _vessel.ReferenceTransform.rotation * Quaternion.Euler( -90, 0, 0 ) ) * TargetOrientation;

			Vector3 deltaRotationEuler = deltaRotation.eulerAngles * Mathf.Deg2Rad;

			// get us some euler angles for the target transform
			float pitch = deltaRotationEuler[0];
			float yaw = deltaRotationEuler[1];
			float roll = deltaRotationEuler[2];

			// law of cosines for the "distance" of the miss in radians
			_errorTotal = Math.Acos( Math.Clamp( Math.Cos( pitch ) * Math.Cos( yaw ), -1.0, 1.0 ) );

			// this is the initial direction of the great circle route of the requested transform
			// (pitch is latitude, yaw is -longitude, and we are "navigating" from 0,0)
			// doing this calculation is the ship frame is a bit easier to reason about.
			Vector3Dbl greatCircleDir = new Vector3Dbl( Math.Sin( pitch ), Math.Cos( pitch ) * Math.Sin( -yaw ), 0.0 );
			greatCircleDir = greatCircleDir.normalized * _errorTotal;

			// we assemble phi in the pitch, roll, yaw basis that vessel.MOI uses (right handed basis)
			Vector3Dbl phi = new Vector3Dbl(
				ClampRadiansPi( greatCircleDir[0] ), // pitch distance around the geodesic
				ClampRadiansPi( roll ),
				ClampRadiansPi( greatCircleDir[1] ) ); // yaw distance around the geodesic

			// apply the axis control from the parent controller
			phi.Scale( Ac_AxisControl );

			// the error in the ship's position is the negative of the reference position in the ship frame
#warning TODO - something somewhere here is broken, because the error doesn't correlate with the vessel angular rotation whatsoever.
			_error0 = -phi;
		}

		private void ResetPID( int i )
		{
			_pid[i].Reset();
			_omega0[i] = double.NaN;
			_error0[i] = double.NaN;
			_error1[i] = double.NaN;
        }

        [MapsInheritingFrom( typeof( FAttitudeAvionics ) )]
        public static SerializationMapping FAttitudeAvionicsMapping()
        {
			return new MemberwiseSerializationMapping<FAttitudeAvionics>()
			{
				("on_set_attitude", new Member<FAttitudeAvionics, ControllerOutput<Vector3>>( o => o.OnSetAttitude ))
			};
        }
    }
}