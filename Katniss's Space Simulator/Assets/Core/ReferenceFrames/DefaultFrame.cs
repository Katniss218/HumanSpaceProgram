using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.ReferenceFrames
{
    public class DefaultFrame : IReferenceFrame
    {
        // incomplete.
        // Instead of the "Vector3Dbl", store vectors as a combination of "bigint" and "float" maybe???
        // bigint for the large scale reference frame setting, and float for the fine detail.
        // integer could be also represented as multiplied by a large factor so that it acts like the upper bits of a larger number. And the float acts like the lower bits???
        // - the factor would be at most equal to half of the allowed floating origin radius.
        // - that way would disallow precise placement of the reference frame, we need probably around 10000 meters granularity at the least.

        public Vector3Large ReferencePosition { get; private set; } // these could be a matrix mayhaps (+ rotation, and scale)?

        public DefaultFrame( Vector3Large referencePosition )
        {
            this.ReferencePosition = referencePosition;
        }

        public IReferenceFrame Shift( Vector3 distanceDelta )
        {
            return new DefaultFrame( this.ReferencePosition + distanceDelta );
        }

        public Vector3 InverseTransformPosition( Vector3Large globalPosition )
        {
            return new Vector3( (float)(globalPosition.x - ReferencePosition.x), (float)(globalPosition.y - ReferencePosition.y), (float)(globalPosition.z - ReferencePosition.z) );
        }
        public Vector3Large TransformPosition( Vector3 localPosition )
        {
            return ReferencePosition + localPosition;
        }
    }
}
