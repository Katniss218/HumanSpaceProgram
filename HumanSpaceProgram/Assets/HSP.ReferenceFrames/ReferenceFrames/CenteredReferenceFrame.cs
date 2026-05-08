using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point. <br/>
    /// The frame is at rest. This class is immutable.
    /// </summary>
    public sealed class CenteredReferenceFrame : IReferenceFrame
    {
        public double ReferenceUT { get; private set; }

        public Vector3Dbl Position => _position;

        private readonly Vector3Dbl _position;

        public CenteredReferenceFrame( double referenceUT, Vector3Dbl center )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
        }

        public IReferenceFrame AtUT( double ut )
        {
            return new CenteredReferenceFrame( ut, _position );
        }


        public KinematicState TransformState( in KinematicState localState )
        {
            return new KinematicState( null )
            {
                Position = Vector3Dbl.Add( _position, localState.Position ),
                Rotation = localState.Rotation,
                Velocity = localState.Velocity,
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
                Velocity = globalState.Velocity,
                AngularVelocity = globalState.AngularVelocity,
                Acceleration = globalState.Acceleration,
                AngularAcceleration = globalState.AngularAcceleration
            };
        }

        public override string ToString()
        {
            return $"CenteredReferenceFrame( UT={ReferenceUT}, Pos={Position} )";
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

        [MapsInheritingFrom( typeof( CenteredReferenceFrame ) )]
        public static IDescriptor CenteredReferenceFrameMapping()
        {
            return new MemberwiseDescriptor<CenteredReferenceFrame>()
                .WithReadonlyMember( "reference_ut", o => o.ReferenceUT )
                .WithReadonlyMember( "position", o => o._position )
                .WithFactory<double, Vector3Dbl>( ( ut, pos ) => new CenteredReferenceFrame( ut, pos ),
                    "reference_ut", "position" );
        }
    }
}