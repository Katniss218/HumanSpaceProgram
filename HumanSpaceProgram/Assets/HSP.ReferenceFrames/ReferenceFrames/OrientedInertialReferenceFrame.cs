using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point, and with a given orientation. <br/>
    /// The inertial terms are constant in time. This class is immutable.
    /// </summary>
    public sealed class OrientedInertialReferenceFrame : IReferenceFrame
    {
        public double ReferenceUT { get; }

        public Vector3Dbl Position => _position;
        public QuaternionDbl Rotation => _rotation;
        public Vector3Dbl Velocity => _velocity;

        private readonly Vector3Dbl _position;
        private readonly QuaternionDbl _rotation;
        private readonly QuaternionDbl _inverseRotation;

        // Inertial terms
        private readonly Vector3Dbl _velocity;

        public OrientedInertialReferenceFrame( double referenceUT, Vector3Dbl center, QuaternionDbl rotation, Vector3Dbl velocity )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );

            this._velocity = velocity;
        }

        public IReferenceFrame Shift( Vector3Dbl absolutePositionDelta )
        {
            return new OrientedInertialReferenceFrame( ReferenceUT, _position + absolutePositionDelta, _rotation, _velocity );
        }

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;

            var newPos = _position + (_velocity * deltaTime);
            //var newRot = QuaternionDbl.AngleAxis( _angularVelocity.magnitude * 57.2957795131 * deltaTime, _angularVelocity ) * _rotation;
            return new OrientedInertialReferenceFrame( ut, newPos, _rotation, _velocity );
        }


        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( _rotation * localPosition, _position );
        }
        public Vector3Dbl InverseTransformPosition( Vector3Dbl absolutePosition )
        {
            return _inverseRotation * Vector3Dbl.Subtract( absolutePosition, _position );
        }


        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return (Vector3)(_rotation * localDirection);
        }
        public Vector3 InverseTransformDirection( Vector3 absoluteDirection )
        {
            return (Vector3)(_inverseRotation * absoluteDirection);
        }


        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return _rotation * localRotation;
        }
        public QuaternionDbl InverseTransformRotation( QuaternionDbl airfRotation )
        {
            return _inverseRotation * airfRotation;
        }


        public Vector3Dbl TransformVelocity( Vector3Dbl localVelocity )
        {
            return Vector3Dbl.Add( _rotation * localVelocity, _velocity );
        }
        public Vector3Dbl InverseTransformVelocity( Vector3Dbl globalVelocity )
        {
            return _inverseRotation * Vector3Dbl.Subtract( globalVelocity, _velocity );
        }


        public Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity )
        {
            return _rotation * localAngularVelocity;
        }
        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return _inverseRotation * globalAngularVelocity;
        }


        public Vector3Dbl TransformAcceleration( Vector3Dbl localAcceleration )
        {
            return _rotation * localAcceleration;
        }
        public Vector3Dbl InverseTransformAcceleration( Vector3Dbl absoluteAcceleration )
        {
            return _inverseRotation * absoluteAcceleration;
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