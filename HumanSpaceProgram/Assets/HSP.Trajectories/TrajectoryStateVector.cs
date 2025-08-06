using System;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents a snapshot of a body's trajectory.
    /// </summary>
    public readonly struct TrajectoryStateVector : IEquatable<TrajectoryStateVector>
    {
#warning TODO - rotation as well
        public Vector3Dbl AbsolutePosition { get; }
        public Vector3Dbl AbsoluteVelocity { get; }
        public Vector3Dbl AbsoluteAcceleration { get; }
        public double Mass { get; }

        public TrajectoryStateVector( Vector3Dbl absolutePosition, Vector3Dbl absoluteVelocity, Vector3Dbl absoluteAcceleration, double mass )
        {
            this.AbsolutePosition = absolutePosition;
            this.AbsoluteVelocity = absoluteVelocity;
            this.AbsoluteAcceleration = absoluteAcceleration;
            this.Mass = mass;
        }

        public TrajectoryStateVector( Vector3Dbl absolutePosition, Vector3Dbl absoluteVelocity, double mass )
        {
            this.AbsolutePosition = absolutePosition;
            this.AbsoluteVelocity = absoluteVelocity;
            this.AbsoluteAcceleration = Vector3Dbl.zero;
            this.Mass = mass;
        }

        public bool Equals( TrajectoryStateVector other )
        {
            return this.AbsolutePosition == other.AbsolutePosition
                && this.AbsoluteVelocity == other.AbsoluteVelocity
                // && this.AbsoluteAcceleration == other.AbsoluteAcceleration
                && this.Mass == other.Mass;
        }

        public static TrajectoryStateVector Lerp( TrajectoryStateVector a, TrajectoryStateVector b, double t )
        {
            return new TrajectoryStateVector(
                Vector3Dbl.Lerp( a.AbsolutePosition, b.AbsolutePosition, t ),
                Vector3Dbl.Lerp( a.AbsoluteVelocity, b.AbsoluteVelocity, t ),
                Vector3Dbl.Lerp( a.AbsoluteAcceleration, b.AbsoluteAcceleration, t ),
                MathD.Lerp( a.Mass, b.Mass, t )
                );
        }
    }

    public static class IReferenceFrameTransform_Ex
    {
        public static TrajectoryStateVector GetBodyState( this ITrajectoryTransform self )
        {
            return new TrajectoryStateVector( 
                self.ReferenceFrameTransform.AbsolutePosition,
                self.ReferenceFrameTransform.Velocity,
                self.ReferenceFrameTransform.Acceleration,
                self.PhysicsTransform.Mass
                );
        }
    }
}