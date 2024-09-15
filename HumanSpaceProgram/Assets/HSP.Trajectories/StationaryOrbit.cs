using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// A trajectory that remains stationary.
    /// </summary>
    public class StationaryOrbit : ITrajectory
    {
        private Vector3Dbl _currentPosition;
        private QuaternionDbl _rotation;

        public double UT { get; private set; }
        public double Mass { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ut"></param>
        /// <param name="absolutePosition"></param>
        /// <param name="absoluteRotation"></param>
        /// <param name="mass">The mass of the object represented by this trajectory.</param>
        public StationaryOrbit( double ut, Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation, double mass )
        {
            this.UT = ut;
            this._currentPosition = absolutePosition;
            this._rotation = absoluteRotation;
            this.Mass = mass;
        }

        public TrajectoryBodyState GetCurrentState()
        {
            return new TrajectoryBodyState( _currentPosition, Vector3Dbl.zero, Vector3Dbl.zero, Mass );
        }

        public void SetCurrentState( TrajectoryBodyState stateVector )
        {
            if( stateVector.AbsoluteVelocity != Vector3Dbl.zero )
                throw new ArgumentException( $"Velocity must be zero.", nameof( stateVector ) );
            if( stateVector.AbsoluteAcceleration != Vector3Dbl.zero )
                throw new ArgumentException( $"Acceleration must be zero.", nameof( stateVector ) );

            _currentPosition = stateVector.AbsolutePosition;
            Mass = stateVector.Mass;
        }

        public TrajectoryBodyState GetStateAtUT( double ut )
        {
            return GetCurrentState();
        }

        public OrbitalFrame GetCurrentOrbitalFrame()
        {
            return new OrbitalFrame( _rotation.NormalizeToQuaternion() );
        }

        public OrbitalFrame GetOrbitalFrameAtUT( double ut )
        {
            return GetCurrentOrbitalFrame();
        }

        public bool HasCacheForUT( double ut )
        {
            return true;
        }

        public void Step( IEnumerable<TrajectoryBodyState> attractors, double dt )
        {
            UT += dt;
            return; // Do nothing, since stationary is not moving.
        }
    }
}