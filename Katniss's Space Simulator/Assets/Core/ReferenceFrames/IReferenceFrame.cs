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
        IReferenceFrame Shift( Vector3 vector );

        /// <summary>
        /// Transforms a point in the Global Inertial Reference Frame space to the scene space ("world space").
        /// </summary>
        Vector3 TransformPosition( Vector3Large globalPosition );

        /// <summary>
        /// Transforms a point in the scene space ("world space") to the Global Inertial Reference Frame space.
        /// </summary>
        Vector3Large InverseTransformPosition( Vector3 localPosition );
        //Quaternion TransformRotation( Quaternion globalRotation );
    }
}
