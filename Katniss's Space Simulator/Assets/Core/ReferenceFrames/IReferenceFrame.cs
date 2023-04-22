using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.ReferenceFrames
{
    public interface IReferenceFrame
    {
        // "Global Inertial Reference Frame" (GIRF) is my invention.

        // It represents the absolute world space.
        // We can't use Unity's world space for that because of precision issues.

        IReferenceFrame Shift( Vector3 vector );

        /// <summary>
        /// Transforms a point in the scene space ("world space") to the Global Inertial Reference Frame space.
        /// </summary>
        Vector3Large TransformPosition( Vector3 localPosition );

        /// <summary>
        /// Transforms a point in the Global Inertial Reference Frame space to the scene space ("world space").
        /// </summary>
        Vector3 InverseTransformPosition( Vector3Large globalPosition );

        //Quaternion TransformRotation( Quaternion globalRotation );
    }
}
