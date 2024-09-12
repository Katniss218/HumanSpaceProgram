using HSP.ReferenceFrames;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// An arbitrary simulated trajectory. A.k.a. ephemeris, but more generalized.
    /// </summary>
    public interface ITrajectory
    {
        /// <summary>
        /// Adds instantaneous acceleration (velocity change) at the current UT.
        /// </summary>
        void AddVelocityChange( Vector3Dbl velocityChange );
        /// <summary>
        /// Adds instantaneous acceleration (velocity change) at the specified UT.
        /// </summary>
        void AddVelocityChangeAtUT( Vector3Dbl velocityChange, double ut );


        // Values are for the current simulation UT.

        Vector3Dbl AbsolutePosition { get; }
        Vector3Dbl AbsoluteVelocity { get; }
        Vector3Dbl AbsoluteAcceleration { get; }
        double Mass { get; }

        bool HasCacheForUT( double ut );

        void Step( IEnumerable<ITrajectory> attractors, double dt );

        OrbitalStateVector GetCurrentStateVector();
        void SetCurrentStateVector( OrbitalStateVector stateVector );
        OrbitalStateVector GetStateVectorAtUT( double ut );

        /// <summary>
        /// Calculates the state vector for the normalized time of the trajectory.
        /// </summary>
        /// <param name="t">Time t in [0..1], 0 at the first valid time, 1 at the last valid time.</param>
        /// <returns></returns>
        //OrbitalStateVector GetStateVector( float t );

        /// <summary>
        /// Calculates the orbital frame for the current UT.
        /// </summary>
        //OrbitalFrame GetCurrentOrbitalFrame();
        /// <summary>
        /// Calculates the orbital frame for the specified UT.
        /// </summary>
        //OrbitalFrame GetOrbitalFrameAtUT( double ut );
    }
}