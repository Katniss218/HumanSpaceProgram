using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor.PackageManager;
using KSS.Core;
using UnityEngine.Diagnostics;
using UnityEditor;
using KSS.Core.ReferenceFrames;
using System.Runtime.CompilerServices;

namespace KSS.Components
{
	/// <summary>
	/// Stability Assist (SAS) module.
	/// </summary>
	[Obsolete( "Not implemented fully yet." )]
	public class FAttitudeAvionics : MonoBehaviour
	{
		/// <summary>
		/// Defines the coordinate frame for the attitude control hold.
		/// </summary>
		IReferenceFrame AttitudeReference; // todo - this will need to be updated/reset periodically.

		Quaternion TargetOrientation; // this needs to be updated by the other control things.

		/// <summary>
		/// Desired vessel-space pitch, yaw, roll, in [-Inf..Inf].
		/// </summary>
		[NamedControl( "Attitude" )]
		public ControllerOutput<Vector3> OnSetAttitude = new();

		private IPartObject vessel;

		private readonly PIDLoop[] _pid = { new PIDLoop(), new PIDLoop(), new PIDLoop() };

		// error in pitch, roll, yaw, in [Rad]
		private Vector3Dbl _error0 = Vector3Dbl.zero;
		private Vector3Dbl _error1 = new Vector3Dbl( double.NaN, double.NaN, double.NaN );

		// max angular acceleration
		private Vector3Dbl _maxAlpha = Vector3Dbl.zero;

		// max angular rotation
		private Vector3Dbl _maxOmega = Vector3Dbl.zero;
		private Vector3Dbl _omega0 = new Vector3Dbl( double.NaN, double.NaN, double.NaN );
		private Vector3Dbl _targetOmega = Vector3Dbl.zero;
		private Vector3Dbl _targetTorque = Vector3Dbl.zero;
		private Vector3Dbl _actuation = Vector3Dbl.zero;

		// error
		private double _errorTotal;

		private const double EPS = 2.2204e-16;

		void FixedUpdate()
		{
			UpdatePredictionPI();

			Vector3Dbl deltaEuler = -_error0;

			for( int i = 0; i < 3; i++ )
			{
				if( Math.Abs( _actuation[i] ) < EPS || double.IsNaN( _actuation[i] ) )
				{
					_actuation[i] = 0;
				}
			}
			//act = _actuation;
		}

		static double ClampRadiansTwoPi( double angle )
		{
			angle %= (2 * Math.PI);
			if( angle < 0 )
				return angle + (2 * Math.PI);
			return angle;
		}

		static double ClampRadiansPi( double angle )
		{
			angle = ClampRadiansTwoPi( angle );
			if( angle > Math.PI )
				return angle - (2 * Math.PI);
			return angle;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsFinite( Vector3Dbl x )
			=> !double.IsNaN( x.x ) && !double.IsInfinity( x.x )
			&& !double.IsNaN( x.y ) && !double.IsInfinity( x.y )
			&& !double.IsNaN( x.z ) && !double.IsInfinity( x.z );

		private void UpdatePredictionPI()
		{
			/*_omega0 = vessel.PhysicsObject.AngularVelocity;

			UpdateError();

			// lowpass filter on the error input
			_error0 = IsFinite( _error1 ) ? _error1 + PosSmoothIn * (_error0 - _error1) : _error0;

			Vector3Dbl controlTorque = Ac.torque;

			// needed to stop wiggling at higher phys warp
			double warpFactor = 1.0; //Ac.VesselState.deltaT / 0.02;

			// see https://archive.is/NqoUm and the "Alt Hold Controller", the acceleration PID is not implemented so we only
			// have the first two PIDs in the cascade.
			for( int i = 0; i < _pid.Length; i++ )
			{
				double error = _error0[i];

				if( Math.Abs( error ) < PosDeadband )
					error = 0;
				else
					error -= Math.Sign( error ) * PosDeadband;

				_maxAlpha[i] = controlTorque[i] / vessel.PhysicsObject.MomentOfInertiaTensor[i]; // TODO - possibly moi should be the eigenvalues instead.

				if( _maxAlpha[i] == 0 )
					_maxAlpha[i] = 1;

				if( Ac.OmegaTarget[i].IsFinite() )
				{
					_targetOmega[i] = Ac.OmegaTarget[i];
				}
				else
				{
					double posKp = PosKp / warpFactor;
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
						if( UseFlipTime ) _maxOmega[i] = Math.Max( _maxOmega[i], Math.PI / MinFlipTime );
						_targetOmega[i] = Math.Clamp( _targetOmega[i], -_maxOmega[i], _maxOmega[i] );
					}

					if( UseControlRange && _errorTotal * Mathf.Rad2Deg > RollControlRange )
						_targetOmega[1] = 0;
				}

				_pid[i].Kp = VelKp / (_maxAlpha[i] * warpFactor);
				_pid[i].Ki = VelKi / (_maxAlpha[i] * warpFactor * warpFactor);
				_pid[i].Kd = VelKd / _maxAlpha[i];
				_pid[i].N = VelN;
				_pid[i].B = VelB;
				_pid[i].C = VelC;
				_pid[i].Ts = Ac.VesselState.deltaT;
				_pid[i].SmoothIn = Math.Clamp( VelSmoothIn, 0, 1 );
				_pid[i].SmoothOut = Math.Clamp( VelSmoothOut, 0, 1 );
				_pid[i].MinOutput = -1;
				_pid[i].MaxOutput = 1;
				_pid[i].Deadband = VelDeadband;
				_pid[i].Clegg = VelClegg;

				// need the negative from the pid due to KSP's orientation of actuation
				_actuation[i] = -_pid[i].Update( _targetOmega[i], _omega0[i] );

				if( Math.Abs( _actuation[i] ) < EPS || double.IsNaN( _actuation[i] ) )
					_actuation[i] = 0;

				_targetTorque[i] = _actuation[i] * Ac.torque[i];

				if( Ac.ActuationControl[i] == 0 )
					ResetPID( i );
			}

			_error1 = _error0;*/
		}

		private void UpdateError()
		{
			Quaternion RequestedAttitude = Quaternion.identity;
			Vector3Dbl AxisControl = Vector3Dbl.one;



			Transform vesselTransform = vessel.ReferenceTransform;

			// 1. The Euler(-90) here is because the unity transform puts "up" as the pointy end, which is wrong.  The rotation means that
			// "forward" becomes the pointy end, and "up" and "right" correctly define e.g. AoA/pitch and AoS/yaw.  This is just KSP being KSP.
			// 2. We then use the inverse ship rotation to transform the requested attitude into the ship frame (we do everything in the ship frame
			// first, and then negate the error to get the error in the target reference frame at the end).
			Quaternion deltaRotation = Quaternion.Inverse( vesselTransform.transform.rotation /* * Quaternion.Euler( -90, 0, 0 )*/ ) * RequestedAttitude;
			Vector3 deltaRotationEuler = deltaRotation.eulerAngles;

			// get us some euler angles for the target transform
			float pitch = deltaRotationEuler[0] * Mathf.Deg2Rad;
			float yaw = deltaRotationEuler[1] * Mathf.Deg2Rad;
			float roll = deltaRotationEuler[2] * Mathf.Deg2Rad;

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
			phi.Scale( AxisControl );

			// the error in the ship's position is the negative of the reference position in the ship frame
			_error0 = -phi;
		}

		private void ResetPID( int i )
		{
			_pid[i].Reset();
			_omega0[i] = double.NaN;
			_error0[i] = double.NaN;
			_error1[i] = double.NaN;
		}
	}
}