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
        Vector3Dbl _referenceAirfPosition; // This could be a double precision matrix (for frames that implement rotation and scale)

        public OffsetReferenceFrame( Vector3Dbl referenceAIRFPosition )
        {
            this._referenceAirfPosition = referenceAIRFPosition;
        }

        public IReferenceFrame Shift( Vector3Dbl distanceDelta )
        {
            return new OffsetReferenceFrame( this._referenceAirfPosition + distanceDelta );
        }

        public Vector3 InverseTransformPosition( Vector3Dbl airfPosition )
        {
            return new Vector3( (float)(airfPosition.x - _referenceAirfPosition.x), (float)(airfPosition.y - _referenceAirfPosition.y), (float)(airfPosition.z - _referenceAirfPosition.z) );
        }

        public Vector3Dbl TransformPosition( Vector3 localPosition )
        {
            return Vector3Dbl.Add( _referenceAirfPosition, localPosition );
        }
        
        public Vector3 InverseTransformVector( Vector3 airfDirection )
        {
            return airfDirection;
        }

        public Vector3 TransformVector( Vector3 localDirection )
        {
            return localDirection;
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
