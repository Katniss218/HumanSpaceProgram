using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    public static class ReferenceFrameTransformUtils
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetScenePositionFromAbsolute( Transform transform, Rigidbody rigidbody, Vector3Dbl absolutePosition )
        {
            Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( absolutePosition );
            transform.position = scenePos; // Setting the transform is still important for some cases.

            // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
            if( rigidbody != null )
                rigidbody.position = scenePos;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetScenePositionFromAbsolute( Transform transform, Rigidbody rigidbody, Vector3Dbl absolutePosition, IReferenceFrame frame )
        {
            Vector3 scenePos = (Vector3)frame.InverseTransformPosition( absolutePosition );
            transform.position = scenePos; // Setting the transform is still important for some cases.

            // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
            if( rigidbody != null )
                rigidbody.position = scenePos;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetSceneRotationFromAbsolute( Transform transform, Rigidbody rigidbody, QuaternionDbl absoluteRotation )
        {
            Quaternion sceneRotation = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( absoluteRotation );
            transform.rotation = sceneRotation; // Setting the transform is still important for some cases.

            // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
            if( rigidbody != null )
                rigidbody.rotation = sceneRotation;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetSceneVelocityFromAbsolute( Rigidbody rigidbody, Vector3Dbl absoluteVelocity )
        {
            Vector3 sceneVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( absoluteVelocity );
            rigidbody.velocity = sceneVelocity;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetSceneAngularVelocityFromAbsolute( Rigidbody rigidbody, Vector3Dbl absoluteAngularVelocity )
        {
            Vector3 sceneAngularVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( absoluteAngularVelocity );
            rigidbody.angularVelocity = sceneAngularVelocity;
        }
    }
}