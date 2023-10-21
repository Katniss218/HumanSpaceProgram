using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.ReferenceFrames
{
    public static class ReferenceFrameUtils
    {
        /// <summary>
        /// Transforms a position in the old frame to a position in the new frame.
        /// </summary>
        public static Vector3 GetNewPosition( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3 oldLocalPosition )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Dbl airfPosition = oldFrame.TransformPosition( oldLocalPosition );
            Vector3 newPosition = (Vector3)newFrame.InverseTransformPosition( airfPosition );
            return newPosition;
        }

        /// <summary>
        /// Transforms a direction in the old frame to a direction in the new frame.
        /// </summary>
        public static Vector3 GetNewDirection( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3 oldLocalDirection )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3 airfDirection = oldFrame.TransformDirection( oldLocalDirection );
            Vector3 newDirection = newFrame.InverseTransformDirection( airfDirection );
            return newDirection;
        }

        /// <summary>
        /// Transforms a rotation in the old frame to a rotation in the new frame.
        /// </summary>
        public static Quaternion GetNewRotation( IReferenceFrame oldFrame, IReferenceFrame newFrame, Quaternion oldLocalRotation )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            QuaternionDbl airfRotation = oldFrame.TransformRotation( oldLocalRotation );
            Quaternion newRotation = (Quaternion)newFrame.InverseTransformRotation( airfRotation );
            return newRotation;
        }
    }
}