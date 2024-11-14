
using HSP.Time;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame aligned with the AIRF frame, and shifted (offset) by a set distance. <br/>
    /// The inertial and non-inertial terms are constant in time. This class is immutable.
    /// </summary>
    public sealed class OrientedNonInertialReferenceFrame : INonInertialReferenceFrame
    {
        public double ReferenceUT { get; }

        public Vector3Dbl Position => _position;
        public QuaternionDbl Rotation => _rotation;
        public Vector3Dbl Velocity => _velocity;
        public Vector3Dbl AngularVelocity => _angularVelocity;
        public Vector3Dbl Acceleration => _acceleration;
        public Vector3Dbl AngularAcceleration => _angularAcceleration;

        private readonly Vector3Dbl _position;
        private readonly QuaternionDbl _rotation;
        private readonly QuaternionDbl _inverseRotation;

        // Inertial terms
        private readonly Vector3Dbl _velocity;

        // Non-inertial terms
        private readonly Vector3Dbl _angularVelocity;
        private readonly Vector3Dbl _acceleration;
        private readonly Vector3Dbl _angularAcceleration;

        public OrientedNonInertialReferenceFrame( double referenceUT, Vector3Dbl center, QuaternionDbl rotation, Vector3Dbl velocity, Vector3Dbl angularVelocity, Vector3Dbl acceleration, Vector3Dbl angularAcceleration )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );

            this._velocity = velocity;
            this._angularVelocity = angularVelocity;

            this._acceleration = acceleration;
            this._angularAcceleration = angularAcceleration;
        }

        public IReferenceFrame Shift( Vector3Dbl absolutePositionDelta )
        {
            // calculate the new rotation/acceleration/etc terms for the new frame, so that the fictitious force is still correct.

            throw new NotImplementedException();
        }

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;

            var newPos = _position + (_velocity * deltaTime);
            var newRot = QuaternionDbl.AngleAxis( _angularVelocity.magnitude * 57.2957795131 * deltaTime, _angularVelocity ) * _rotation;
            //QuaternionDbl deltaRotation = QuaternionDbl.Euler( _angularVelocity * TimeManager.FixedDeltaTime * 57.2957795131 );
            //var newRot = deltaRotation * _rotation;
            return new OrientedNonInertialReferenceFrame( ut, newPos, newRot, _velocity, _angularVelocity, _acceleration, _angularAcceleration );

            throw new NotImplementedException();
        }


        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( _rotation * localPosition, _position );
        }
        public Vector3Dbl InverseTransformPosition( Vector3Dbl globalPosition )
        {
            return _inverseRotation * Vector3Dbl.Subtract( globalPosition, _position );
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


        public Vector3Dbl TransformAcceleration( Vector3Dbl localAcceleration )
        {
            return Vector3Dbl.Add( _rotation * localAcceleration, _acceleration );
        }

        public Vector3Dbl InverseTransformAcceleration( Vector3Dbl globalAcceleration )
        {
            return _inverseRotation * Vector3Dbl.Subtract( globalAcceleration, _acceleration );
        }


        public Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity )
        {
            return Vector3Dbl.Subtract( _rotation * localAngularVelocity, _angularVelocity );
        }
        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return _inverseRotation * Vector3Dbl.Subtract( globalAngularVelocity, _angularVelocity );
        }


        public Vector3Dbl TransformAngularAcceleration( Vector3Dbl localAngularAcceleration )
        {
            return Vector3Dbl.Add( _rotation * localAngularAcceleration, _angularAcceleration );
        }
        public Vector3Dbl InverseTransformAngularAcceleration( Vector3Dbl globalAngularAcceleration )
        {
            return _inverseRotation * Vector3Dbl.Subtract( globalAngularAcceleration, _angularAcceleration );
        }


        public Vector3Dbl GetTangentialVelocity( Vector3Dbl localPosition )
        {
            localPosition = _rotation * localPosition;
            return Vector3Dbl.Cross( AngularVelocity, localPosition );
        }

        public Vector3Dbl GetFicticiousAcceleration( Vector3Dbl localPosition, Vector3Dbl localVelocity )
        {
#warning TODO - handle the frame's own orientation (like with tangential velocity).
            // Everything is in local (including the returned values), so the orientation is irrelevant.

            // TODO - handle near-zeroes in the terms.

            var centrifugalAcc = Vector3Dbl.Cross( _angularVelocity, Vector3Dbl.Cross( _angularVelocity, localPosition ) );

            var coriolisAcc = -2.0 * Vector3Dbl.Cross( _angularVelocity, localVelocity );

            var eulerAcc = -Vector3Dbl.Cross( _angularAcceleration, localPosition );

            var linearAcc = -_acceleration;

            return (centrifugalAcc + coriolisAcc + eulerAcc + linearAcc);
        }

        public Vector3Dbl GetFictitiousAngularAcceleration( Vector3Dbl localPosition, Vector3Dbl localAngularVelocity )
        {
            // Everything is in local (including the returned values), so the orientation is irrelevant.

            // If not accounted for, the object would pick up rotational velocity from the frame.
            return (-_angularAcceleration);
        }

        public bool Equals( IReferenceFrame other )
        {
            if( other == null )
                return false;

            return other.TransformPosition( Vector3Dbl.zero ) == this._position
                && other.TransformRotation( QuaternionDbl.identity ) == this._rotation
                && other.TransformVelocity( Vector3Dbl.zero ) == this._velocity
                && other.TransformAngularVelocity( Vector3Dbl.zero ) == this._angularVelocity
                && other.TransformAcceleration( Vector3Dbl.zero ) == this._acceleration
                && other.TransformAngularAcceleration( Vector3Dbl.zero ) == this._angularAcceleration;
        }
        public bool EqualsIgnoreUT( IReferenceFrame other )
        {
            if( other == null )
                return false;

            IReferenceFrame otherNormalizedUT = other.AtUT( this.ReferenceUT );

            return otherNormalizedUT.TransformPosition( Vector3Dbl.zero ) == this._position
                && otherNormalizedUT.TransformRotation( QuaternionDbl.identity ) == this._rotation
                && otherNormalizedUT.TransformVelocity( Vector3Dbl.zero ) == this._velocity
                && otherNormalizedUT.TransformAngularVelocity( Vector3Dbl.zero ) == this._angularVelocity
                && otherNormalizedUT.TransformAcceleration( Vector3Dbl.zero ) == this._acceleration
                && otherNormalizedUT.TransformAngularAcceleration( Vector3Dbl.zero ) == this._angularAcceleration;
        }

        //[MapsInheritingFrom( typeof( OrientedNonInertialReferenceFrame ) )]
        public static SerializationMapping OrientedNonInertialReferenceFrameMapping()
        {
            throw new NotImplementedException();
        }
    }
}