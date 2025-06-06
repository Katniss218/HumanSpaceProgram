﻿using HSP.Trajectories;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Trajectories
{
    /// <summary>
    /// A trajectory that follows a newtonian gravitational field.
    /// </summary>
    public class NewtonianOrbit : ITrajectory
    {
        private double _cachedUpToUT => UT;

        // cache could work as an array of arrays and their dividing time points. Possibly halving the distance.

        //private List<Vector3Dbl> _cachedPositions;
        //private List<Vector3Dbl> _cachedVelocities;

        public double UT { get; private set; }

        private Vector3Dbl _currentPosition;
        private Vector3Dbl _currentVelocity;
        private Vector3Dbl _currentAcceleration;

        public double Mass { get; set; }

        /// <param name="mass">The mass of the object represented by this trajectory.</param>
        public NewtonianOrbit( double ut, Vector3Dbl initialAbsolutePosition, Vector3Dbl initialAbsoluteVelocity, Vector3Dbl initialAbsoluteAcceleration, double mass )
        {
            this.UT = ut;
            this._currentPosition = initialAbsolutePosition;
            this._currentVelocity = initialAbsoluteVelocity;
            this._currentAcceleration = initialAbsoluteAcceleration;
            this.Mass = mass;
        }

        public TrajectoryBodyState GetCurrentState()
        {
            return new TrajectoryBodyState( _currentPosition, _currentVelocity, _currentAcceleration, Mass );
        }

        public void SetCurrentState( TrajectoryBodyState stateVector )
        {
            _currentPosition = stateVector.AbsolutePosition;
            _currentVelocity = stateVector.AbsoluteVelocity;
            _currentAcceleration = stateVector.AbsoluteAcceleration;
            Mass = stateVector.Mass;
        }

        public TrajectoryBodyState GetStateAtUT( double ut )
        {
            if( _cachedUpToUT < ut )
            {
                throw new ArgumentOutOfRangeException( $"The {nameof( NewtonianOrbit )} hasn't been cached for UT: '{ut}'." );
            }

            throw new NotImplementedException();
        }

        public OrbitalFrame GetCurrentOrbitalFrame()
        {
            return new OrbitalFrame( _currentVelocity.NormalizeToVector3(), -_currentAcceleration.NormalizeToVector3() );
        }

        public OrbitalFrame GetOrbitalFrameAtUT( double ut )
        {
            // Prograde -> towards velocity.
            // 'up' -> opposite of acceleration (gravity).

            if( _cachedUpToUT < ut )
            {
                throw new ArgumentOutOfRangeException( $"The {nameof( NewtonianOrbit )} hasn't been cached for UT: '{ut}'." );
            }

            throw new NotImplementedException();
        }

        public bool HasCacheForUT( double ut )
        {
            return false;
        }

        public void Step( IEnumerable<TrajectoryBodyState> attractors, double dt )
        {
            Vector3Dbl selfAbsolutePosition = this.GetCurrentState().AbsolutePosition;

            int i = 0;
            Vector3Dbl accSum = Vector3Dbl.zero;
            foreach( var attractor in attractors )
            {
                Vector3Dbl toBody = attractor.AbsolutePosition - selfAbsolutePosition;

                double distanceSq = toBody.sqrMagnitude;
                // Skip 'this', if 'this' is an attractor (for allocation reasons, the list of attractors is the same for every attractor, and will contain 'this'.)
                if( distanceSq == 0.0 )
                {
                    continue;
                }

                double accelerationMagnitude = PhysicalConstants.G * (attractor.Mass / distanceSq);
                accSum += toBody.normalized * accelerationMagnitude;
                i++;
            }

            _currentAcceleration = accSum;
            _currentVelocity += _currentAcceleration * dt;
            _currentPosition += _currentVelocity * dt;
            UT += dt;
        }

        [MapsInheritingFrom( typeof( NewtonianOrbit ) )]
        public static SerializationMapping NewtonianOrbitMapping()
        {
            return new MemberwiseSerializationMapping<NewtonianOrbit>()
                .WithReadonlyMember( "ut", o => o.UT )
                .WithReadonlyMember( "mass", o => o.Mass )
                .WithReadonlyMember( "position", o => o._currentPosition )
                .WithReadonlyMember( "velocity", o => o._currentVelocity )
                .WithFactory<double, double, Vector3Dbl, Vector3Dbl>( ( ut, mass, position, velocity ) => new NewtonianOrbit( ut, position, velocity, Vector3Dbl.zero, mass ) ); ;
        }
    }
}