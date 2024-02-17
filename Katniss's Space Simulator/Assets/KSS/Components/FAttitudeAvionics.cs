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

namespace KSS.Components
{
	/// <summary>
	/// Stability Assist (SAS) module.
	/// </summary>
	[Obsolete( "Not implemented fully yet." )]
	public class FAttitudeAvionics : MonoBehaviour
	{

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

		/*void FixedUpdate()
		{
			UpdatePredictionPI();

			Vector3Dbl deltaEuler = -_error0;

			for( int i = 0; i < 3; i++ )
				if( Math.Abs( _actuation[i] ) < EPS || double.IsNaN( _actuation[i] ) )
					_actuation[i] = 0;

			act = _actuation;
		}

		private void UpdatePredictionPI()
		{
			_omega0 = vessel.PhysicsObject.AngularVelocity;

			UpdateError();
		}

		private void UpdateError()
		{
			Transform vesselTransform = vessel.ReferenceTransform;

			// 1. The Euler(-90) here is because the unity transform puts "up" as the pointy end, which is wrong.  The rotation means that
			// "forward" becomes the pointy end, and "up" and "right" correctly define e.g. AoA/pitch and AoS/yaw.  This is just KSP being KSP.
			// 2. We then use the inverse ship rotation to transform the requested attitude into the ship frame (we do everything in the ship frame
			// first, and then negate the error to get the error in the target reference frame at the end).
			Quaternion deltaRotation = Quaternion.Inverse( vesselTransform.transform.rotation * Quaternion.Euler( -90, 0, 0 ) ) * RequestedAttitude;

			// get us some euler angles for the target transform
			Vector3Dbl ea = deltaRotation.eulerAngles;
			double pitch = ea[0] * UtilMath.Deg2Rad;
			double yaw = ea[1] * UtilMath.Deg2Rad;
			double roll = ea[2] * UtilMath.Deg2Rad;

			// law of cosines for the "distance" of the miss in radians
			_errorTotal = Math.Acos( Math.Clamp( Math.Cos( pitch ) * Math.Cos( yaw ), -1, 1 ) );

			// this is the initial direction of the great circle route of the requested transform
			// (pitch is latitude, yaw is -longitude, and we are "navigating" from 0,0)
			// doing this calculation is the ship frame is a bit easier to reason about.
			var temp = new Vector3Dbl( Math.Sin( pitch ), Math.Cos( pitch ) * Math.Sin( -yaw ), 0 );
			temp = temp.normalized * _errorTotal;

			// we assemble phi in the pitch, roll, yaw basis that vessel.MOI uses (right handed basis)
			var phi = new Vector3Dbl(
				MuUtils.ClampRadiansPi( temp[0] ), // pitch distance around the geodesic
				MuUtils.ClampRadiansPi( roll ),
				MuUtils.ClampRadiansPi( temp[1] ) // yaw distance around the geodesic
				);

			// apply the axis control from the parent controller
			phi.Scale( AxisControl );

			// the error in the ship's position is the negative of the reference position in the ship frame
			_error0 = -phi;
		}*/
	}
}