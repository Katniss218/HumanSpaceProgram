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
        Vector3 TransformPosition( Vector3Dbl globalPosition );
        Quaternion TransformRotation( Quaternion globalRotation );
    }

    public class DefaultFrame : IReferenceFrame
    {
        // incomplete.

        public Vector3 TransformPosition( Vector3Dbl globalPosition )
        {
            return new Vector3( (float)globalPosition.x, (float)globalPosition.y, (float)globalPosition.z );
        }

        public Quaternion TransformRotation( Quaternion globalRotation )
        {
            return new Quaternion( (float)globalRotation.x, (float)globalRotation.y, (float)globalRotation.z, (float)globalRotation.w );
        }
    }
}
