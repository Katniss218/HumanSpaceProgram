using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;
            if( deltaTime == 0 )
                return this;

            Vector3Dbl newPos = _position;
            Integrate( ref newPos, _velocity, deltaTime );

            return new OrientedInertialReferenceFrame( ut, newPos, _rotation, _velocity );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static void Integrate( ref Vector3Dbl position, in Vector3Dbl velocity, double deltaTime )
        {
            position += velocity * deltaTime;
        }


        public KinematicState TransformState( in KinematicState localState )
        {
            return new KinematicState( null )
            {
                Position = Vector3Dbl.Add( _rotation * localState.Position, _position ),
                Rotation = _rotation * localState.Rotation,
                Velocity = Vector3Dbl.Add( _rotation * localState.Velocity, _velocity ),
                AngularVelocity = _rotation * localState.AngularVelocity,
                Acceleration = _rotation * localState.Acceleration,
                AngularAcceleration = _rotation * localState.AngularAcceleration
            };
        }

        public KinematicState InverseTransformState( in KinematicState globalState )
        {
            return new KinematicState( this )
            {
                Position = _inverseRotation * Vector3Dbl.Subtract( globalState.Position, _position ),
                Rotation = _inverseRotation * globalState.Rotation,
                Velocity = _inverseRotation * Vector3Dbl.Subtract( globalState.Velocity, _velocity ),
                AngularVelocity = _inverseRotation * globalState.AngularVelocity,
                Acceleration = _inverseRotation * globalState.Acceleration,
                AngularAcceleration = _inverseRotation * globalState.AngularAcceleration
            };
        }

        public override string ToString()
        {
            return $"OrientedInertialReferenceFrame( UT={ReferenceUT}, Pos={Position}, Rot={Rotation}, Vel={Velocity} )";
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

        [MapsInheritingFrom( typeof( OrientedInertialReferenceFrame ) )]
        public static IDescriptor OrientedInertialReferenceFrameMapping()
        {
            return new MemberwiseDescriptor<OrientedInertialReferenceFrame>()
                .WithReadonlyMember( "reference_ut", o => o.ReferenceUT )
                .WithReadonlyMember( "position", o => o._position )
                .WithReadonlyMember( "rotation", o => o._rotation )
                .WithReadonlyMember( "velocity", o => o._velocity )
                .WithFactory<double, Vector3Dbl, QuaternionDbl, Vector3Dbl>( ( ut, pos, rot, vel ) => new OrientedInertialReferenceFrame( ut, pos, rot, vel ),
                    "reference_ut", "position", "rotation", "velocity" );
        }
    }
}