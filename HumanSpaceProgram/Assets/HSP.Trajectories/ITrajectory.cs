using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    public struct TrajectoryBodyState
    {
        public Vector3Dbl AbsolutePosition;
        public Vector3Dbl AbsoluteVelocity;
        public Vector3Dbl AbsoluteAcceleration;
        public double Mass;

        public TrajectoryBodyState( Vector3Dbl absolutePosition, Vector3Dbl absoluteVelocity, Vector3Dbl absoluteAcceleration, double mass )
        {
            this.AbsolutePosition = absolutePosition;
            this.AbsoluteVelocity = absoluteVelocity;
            this.AbsoluteAcceleration = absoluteAcceleration;
            this.Mass = mass;
        }
    }

    /// <summary>
    /// An arbitrary simulated trajectory. A.k.a. ephemeris, but more generalized.
    /// </summary>
    public interface ITrajectory
    {
        /// <summary>
        /// The current UT
        /// </summary>
        public double UT { get; }

        /// <summary>
        /// The mass of the body represented by this trajectory.
        /// </summary>
        double Mass { get; }

        /// <summary>
        /// Gets the position, velocity, acceleration, and mass for the point on the trajectory at the current UT.
        /// </summary>
        TrajectoryBodyState GetCurrentState();

        /// <summary>
        /// Sets the position, velocity, acceleration, and mass for the point on the trajectory at the current UT.
        /// </summary>
        /// <remarks>
        /// Resets any cache that might've been calculated for future UTs.
        /// </remarks>
        void SetCurrentState( TrajectoryBodyState stateVector );

        /// <summary>
        /// Gets the position, velocity, acceleration, and mass for the point on the trajectory at the specified UT.
        /// </summary>
        TrajectoryBodyState GetStateAtUT( double ut );

        /// <summary>
        /// Gets the orbital frame (orientation) for the point on the trajectory at the current UT.
        /// </summary>
        OrbitalFrame GetCurrentOrbitalFrame();

        /// <summary>
        /// Gets the orbital frame (orientation) for the point on the trajectory at the specified UT.
        /// </summary>
        OrbitalFrame GetOrbitalFrameAtUT( double ut );

        /// <summary>
        /// If true, the trajectory won't be simulated, and the GetStateVectorAtUT will be used instead.
        /// </summary>
        /// <param name="ut">The universal time to check for cache.</param>
        /// <returns>True if the trajectory has valid cache at the point <paramref name="ut"/>.</returns>
        bool HasCacheForUT( double ut );

        /// <summary>
        /// Advances the trajectory forward (or back) in time once.
        /// </summary>
        /// <param name="attractors">Every trajectory that acts like an attractor on this trajectory.</param>
        /// <param name="dt">The delta-time between the current time and the new time.</param>
        void Step( IEnumerable<TrajectoryBodyState> attractors, double dt );
    }
}