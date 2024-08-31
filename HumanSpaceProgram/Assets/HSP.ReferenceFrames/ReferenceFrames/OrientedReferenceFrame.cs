using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point, and with a given orientation. <br/>
    /// The frame is at rest. This class is immutable.
    /// </summary>
    public sealed class OrientedReferenceFrame : IReferenceFrame
    {
        public double ReferenceUT { get; }

        public Vector3Dbl Position => _position;
        public QuaternionDbl Rotation => _rotation;

        private readonly Vector3Dbl _position;
        private readonly QuaternionDbl _rotation;
        private readonly QuaternionDbl _inverseRotation;

        public OrientedReferenceFrame( double referenceUT, Vector3Dbl center, QuaternionDbl rotation )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );
        }

        public IReferenceFrame Shift( Vector3Dbl absolutePositionDelta )
        {
            return new OrientedReferenceFrame( ReferenceUT, this._position + absolutePositionDelta, this._rotation );
        }

        public IReferenceFrame AtUT( double ut )
        {
            return this; // Reference frames are immutable, so this is allowed.
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
            return _rotation * localVelocity;
        }
        public Vector3Dbl InverseTransformVelocity( Vector3Dbl globalVelocity )
        {
            return _inverseRotation * globalVelocity;
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

        public bool Equals( IReferenceFrame other )
        {
            if( other == null )
                return false;

            return other.TransformPosition( Vector3Dbl.zero ) == this._position
                && other.TransformRotation( QuaternionDbl.identity ) == this._rotation
                && other.TransformVelocity( Vector3Dbl.zero ) == Vector3Dbl.zero
                && other.TransformAngularVelocity( Vector3Dbl.zero ) == Vector3Dbl.zero
                && other.TransformAcceleration( Vector3Dbl.zero ) == Vector3Dbl.zero
                && other.TransformAngularAcceleration( Vector3Dbl.zero ) == Vector3Dbl.zero;
        }

        public bool EqualsIgnoreUT( IReferenceFrame other )
        {
            if( other == null )
                return false;

            IReferenceFrame otherNormalizedUT = other.AtUT( this.ReferenceUT );

            return otherNormalizedUT.TransformPosition( Vector3Dbl.zero ) == this._position
                && otherNormalizedUT.TransformRotation( QuaternionDbl.identity ) == this._rotation
                && otherNormalizedUT.TransformVelocity( Vector3Dbl.zero ) == Vector3Dbl.zero
                && otherNormalizedUT.TransformAngularVelocity( Vector3Dbl.zero ) == Vector3Dbl.zero
                && otherNormalizedUT.TransformAcceleration( Vector3Dbl.zero ) == Vector3Dbl.zero
                && otherNormalizedUT.TransformAngularAcceleration( Vector3Dbl.zero ) == Vector3Dbl.zero;
        }
    }
}