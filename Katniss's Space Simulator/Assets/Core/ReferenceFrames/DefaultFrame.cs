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
        // Instead of the "Vector3Dbl", store vectors as a combination of "bigint" and "float" maybe???
        // bigint for the large scale reference frame setting, and float for the fine detail.
        // integer could be also represented as multiplied by a large factor so that it acts like the upper bits of a larger number. And the float acts like the lower bits???
        // - the factor would be at most equal to half of the allowed floating origin radius.
        // - that way would disallow precise placement of the reference frame, we need probably around 10000 meters granularity at the least.


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
