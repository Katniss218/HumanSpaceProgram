using System;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents a snapshot of a body's trajectory.
    /// </summary>
    public readonly struct TrajectoryStateVector : IEquatable<TrajectoryStateVector>
    {
        public Vector3Dbl AbsolutePosition { get; }
        public Vector3Dbl AbsoluteVelocity { get; }
        public Vector3Dbl AbsoluteAcceleration { get; }
#warning TODO - rotations
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

        private static double InverseLerp( double a, double b, double value )
        {
            if( a != b )
            {
                return (value - a) / (b - a);
            }

            return 0f;
        }

        public bool EqualsIgnoreAcceleration( TrajectoryStateVector other )
        {
            return this.AbsolutePosition.Equals( other.AbsolutePosition ) &&
                   this.AbsoluteVelocity.Equals( other.AbsoluteVelocity ) &&
                   this.Mass == other.Mass;
        }

        public bool Equals( TrajectoryStateVector other )
        {
            return this.AbsolutePosition.Equals( other.AbsolutePosition ) &&
                   this.AbsoluteVelocity.Equals( other.AbsoluteVelocity ) &&
                   this.AbsoluteAcceleration.Equals( other.AbsoluteAcceleration ) &&
                   this.Mass == other.Mass;
        }

        public override bool Equals( object obj )
        {
            return obj is TrajectoryStateVector other && this.Equals( other );
        }

        public override string ToString()
        {
            return $"{nameof( TrajectoryStateVector )}( Position: {this.AbsolutePosition}, Velocity: {this.AbsoluteVelocity}, Acceleration: {this.AbsoluteAcceleration}, Mass: {this.Mass} )";
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
}