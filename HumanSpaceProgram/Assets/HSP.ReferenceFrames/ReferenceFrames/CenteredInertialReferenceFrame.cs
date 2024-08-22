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
        public Vector3Dbl AngularVelocity => _angularVelocity;

        private readonly Vector3Dbl _position;

        // Inertial terms
        private readonly Vector3Dbl _velocity;
        private readonly Vector3Dbl _angularVelocity;

        public CenteredInertialReferenceFrame( double referenceUT, Vector3Dbl center, Vector3Dbl velocity, Vector3Dbl angularVelocity )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;

            this._velocity = velocity;
            this._angularVelocity = angularVelocity;
        }

        public IReferenceFrame Shift( Vector3Dbl absolutePositionDelta )
        {
            return new CenteredInertialReferenceFrame( ReferenceUT, _position + absolutePositionDelta, _velocity, _angularVelocity );
        }

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;

            var newPos = _position + (_velocity * deltaTime);
            double angularVelocityMagnitude = _angularVelocity.magnitude;
            if( angularVelocityMagnitude == 0 )
            {
                return new CenteredInertialReferenceFrame( ut, newPos, _velocity, _angularVelocity );
            }
            else
            {
                var newRot = /* 0      + */ QuaternionDbl.AngleAxis( _angularVelocity.magnitude * 57.2957795131 * deltaTime, _angularVelocity );
                return new OrientedInertialReferenceFrame( ut, newPos, newRot, _velocity, _angularVelocity );
            }
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
            return Vector3Dbl.Subtract( localAngularVelocity, _angularVelocity );
        }
        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return Vector3Dbl.Subtract( globalAngularVelocity, _angularVelocity );
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