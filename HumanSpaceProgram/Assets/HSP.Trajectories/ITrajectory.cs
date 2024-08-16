using System;
using UnityEngine;

namespace HSP.Trajectories
{
    /*public struct Continuous<T>
    {
        public T value;
        public Func<double, T> valueGetter;

        public static implicit operator Continuous<T>( T value )
        {
            return new Continuous<T>() { value = value };
        }

        public static implicit operator Continuous<T>( Func<double, T> valueGetter )
        {
            return new Continuous<T>() { valueGetter = valueGetter };
        }
    }*/
    
    public interface ITrajectory
    {
        /// <summary>
        /// Adds instantaneous acceleration (velocity change) at the current UT.
        /// </summary>
        void AddAcceleration( Vector3Dbl acceleration );
        /// <summary>
        /// Adds instantaneous acceleration (velocity change) at the specified UT.
        /// </summary>
        void AddAccelerationAtUT( Vector3Dbl acceleration, double ut );

        OrbitalStateVector GetCurrentStateVector();
        void SetCurrentStateVector( OrbitalStateVector stateVector );
        OrbitalStateVector GetStateVectorAtUT( double ut );

        /// <summary>
        /// Calculates the state vector for the normalized time of the trajectory.
        /// </summary>
        /// <param name="t">Time t in [0..1], 0 at the first valid time, 1 at the last valid time.</param>
        /// <returns></returns>
        OrbitalStateVector GetStateVector( float t );

        /// <summary>
        /// Calculates the orbital frame for the current UT.
        /// </summary>
        OrbitalFrame GetCurrentOrbitalFrame();
        /// <summary>
        /// Calculates the orbital frame for the specified UT.
        /// </summary>
        OrbitalFrame GetOrbitalFrameAtUT( double ut );
    }
}