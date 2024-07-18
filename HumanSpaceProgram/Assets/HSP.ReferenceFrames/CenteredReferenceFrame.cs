using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame aligned with the AIRF frame, and shifted (offset) by a certain amount. This class is immutable.
    /// </summary>
    public sealed class CenteredReferenceFrame : IReferenceFrame
    {
        private Vector3Dbl _position;

        public CenteredReferenceFrame( Vector3Dbl center )
        {
            this._position = center;
        }

        public IReferenceFrame Shift( Vector3Dbl airfDistanceDelta )
        {
            return new CenteredReferenceFrame( this._position + airfDistanceDelta );
        }

        public Vector3Dbl InverseTransformPosition( Vector3Dbl airfPosition )
        {
            return Vector3Dbl.Subtract( airfPosition, _position );
        }

        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( _position, localPosition );
        }

        public Vector3 InverseTransformDirection( Vector3 airfDirection )
        {
            return airfDirection;
        }

        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return localDirection;
        }

        public QuaternionDbl InverseTransformRotation( QuaternionDbl airfRotation )
        {
            return airfRotation;
        }

        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return localRotation;
        }
    }
}