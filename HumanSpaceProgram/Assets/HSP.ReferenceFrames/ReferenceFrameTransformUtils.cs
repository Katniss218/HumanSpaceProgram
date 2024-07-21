using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    public static class ReferenceFrameTransformUtils
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void UpdateScenePositionFromAbsolute( Transform transform, Rigidbody rigidbody, Vector3Dbl absolutePosition )
        {
            Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( absolutePosition );
            // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
            if( rigidbody != null )
                rigidbody.position = scenePos;

            transform.position = scenePos;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void UpdateSceneRotationFromAbsolute( Transform transform, Rigidbody rigidbody, QuaternionDbl absoluteRotation )
        {
            Quaternion sceneRotation = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( absoluteRotation );
            // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
            if( rigidbody != null )
                rigidbody.rotation = sceneRotation;

            transform.rotation = sceneRotation;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void UpdateSceneVelocityFromAbsolute( Rigidbody rigidbody, Vector3Dbl absoluteVelocity )
        {
            Vector3 sceneVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( absoluteVelocity );
            rigidbody.velocity = sceneVelocity;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void UpdateSceneAngularVelocityFromAbsolute( Rigidbody rigidbody, Vector3Dbl absoluteAngularVelocity )
        {
            Vector3 sceneAngularVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( absoluteAngularVelocity );
            rigidbody.angularVelocity = sceneAngularVelocity;
        }
    }
}