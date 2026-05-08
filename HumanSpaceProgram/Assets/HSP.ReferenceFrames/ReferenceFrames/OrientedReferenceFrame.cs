using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point, and with a given orientation. <br/>
    /// The frame is at rest. This class is immutable.
    /// </summary>
    public sealed class OrientedReferenceFrame : IReferenceFrame
    {
        public double ReferenceUT { get; }

        public Vector3Dbl Position => _position;
        public QuaternionDbl Rotation => _rotation;

        private readonly Vector3Dbl _position;
        private readonly QuaternionDbl _rotation;
        private readonly QuaternionDbl _inverseRotation;

        public OrientedReferenceFrame( double referenceUT, Vector3Dbl center, QuaternionDbl rotation )
        {
            this.ReferenceUT = referenceUT;
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );
        }

        public IReferenceFrame AtUT( double ut )
        {
            return new OrientedReferenceFrame( ut, _position, _rotation );
        }


        public KinematicState TransformState( in KinematicState localState )
        {
            return new KinematicState( null )
            {
                Position = Vector3Dbl.Add( _rotation * localState.Position, _position ),
                Rotation = _rotation * localState.Rotation,
                Velocity = _rotation * localState.Velocity,
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
                Velocity = _inverseRotation * globalState.Velocity,
                AngularVelocity = _inverseRotation * globalState.AngularVelocity,
                Acceleration = _inverseRotation * globalState.Acceleration,
                AngularAcceleration = _inverseRotation * globalState.AngularAcceleration
            };
        }

        public override string ToString()
        {
            return $"OrientedReferenceFrame( UT={ReferenceUT}, Pos={Position}, Rot={Rotation} )";
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

        [MapsInheritingFrom( typeof( OrientedReferenceFrame ) )]
        public static IDescriptor OrientedReferenceFrameMapping()
        {
            return new MemberwiseDescriptor<OrientedReferenceFrame>()
                .WithReadonlyMember( "reference_ut", o => o.ReferenceUT )
                .WithReadonlyMember( "position", o => o._position )
                .WithReadonlyMember( "rotation", o => o._rotation )
                .WithFactory<double, Vector3Dbl, QuaternionDbl>( ( ut, pos, rot ) => new OrientedReferenceFrame( ut, pos, rot ),
                    "reference_ut", "position", "rotation" );
        }
    }
}