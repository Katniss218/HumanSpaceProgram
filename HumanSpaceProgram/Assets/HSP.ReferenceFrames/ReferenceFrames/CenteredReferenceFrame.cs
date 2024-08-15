using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point. <br/>
    /// The frame is at rest. This class is immutable.
    /// </summary>
    public sealed class CenteredReferenceFrame : IReferenceFrame
    {
        public double ReferenceUT { get; }

        public Vector3Dbl Position => _position;

        private readonly Vector3Dbl _position;

        public CenteredReferenceFrame( double referenceUT, Vector3Dbl center )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
        }

        public IReferenceFrame Shift( Vector3Dbl absolutePositionDelta )
        {
            return new CenteredReferenceFrame( ReferenceUT, _position + absolutePositionDelta );
        }

        public IReferenceFrame AtUT( double ut )
        {
            return this; // Reference frames are immutable, so this is allowed.
        }


        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( _position, localPosition );
        }
        public Vector3Dbl InverseTransformPosition( Vector3Dbl globalPosition )
        {
            return Vector3Dbl.Subtract( globalPosition, _position );
        }


        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return localDirection;
        }
        public Vector3 InverseTransformDirection( Vector3 globalDirection )
        {
            return globalDirection;
        }


        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return localRotation;
        }
        public QuaternionDbl InverseTransformRotation( QuaternionDbl globalRotation )
        {
            return globalRotation;
        }


        public Vector3Dbl TransformVelocity( Vector3Dbl localVelocity )
        {
            return localVelocity;
        }
        public Vector3Dbl InverseTransformVelocity( Vector3Dbl absoluteVelocity )
        {
            return absoluteVelocity;
        }


        public Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity )
        {
            return localAngularVelocity;
        }
        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return globalAngularVelocity;
        }


        public Vector3Dbl TransformAcceleration( Vector3Dbl localAcceleration )
        {
            return localAcceleration;
        }
        public Vector3Dbl InverseTransformAcceleration( Vector3Dbl absoluteAcceleration )
        {
            return absoluteAcceleration;
        }


        public Vector3Dbl TransformAngularAcceleration( Vector3Dbl localAngularAcceleration )
        {
            return localAngularAcceleration;
        }
        public Vector3Dbl InverseTransformAngularAcceleration( Vector3Dbl absoluteAngularAcceleration )
        {
            return absoluteAngularAcceleration;
        }
    }
}