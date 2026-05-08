using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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
            if( deltaTime == 0 )
                return this;

            Vector3Dbl newPos = _position;
            Vector3Dbl newVelocity = _velocity;
            Vector3Dbl newAngularVelocity = _angularVelocity;

            var angVelMag = _angularVelocity.magnitude;
            var angAccMag = _angularAcceleration.magnitude;
            var hasAngularMotion = (angVelMag > 1e-12) || (angAccMag > 1e-12);

            if( hasAngularMotion )
            {
                QuaternionDbl newRot = QuaternionDbl.identity;
                Integrate( ref newPos, ref newRot, ref newVelocity, ref newAngularVelocity, _acceleration, _angularAcceleration, deltaTime );
                return new OrientedNonInertialReferenceFrame( ut, newPos, newRot, newVelocity, newAngularVelocity, _acceleration, _angularAcceleration );
            }
            else
            {
                Integrate( ref newPos, ref newVelocity, ref newAngularVelocity, _acceleration, _angularAcceleration, deltaTime );
                return new CenteredNonInertialReferenceFrame( ut, newPos, newVelocity, newAngularVelocity, _acceleration, _angularAcceleration );
            }
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

            // 1. Position integration
            position += (velocity * deltaTime) + (0.5 * acceleration * (deltaTime * deltaTime));

            // 2. Rotation integration
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

            // 3. Velocity integration
            velocity += acceleration * deltaTime;
            angularVelocity += angularAcceleration * deltaTime;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static void Integrate(
            ref Vector3Dbl position,
            ref Vector3Dbl velocity, ref Vector3Dbl angularVelocity,
            in Vector3Dbl acceleration, in Vector3Dbl angularAcceleration,
            double deltaTime )
        {
            if( deltaTime == 0 ) 
                return;

            position += (velocity * deltaTime) + (0.5 * acceleration * (deltaTime * deltaTime));
            velocity += acceleration * deltaTime;
            angularVelocity += angularAcceleration * deltaTime;
        }


        public KinematicState TransformState( in KinematicState localState )
        {
            return new KinematicState( null )
            {
                Position = Vector3Dbl.Add( localState.Position, _position ),
                Rotation = localState.Rotation,
                Velocity = Vector3Dbl.Add( localState.Velocity, _velocity ),
                AngularVelocity = Vector3Dbl.Add( localState.AngularVelocity, _angularVelocity ),
                Acceleration = Vector3Dbl.Add( localState.Acceleration, _acceleration ),
                AngularAcceleration = Vector3Dbl.Add( localState.AngularAcceleration, _angularAcceleration )
            };
        }

        public KinematicState InverseTransformState( in KinematicState globalState )
        {
            return new KinematicState( this )
            {
                Position = Vector3Dbl.Subtract( globalState.Position, _position ),
                Rotation = globalState.Rotation,
                Velocity = Vector3Dbl.Subtract( globalState.Velocity, _velocity ),
                AngularVelocity = Vector3Dbl.Subtract( globalState.AngularVelocity, _angularVelocity ),
                Acceleration = Vector3Dbl.Subtract( globalState.Acceleration, _acceleration ),
                AngularAcceleration = Vector3Dbl.Subtract( globalState.AngularAcceleration, _angularAcceleration )
            };
        }

        public Vector3Dbl GetTangentialVelocity( Vector3Dbl localPosition )
        {
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
            return $"CenteredNonInertialReferenceFrame( UT={ReferenceUT}, Pos={Position}, Vel={Velocity}, AngVel={AngularVelocity}, Acc={Acceleration}, AngAcc={AngularAcceleration} )";
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


        [MapsInheritingFrom( typeof( CenteredNonInertialReferenceFrame ) )]
        public static IDescriptor CenteredNonInertialReferenceFrameMapping()
        {
            return new MemberwiseDescriptor<CenteredNonInertialReferenceFrame>()
                .WithReadonlyMember( "reference_ut", o => o.ReferenceUT )
                .WithReadonlyMember( "position", o => o._position )
                .WithReadonlyMember( "velocity", o => o._velocity )
                .WithReadonlyMember( "angular_velocity", o => o._angularVelocity )
                .WithReadonlyMember( "acceleration", o => o._acceleration )
                .WithReadonlyMember( "angular_acceleration", o => o._angularAcceleration )
                .WithFactory<double, Vector3Dbl, Vector3Dbl, Vector3Dbl, Vector3Dbl, Vector3Dbl>( ( ut, pos, vel, angVel, acc, angAcc ) => new CenteredNonInertialReferenceFrame( ut, pos, vel, angVel, acc, angAcc ),
                    "reference_ut", "position", "velocity", "angular_velocity", "accceleration", "angular_acceleration" );
        }
    }
}