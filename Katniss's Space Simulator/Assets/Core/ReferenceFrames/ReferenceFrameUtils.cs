using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.ReferenceFrames
{
    public static class ReferenceFrameUtils
    {
        public static Vector3 GetNewPosition( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3 oldLocalPosition )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Dbl airfPosition = oldFrame.TransformPosition( oldLocalPosition );
            Vector3 newPosition = newFrame.InverseTransformPosition( airfPosition );
            return newPosition;
        }

        public static Quaternion GetNewRotation( IReferenceFrame oldFrame, IReferenceFrame newFrame, Quaternion oldLocalRotation )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Quaternion airfRotation = oldFrame.TransformRotation( oldLocalRotation );
            Quaternion newRotation = newFrame.InverseTransformRotation( airfRotation );
            return newRotation;
        }
    }
}