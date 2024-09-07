using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point. <br/>
    /// The inertial terms are constant in time. This class is immutable.
    /// </summary>
    public sealed class CenteredInertialReferenceFrame : IReferenceFrame
    {
        public double ReferenceUT { get; }

        public Vector3Dbl Position => _position;
        public Vector3Dbl Velocity => _velocity;

        private readonly Vector3Dbl _position;

        // Inertial terms
        private readonly Vector3Dbl _velocity;

        public CenteredInertialReferenceFrame( double referenceUT, Vector3Dbl center, Vector3Dbl velocity )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;

            this._velocity = velocity;
        }

        public IReferenceFrame Shift( Vector3Dbl absolutePositionDelta )
        {
            return new CenteredInertialReferenceFrame( ReferenceUT, _position + absolutePositionDelta, _velocity );
        }

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;
            if( deltaTime == 0 )
                return this;

            var newPos = _position + (_velocity * deltaTime);
            return new CenteredInertialReferenceFrame( ut, newPos, _velocity );
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
            return Vector3Dbl.Add( localVelocity, _velocity );
        }
        public Vector3Dbl InverseTransformVelocity( Vector3Dbl globalVelocity )
        {
            return Vector3Dbl.Subtract( globalVelocity, _velocity );
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

        public bool Equals( IReferenceFrame other )
        {
            if( other == null )
                return false;

            return other.TransformPosition( Vector3Dbl.zero ) == this._position
                && other.TransformRotation( QuaternionDbl.identity ) == QuaternionDbl.identity
                && other.TransformVelocity( Vector3Dbl.zero ) == this._velocity
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
                && otherNormalizedUT.TransformRotation( QuaternionDbl.identity ) == QuaternionDbl.identity
                && otherNormalizedUT.TransformVelocity( Vector3Dbl.zero ) == this._velocity
                && otherNormalizedUT.TransformAngularVelocity( Vector3Dbl.zero ) == Vector3Dbl.zero
                && otherNormalizedUT.TransformAcceleration( Vector3Dbl.zero ) == Vector3Dbl.zero
                && otherNormalizedUT.TransformAngularAcceleration( Vector3Dbl.zero ) == Vector3Dbl.zero;
        }
    }
}