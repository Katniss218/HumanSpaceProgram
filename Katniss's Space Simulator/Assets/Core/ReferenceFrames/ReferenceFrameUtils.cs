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
        public static Vector3 GetNewPosition( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3 oldPosition )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Dbl globalPosition = oldFrame.TransformPosition( oldPosition );
            Vector3 newPosition = newFrame.InverseTransformPosition( globalPosition );
            return newPosition;
        }
    }
}