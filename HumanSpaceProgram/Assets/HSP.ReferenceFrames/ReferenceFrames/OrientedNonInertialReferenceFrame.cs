using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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

        public OrientedNonInertialReferenceFrame( double referenceUT, Vector3Dbl center, QuaternionDbl rotation, Vector3Dbl velocity, Vector3Dbl angularVelocity )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );

            this._velocity = velocity;
            this._angularVelocity = angularVelocity;

            this._acceleration = Vector3Dbl.zero;
            this._angularAcceleration = Vector3Dbl.zero;
        }

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

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;
            if( deltaTime == 0 )
                return this;

            Vector3Dbl newPos = _position;
            QuaternionDbl newRot = _rotation;
            Vector3Dbl newVelocity = _velocity;
            Vector3Dbl newAngularVelocity = _angularVelocity;
            Integrate( ref newPos, ref newRot, ref newVelocity, ref newAngularVelocity, _acceleration, _angularAcceleration, deltaTime );

            return new OrientedNonInertialReferenceFrame( ut, newPos, newRot, newVelocity, newAngularVelocity, _acceleration, _angularAcceleration );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static void Integrate(
            ref Vector3Dbl position, ref QuaternionDbl rotation,
            ref Vector3Dbl velocity, ref Vector3Dbl angularVelocity,
            in Vector3Dbl acceleration, in Vector3Dbl angularAcceleration,
            double deltaTime )
        {
            if( deltaTime == 0 )
                return;

            // 1. Position integration (using existing velocity and constant acceleration)
            position += (velocity * deltaTime) + (0.5 * acceleration * (deltaTime * deltaTime));

            // 2. Rotation integration (using existing angular velocity and constant angular acceleration)
            var angVelMag = angularVelocity.magnitude;
            if( angVelMag > 1e-12 )
            {
                var angVelRot = QuaternionDbl.AngleAxis( angVelMag * deltaTime * 57.29577951308232, angularVelocity );
                rotation = angVelRot * rotation;
            }

            var angAccMag = angularAcceleration.magnitude;
            if( angAccMag > 1e-12 )
            {
                var angAccRot = QuaternionDbl.AngleAxis( 0.5 * angAccMag * (deltaTime * deltaTime) * 57.29577951308232, angularAcceleration );
                rotation = angAccRot * rotation;
            }

            // 3. Velocity integration (constant acceleration)
            velocity += acceleration * deltaTime;
            angularVelocity += angularAcceleration * deltaTime;
        }



        public KinematicState TransformState( in KinematicState localState )
        {
            return new KinematicState( null )
            {
                Position = Vector3Dbl.Add( _rotation * localState.Position, _position ),
                Rotation = _rotation * localState.Rotation,
                Velocity = Vector3Dbl.Add( _rotation * localState.Velocity, _velocity ),
                AngularVelocity = Vector3Dbl.Add( _rotation * localState.AngularVelocity, _angularVelocity ),
                Acceleration = Vector3Dbl.Add( _rotation * localState.Acceleration, _acceleration ),
                AngularAcceleration = Vector3Dbl.Add( _rotation * localState.AngularAcceleration, _angularAcceleration )
            };
        }

        public KinematicState InverseTransformState( in KinematicState globalState )
        {
            return new KinematicState( this )
            {
                Position = _inverseRotation * Vector3Dbl.Subtract( globalState.Position, _position ),
                Rotation = _inverseRotation * globalState.Rotation,
                Velocity = _inverseRotation * Vector3Dbl.Subtract( globalState.Velocity, _velocity ),
                AngularVelocity = _inverseRotation * Vector3Dbl.Subtract( globalState.AngularVelocity, _angularVelocity ),
                Acceleration = _inverseRotation * Vector3Dbl.Subtract( globalState.Acceleration, _acceleration ),
                AngularAcceleration = _inverseRotation * Vector3Dbl.Subtract( globalState.AngularAcceleration, _angularAcceleration )
            };
        }

        public Vector3Dbl GetTangentialVelocity( Vector3Dbl localPosition )
        {
            // Since the output is in absolute space, the order matters here.
            localPosition = _rotation * localPosition;

            return Vector3Dbl.Cross( AngularVelocity, localPosition );
        }

        public Vector3Dbl GetFicticiousAcceleration( Vector3Dbl localPosition, Vector3Dbl localVelocity )
        {
            // centrifugal acceleration.
            Vector3Dbl result = -Vector3Dbl.Cross( _angularVelocity, Vector3Dbl.Cross( _angularVelocity, localPosition ) );

            // coriolis acceleration.
            result -= 2.0 * Vector3Dbl.Cross( _angularVelocity, localVelocity );

            // euler acceleration.
            result -= Vector3Dbl.Cross( _angularAcceleration, localPosition );

            // linear acceleration.
            result -= _acceleration;

            return result;
        }

        public Vector3Dbl GetFictitiousAngularAcceleration( Vector3Dbl localPosition, Vector3Dbl localAngularVelocity )
        {
            // If not accounted for, the object would pick up rotational velocity from the frame.
            Vector3Dbl result = -_angularAcceleration;

            // Result of the frame's angular velocity axis not matching the object's.
            result -= Vector3Dbl.Cross( _angularVelocity, localAngularVelocity );

            return result;
        }

        public override string ToString()
        {
            return $"OrientedNonInertialReferenceFrame( UT={ReferenceUT}, Pos={Position}, Rot={Rotation}, Vel={Velocity}, AngVel={AngularVelocity}, Acc={Acceleration}, AngAcc={AngularAcceleration} )";
        }

        public bool Equals( IReferenceFrame other )
        {
            if( other == null )
                return false;

            var state = KinematicState.AbsoluteIdentity;
            var otherState = other.TransformState( state );
            var thisState = this.TransformState( state );

            return otherState.Equals( thisState );
        }
        public bool EqualsIgnoreUT( IReferenceFrame other )
        {
            if( other == null )
                return false;

            IReferenceFrame otherNormalizedUT = other.AtUT( this.ReferenceUT );

            var state = KinematicState.AbsoluteIdentity;
            var otherState = otherNormalizedUT.TransformState( state );
            var thisState = this.TransformState( state );

            return otherState.Equals( thisState );
        }

        [MapsInheritingFrom( typeof( OrientedNonInertialReferenceFrame ) )]
        public static IDescriptor OrientedNonInertialReferenceFrameMapping()
        {
            return new MemberwiseDescriptor<OrientedNonInertialReferenceFrame>()
                .WithReadonlyMember( "reference_ut", o => o.ReferenceUT )
                .WithReadonlyMember( "position", o => o._position )
                .WithReadonlyMember( "rotation", o => o._rotation )
                .WithReadonlyMember( "velocity", o => o._velocity )
                .WithReadonlyMember( "angular_velocity", o => o._angularVelocity )
                .WithReadonlyMember( "acceleration", o => o._acceleration )
                .WithReadonlyMember( "angular_acceleration", o => o._angularAcceleration )
                .WithFactory<double, Vector3Dbl, QuaternionDbl, Vector3Dbl, Vector3Dbl, Vector3Dbl, Vector3Dbl>( ( ut, pos, rot, vel, angVel, acc, angAcc ) => new OrientedNonInertialReferenceFrame( ut, pos, rot, vel, angVel, acc, angAcc ),
                    "reference_ut", "position", "rotation", "velocity", "angular_velocity", "accceleration", "angular_acceleration" );
        }
    }
}