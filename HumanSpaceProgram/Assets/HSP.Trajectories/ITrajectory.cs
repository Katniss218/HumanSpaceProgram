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

#warning TODO - get/set the pos/vel/acc instead of a state vector maybe?
        OrbitalStateVector GetCurrentStateVector();

        void SetCurrentStateVector( OrbitalStateVector stateVector );

        OrbitalStateVector GetStateVectorAtUT( double ut );

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