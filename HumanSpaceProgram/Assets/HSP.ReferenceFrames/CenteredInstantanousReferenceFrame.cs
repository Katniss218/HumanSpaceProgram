
using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame aligned with the AIRF frame, and shifted (offset) by a set distance. The non-inertial terms are instantanous and constant. This class is immutable.
    /// </summary>
    public sealed class CenteredInstantanousReferenceFrame : INonInertialReferenceFrame
    {
        private readonly Vector3Dbl _position;

        private readonly Vector3Dbl _velocity;
        private readonly Vector3Dbl _acceleration;
        private readonly Vector3Dbl _angularVelocity;
        private readonly Vector3Dbl _angularAcceleration;

        public CenteredInstantanousReferenceFrame( Vector3Dbl center, Vector3Dbl velocity, Vector3Dbl acceleration, Vector3Dbl angularVelocity, Vector3Dbl angularAcceleration )
        {
            this._position = center;

            this._velocity = velocity;
            this._acceleration = acceleration;
            this._angularVelocity = angularVelocity;
            this._angularAcceleration = angularAcceleration;
        }

        public IReferenceFrame Shift( Vector3Dbl airfDistanceDelta )
        {
            // calculate the new rotation/acceleration/etc terms for the new frame, so that the fictitious force is still correct.

            throw new NotImplementedException();
        }

        public Vector3Dbl InverseTransformPosition( Vector3Dbl airfPosition )
        {
            return Vector3Dbl.Subtract( airfPosition, _position );
        }

        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( localPosition, _position );
        }

        public Vector3 InverseTransformDirection( Vector3 globalDirection )
        {
            return globalDirection;
        }

        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return localDirection;
        }

        public QuaternionDbl InverseTransformRotation( QuaternionDbl globalRotation )
        {
            return globalRotation;
        }

        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return localRotation;
        }

        //  --

        public Vector3Dbl TransformVelocity( Vector3Dbl localVelocity )
        {
            return localVelocity;
        }

        public Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity )
        {
            return localAngularVelocity;
        }

        //  --

        public Vector3Dbl InverseTransformVelocity( Vector3Dbl globalVelocity )
        {
            return Vector3Dbl.Subtract( globalVelocity, _velocity );
        }

        public Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl globalAngularVelocity )
        {
            return Vector3Dbl.Subtract( globalAngularVelocity, _angularVelocity );
        }

        //  --

        public Vector3Dbl InverseTransformAcceleration( Vector3Dbl globalAcceleration )
        {
            return Vector3Dbl.Subtract( globalAcceleration, _acceleration );
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

            // handle angular acceleration when the frame's angular velocity changes.
        }

        public Vector3Dbl GetFictitiousAngularAcceleration( Vector3Dbl localPosition, Vector3Dbl localAngularVelocity )
        {
            // If not accounted for, the object would pick up rotational velocity from the frame.
            return (-_angularAcceleration);
        }
    }
}