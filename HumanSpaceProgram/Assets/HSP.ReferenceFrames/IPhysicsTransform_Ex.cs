using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    public static class IPhysicsTransform_Ex
    {
        /// <summary>
        /// Calculates the rotational inertia (analogous to mass) about a particular axis in scene space.
        /// </summary>
        /// <param name="sceneAxis">The direction of torque.</param>
        /// <returns>The magnitude of the moment of inertia along the specified axis, in [kg*m^2]</returns>
        public static double GetInertia( this IPhysicsTransform self, Vector3 sceneAxis )
        {
            Vector3 axisInInertiaSpace = Quaternion.Inverse( self.MomentsOfInertiaRotation ) * self.transform.InverseTransformDirection( sceneAxis.normalized );

            Vector3 principalInertia = self.MomentsOfInertia;

            float inertia = principalInertia.x * (axisInInertiaSpace.x * axisInInertiaSpace.x)
                          + principalInertia.y * (axisInInertiaSpace.y * axisInInertiaSpace.y)
                          + principalInertia.z * (axisInInertiaSpace.z * axisInInertiaSpace.z);

            return inertia;
        }

        /// <summary>
        /// Gets the full 3x3 moments of inertia tensor with axes aligned to the local space of the object.
        /// </summary>
        public static Matrix3x3 GetInertiaTensor( this IPhysicsTransform self )
        {
            Matrix3x3 R = Matrix3x3.Rotate( self.MomentsOfInertiaRotation );
            Matrix3x3 S = Matrix3x3.Scale( self.MomentsOfInertia );
            return R * S * R.transpose;
        }

        /// <summary>
        /// Sets the full 3x3 moments of inertia tensor with axes aligned to the local space of the object.
        /// </summary>
        public static void SetInertiaTensor( this IPhysicsTransform self, Matrix3x3 value )
        {
            (Vector3 eigenvector, float eigenvalue)[] eigen = value.Diagonalize().OrderByDescending( m => m.eigenvalue ).ToArray();

            Matrix3x3 m = new Matrix3x3( eigen[0].eigenvector.x, eigen[0].eigenvector.y, eigen[0].eigenvector.z,
                eigen[1].eigenvector.x, eigen[1].eigenvector.y, eigen[1].eigenvector.z,
                eigen[2].eigenvector.x, eigen[2].eigenvector.y, eigen[2].eigenvector.z );

            self.MomentsOfInertia = new Vector3( eigen[0].eigenvalue, eigen[1].eigenvalue, eigen[2].eigenvalue );
            self.MomentsOfInertiaRotation = m.rotation;
        }
    }
}