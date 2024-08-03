using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point, and with a given orientation. This class is immutable.
    /// </summary>
    public sealed class OrientedReferenceFrame : IReferenceFrame
    {
        private readonly Vector3Dbl _position;
        private readonly QuaternionDbl _rotation;
        private readonly QuaternionDbl _inverseRotation;

        public OrientedReferenceFrame( Vector3Dbl center, QuaternionDbl rotation )
        {
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );
        }

        public IReferenceFrame Shift( Vector3Dbl airfDistanceDelta )
        {
            return new OrientedReferenceFrame( this._position + airfDistanceDelta, this._rotation );
        }


        public Vector3Dbl InverseTransformPosition( Vector3Dbl absolutePosition )
        {
            return _inverseRotation * Vector3Dbl.Subtract( absolutePosition, _position );
        }
        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return _rotation * Vector3Dbl.Subtract( localPosition, _position );
        }


        public Vector3 InverseTransformDirection( Vector3 absoluteDirection )
        {
            return (Vector3)(_inverseRotation * absoluteDirection);
        }
        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return (Vector3)(_rotation * localDirection);
        }


        public QuaternionDbl InverseTransformRotation( QuaternionDbl airfRotation )
        {
            return _inverseRotation * airfRotation;
        }
        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return _rotation * localRotation;
        }


        public Vector3Dbl TransformVelocity( Vector3Dbl localVelocity )
        {
            return _rotation * localVelocity;
        }
        public Vector3Dbl InverseTransformVelocity( Vector3Dbl globalVelocity )
        {
            return _inverseRotation * globalVelocity;
        }


        public Vector3Dbl TransformAcceleration( Vector3Dbl localAcceleration )
        {
            return _rotation * localAcceleration;
        }
        public Vector3Dbl InverseTransformAcceleration( Vector3Dbl absoluteAcceleration )
        {
            return _inverseRotation * absoluteAcceleration;
        }


        public Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity )
        {
            return _rotation * localAngularVelocity;
        }
        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return _inverseRotation * globalAngularVelocity;
        }


        public Vector3Dbl TransformAngularAcceleration( Vector3Dbl localAngularAcceleration )
        {
            return _rotation * localAngularAcceleration;
        }
        public Vector3Dbl InverseTransformAngularAcceleration( Vector3Dbl absoluteAngularAcceleration )
        {
            return _inverseRotation * absoluteAngularAcceleration;
        }
    }
}