using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Represents the full kinematic state of a body at a specific point in universal time (UT).
    /// </summary>
    public struct KinematicState : IEquatable<KinematicState>
    {
        public readonly IReferenceFrame Frame; // null for global/absolute.

        public readonly double UT => Frame?.ReferenceUT ?? 0.0;

        public Vector3Dbl Position;
        public QuaternionDbl Rotation;

        public Vector3Dbl Velocity;
        public Vector3Dbl AngularVelocity;

        public Vector3Dbl Acceleration;
        public Vector3Dbl AngularAcceleration;

        public static readonly KinematicState AbsoluteIdentity = new KinematicState( null );
        public static KinematicState GetIdentity( IReferenceFrame frame = null ) => new KinematicState( frame );

        public KinematicState( IReferenceFrame frame )
        {
            Frame = frame;
            Position = Vector3Dbl.zero;
            Rotation = QuaternionDbl.identity;
            Velocity = Vector3Dbl.zero;
            AngularVelocity = Vector3Dbl.zero;
            Acceleration = Vector3Dbl.zero;
            AngularAcceleration = Vector3Dbl.zero;
        }

        public KinematicState( IReferenceFrame frame, Vector3Dbl position, QuaternionDbl rotation, Vector3Dbl velocity, Vector3Dbl angularVelocity, Vector3Dbl acceleration, Vector3Dbl angularAcceleration )
        {
            Frame = frame;
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            Acceleration = acceleration;
            AngularAcceleration = angularAcceleration;
        }

        public KinematicState InFrame( IReferenceFrame newFrame )
        {
            if( ReferenceEquals( Frame, newFrame ) || (Frame != null && Frame.Equals( newFrame )) )
                return this;

            KinematicState absoluteState = this;
            if( Frame != null )
            {
#warning TODO - use the proper frame transformation.
                absoluteState = Frame.TransformState( this );
            }

            if( newFrame != null )
            {
                return newFrame.InverseTransformState( absoluteState );
            }

            return absoluteState;
        }

        public bool Equals( KinematicState other )
        {
            return UT == other.UT &&
                   Position.Equals( other.Position ) &&
                   Rotation.Equals( other.Rotation ) &&
                   Velocity.Equals( other.Velocity ) &&
                   AngularVelocity.Equals( other.AngularVelocity ) &&
                   Acceleration.Equals( other.Acceleration ) &&
                   AngularAcceleration.Equals( other.AngularAcceleration );
        }

        public override bool Equals( object obj )
        {
            return obj is KinematicState other && Equals( other );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = UT.GetHashCode();
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
                hashCode = (hashCode * 397) ^ Velocity.GetHashCode();
                hashCode = (hashCode * 397) ^ AngularVelocity.GetHashCode();
                hashCode = (hashCode * 397) ^ Acceleration.GetHashCode();
                hashCode = (hashCode * 397) ^ AngularAcceleration.GetHashCode();
                return hashCode;
            }
        }
    }
}