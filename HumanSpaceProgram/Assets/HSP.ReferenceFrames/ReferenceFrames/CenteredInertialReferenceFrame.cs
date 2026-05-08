using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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

        public IReferenceFrame AtUT( double ut )
        {
            double deltaTime = ut - ReferenceUT;
            if( deltaTime == 0 )
                return this;

            Vector3Dbl newPos = _position;
            Integrate( ref newPos, _velocity, deltaTime );
            return new CenteredInertialReferenceFrame( ut, newPos, _velocity );
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
                Position = Vector3Dbl.Add( _position, localState.Position ),
                Rotation = localState.Rotation,
                Velocity = Vector3Dbl.Add( _velocity, localState.Velocity ),
                AngularVelocity = localState.AngularVelocity,
                Acceleration = localState.Acceleration,
                AngularAcceleration = localState.AngularAcceleration
            };
        }

        public KinematicState InverseTransformState( in KinematicState globalState )
        {
            return new KinematicState( this )
            {
                Position = Vector3Dbl.Subtract( globalState.Position, _position ),
                Rotation = globalState.Rotation,
                Velocity = Vector3Dbl.Subtract( globalState.Velocity, _velocity ),
                AngularVelocity = globalState.AngularVelocity,
                Acceleration = globalState.Acceleration,
                AngularAcceleration = globalState.AngularAcceleration
            };
        }

        public override string ToString()
        {
            return $"CenteredInertialReferenceFrame( UT={ReferenceUT}, Pos={Position}, Vel={Velocity} )";
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

        [MapsInheritingFrom( typeof( CenteredInertialReferenceFrame ) )]
        public static IDescriptor CenteredInertialReferenceFrameMapping()
        {
            return new MemberwiseDescriptor<CenteredInertialReferenceFrame>()
                .WithReadonlyMember( "reference_ut", o => o.ReferenceUT )
                .WithReadonlyMember( "position", o => o._position )
                .WithReadonlyMember( "velocity", o => o._velocity )
                .WithFactory<double, Vector3Dbl, Vector3Dbl>( ( ut, pos, vel ) => new CenteredInertialReferenceFrame( ut, pos, vel ),
                    "reference_ut", "position", "velocity" );
        }
    }
}