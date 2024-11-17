using UnityEngine;

namespace HSP.ReferenceFrames
{
    public static class ReferenceFrameUtils
    {
        /// <summary>
        /// Transforms a position in the old frame to a position in the new frame.
        /// </summary>
        public static Vector3Dbl GetNewPosition( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3Dbl oldLocalPosition )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Dbl absolutePosition = oldFrame.TransformPosition( oldLocalPosition );
            Vector3Dbl newPosition = newFrame.InverseTransformPosition( absolutePosition );
            return newPosition;
        }

        /// <summary>
        /// Transforms a direction in the old frame to a direction in the new frame.
        /// </summary>
        public static Vector3 GetNewDirection( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3 oldLocalDirection )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3 absoluteDirection = oldFrame.TransformDirection( oldLocalDirection );
            Vector3 newDirection = newFrame.InverseTransformDirection( absoluteDirection );
            return newDirection;
        }

        /// <summary>
        /// Transforms a rotation in the old frame to a rotation in the new frame.
        /// </summary>
        public static QuaternionDbl GetNewRotation( IReferenceFrame oldFrame, IReferenceFrame newFrame, QuaternionDbl oldLocalRotation )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            QuaternionDbl aabsoluteRotation = oldFrame.TransformRotation( oldLocalRotation );
            QuaternionDbl newRotation = newFrame.InverseTransformRotation( aabsoluteRotation );
            return newRotation;
        }

        /// <summary>
        /// Transforms a velocity in the old frame to a velocity in the new frame.
        /// </summary>
        public static Vector3Dbl GetNewVelocity( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3Dbl oldLocalVelocity )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Dbl absoluteVelocity = oldFrame.TransformVelocity( oldLocalVelocity );
            Vector3Dbl newVelocity = newFrame.InverseTransformVelocity( absoluteVelocity );
            return newVelocity;
        }

        /// <summary>
        /// Transforms an angular velocity in the old frame to an angular velocity in the new frame.
        /// </summary>
        public static Vector3Dbl GetNewAngularVelocity( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3Dbl oldLocalAngularVelocity )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Dbl absoluteAngularVelocity = oldFrame.TransformVelocity( oldLocalAngularVelocity );
            Vector3Dbl newAngularVelocity = newFrame.InverseTransformVelocity( absoluteAngularVelocity );
            return newAngularVelocity;
        }
    }
}