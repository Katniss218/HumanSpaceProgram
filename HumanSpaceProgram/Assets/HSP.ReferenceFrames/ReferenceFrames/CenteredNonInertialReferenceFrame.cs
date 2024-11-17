
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point. <br/>
    /// The inertial and non-inertial terms are constant in time. This class is immutable.
    /// </summary>
    public sealed class CenteredNonInertialReferenceFrame : INonInertialReferenceFrame
    {
        public double ReferenceUT { get; private set; }

        public Vector3Dbl Position => _position;
        public Vector3Dbl Velocity => _velocity;
        public Vector3Dbl AngularVelocity => _angularVelocity;
        public Vector3Dbl Acceleration => _acceleration;
        public Vector3Dbl AngularAcceleration => _angularAcceleration;

        private readonly Vector3Dbl _position;

        // Inertial terms
        private readonly Vector3Dbl _velocity;

        // Non-inertial terms
        private readonly Vector3Dbl _angularVelocity;
        private readonly Vector3Dbl _acceleration;
        private readonly Vector3Dbl _angularAcceleration;

        public CenteredNonInertialReferenceFrame( double referenceUT, Vector3Dbl center, Vector3Dbl velocity, Vector3Dbl angularVelocity )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;

            this._velocity = velocity;
            this._angularVelocity = angularVelocity;
        }
        
        public CenteredNonInertialReferenceFrame( double referenceUT, Vector3Dbl center, Vector3Dbl velocity, Vector3Dbl angularVelocity, Vector3Dbl acceleration, Vector3Dbl angularAcceleration )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;

            this._velocity = velocity;
            this._angularVelocity = angularVelocity;

            this._acceleration = acceleration;
            this._angularAcceleration = angularAcceleration;
        }

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;

            // new pos/rot consist of the component due to existing velocity and the component due to constant acceleration.
            var newPos = _position
                + (_velocity * deltaTime)
                + (0.5 * _acceleration * (deltaTime * deltaTime));
            var newRot =
                QuaternionDbl.AngleAxis( 0.5 * _angularAcceleration.magnitude * (deltaTime * deltaTime) * 57.29577951308232, _angularAcceleration )
                * QuaternionDbl.AngleAxis( _angularVelocity.magnitude * deltaTime * 57.29577951308232, _angularVelocity );

            Vector3Dbl newVelocity = _velocity + (_acceleration * deltaTime);
            Vector3Dbl newAngularVelocity = _angularVelocity + (_angularAcceleration * deltaTime);

            return new OrientedNonInertialReferenceFrame( ut, newPos, newRot, newVelocity, newAngularVelocity, _acceleration, _angularAcceleration );
        }


        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( localPosition, _position );
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
            return Vector3Dbl.Add( localVelocity, _velocity );
        }
        public Vector3Dbl InverseTransformVelocity( Vector3Dbl globalVelocity )
        {
            return Vector3Dbl.Subtract( globalVelocity, _velocity );
        }


        public Vector3Dbl TransformAcceleration( Vector3Dbl localAcceleration )
        {
            return Vector3Dbl.Add( localAcceleration, _acceleration );
        }

        public Vector3Dbl InverseTransformAcceleration( Vector3Dbl globalAcceleration )
        {
            return Vector3Dbl.Subtract( globalAcceleration, _acceleration );
        }


        public Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity )
        {
            return Vector3Dbl.Subtract( localAngularVelocity, _angularVelocity );
        }
        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return Vector3Dbl.Subtract( globalAngularVelocity, _angularVelocity );
        }


        public Vector3Dbl TransformAngularAcceleration( Vector3Dbl localAngularAcceleration )
        {
            return Vector3Dbl.Add( localAngularAcceleration, _angularAcceleration );
        }
        public Vector3Dbl InverseTransformAngularAcceleration( Vector3Dbl globalAngularAcceleration )
        {
            return Vector3Dbl.Subtract( globalAngularAcceleration, _angularAcceleration );
        }


        public Vector3Dbl GetTangentialVelocity( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Cross( AngularVelocity, localPosition );
        }

        public Vector3Dbl GetFicticiousAcceleration( Vector3Dbl localPosition, Vector3Dbl localVelocity )
        {
            // TODO - handle near-zeroes in the terms.

            var centrifugalAcc = Vector3Dbl.Cross( _angularVelocity, Vector3Dbl.Cross( _angularVelocity, localPosition ) );

            var coriolisAcc = -2.0 * Vector3Dbl.Cross( _angularVelocity, localVelocity );

            var eulerAcc = -Vector3Dbl.Cross( _angularAcceleration, localPosition );

            var linearAcc = -_acceleration;

            return (centrifugalAcc + coriolisAcc + eulerAcc + linearAcc);
        }

        public Vector3Dbl GetFictitiousAngularAcceleration( Vector3Dbl localPosition, Vector3Dbl localAngularVelocity )
        {
            // If not accounted for, the object would pick up rotational velocity from the frame.
            return (-_angularAcceleration);
        }

        public bool Equals( IReferenceFrame other )
        {
            if( other == null )
                return false;

            return other.TransformPosition( Vector3Dbl.zero ) == this._position
                && other.TransformRotation( QuaternionDbl.identity ) == QuaternionDbl.identity
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
                && otherNormalizedUT.TransformRotation( QuaternionDbl.identity ) == QuaternionDbl.identity
                && otherNormalizedUT.TransformVelocity( Vector3Dbl.zero ) == this._velocity
                && otherNormalizedUT.TransformAngularVelocity( Vector3Dbl.zero ) == this._angularVelocity
                && otherNormalizedUT.TransformAcceleration( Vector3Dbl.zero ) == this._acceleration
                && otherNormalizedUT.TransformAngularAcceleration( Vector3Dbl.zero ) == this._angularAcceleration;
        }

        //[MapsInheritingFrom( typeof( CenteredNonInertialReferenceFrame ) )]
        public static SerializationMapping CenteredNonInertialReferenceFrameMapping()
        {
#warning TODO - easier way to load memberwise objects that are immutable
            throw new NotImplementedException();
        }
    }
}