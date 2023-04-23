using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.ReferenceFrames
{
    public class OffsetReferenceFrame : IReferenceFrame
    {
        // incomplete.
        // Instead of the "Vector3Dbl", store vectors as a combination of "bigint" and "float" maybe???
        // bigint for the large scale reference frame setting, and float for the fine detail.
        // integer could be also represented as multiplied by a large factor so that it acts like the upper bits of a larger number. And the float acts like the lower bits???
        // - the factor would be at most equal to half of the allowed floating origin radius.
        // - that way would disallow precise placement of the reference frame, we need probably around 10000 meters granularity at the least.

        public Vector3Dbl ReferenceAIRFPosition { get; private set; } // these could be a matrix mayhaps (+ rotation, and scale)?

        public OffsetReferenceFrame( Vector3Dbl referenceAIRFPosition )
        {
            this.ReferenceAIRFPosition = referenceAIRFPosition;
        }

        public IReferenceFrame Shift( Vector3Dbl distanceDelta )
        {
            return new OffsetReferenceFrame( this.ReferenceAIRFPosition + distanceDelta );
        }

        public Vector3 InverseTransformPosition( Vector3Dbl airfPosition )
        {
            return new Vector3( (float)(airfPosition.x - ReferenceAIRFPosition.x), (float)(airfPosition.y - ReferenceAIRFPosition.y), (float)(airfPosition.z - ReferenceAIRFPosition.z) );
        }

        public Vector3Dbl TransformPosition( Vector3 localPosition )
        {
            return Vector3Dbl.Add( ReferenceAIRFPosition, localPosition );
        }

        public Quaternion InverseTransformRotation( Quaternion airfRotation )
        {
            return airfRotation;
        }

        public Quaternion TransformRotation( Quaternion localRotation )
        {
            return localRotation;
        }
    }
}
