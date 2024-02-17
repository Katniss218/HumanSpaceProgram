using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.Physics
{
	public static class InertiaUtils
	{
		[Obsolete( "untested" )]
		public static Matrix3x3 CalculateInertiaTensor( IEnumerable<(float, Vector3)> pointMasses )
		{
			float totalMass = 0.0f;
			Vector3 centerOfMass = Vector3.zero;

			(float mass, Vector3 pos)[] masses = pointMasses.ToArray();

			// Calculate total mass and center of mass
			for( int i = 0; i < masses.Length; i++ )
			{
				totalMass += masses[i].mass;
				centerOfMass += (float)masses[i].mass * masses[i].pos;
			}

			centerOfMass /= (float)totalMass;

			// Calculate moment of inertia tensor components
			Matrix3x3 momentOfInertiaTensor = Matrix3x3.zero;
			for( int i = 0; i < masses.Length; i++ )
			{
				Vector3 r = masses[i].pos - centerOfMass;
				float rSquared = Vector3.Dot( r, r );

				momentOfInertiaTensor.m00 += masses[i].mass * (rSquared - r.x * r.x);
				momentOfInertiaTensor.m11 += masses[i].mass * (rSquared - r.y * r.y);
				momentOfInertiaTensor.m22 += masses[i].mass * (rSquared - r.z * r.z);
				momentOfInertiaTensor.m01 += masses[i].mass * (-r.x * r.y);
				momentOfInertiaTensor.m02 += masses[i].mass * (-r.x * r.z);
				momentOfInertiaTensor.m12 += masses[i].mass * (-r.y * r.z);
			}

			momentOfInertiaTensor.m10 = momentOfInertiaTensor.m01;
			momentOfInertiaTensor.m20 = momentOfInertiaTensor.m02;
			momentOfInertiaTensor.m21 = momentOfInertiaTensor.m12;

			if( momentOfInertiaTensor == Matrix3x3.zero )
			{
				momentOfInertiaTensor = Matrix3x3.identity;
			}

			return momentOfInertiaTensor;
		}
	}
}
