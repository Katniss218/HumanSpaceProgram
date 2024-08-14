
using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point. <br/>
    /// The inertial and non-inertial terms are instantanous and constant. This class is immutable.
    /// </summary>
    public sealed class CenteredNonInertialReferenceFrame : INonInertialReferenceFrame
    {
        public Vector3Dbl Position => _position;
        public Vector3Dbl Velocity => _velocity;
        public Vector3Dbl AngularVelocity => _angularVelocity;
        public Vector3Dbl Acceleration => _acceleration;
        public Vector3Dbl AngularAcceleration => _angularAcceleration;

        private readonly Vector3Dbl _position;

        // Inertial terms
        private readonly Vector3Dbl _velocity;
        private readonly Vector3Dbl _angularVelocity;

        // Non-inertial terms
        private readonly Vector3Dbl _acceleration;
        private readonly Vector3Dbl _angularAcceleration;

        public CenteredNonInertialReferenceFrame( Vector3Dbl center, Vector3Dbl velocity, Vector3Dbl angularVelocity, Vector3Dbl acceleration, Vector3Dbl angularAcceleration )
        {
            this._position = center;

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

        public IReferenceFrame AddUT( double ut )
        {
            throw new NotImplementedException();
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
    }
}