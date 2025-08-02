using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Trajectories
{
    public interface IReadonlyTrajectorySimulator
    {
        TrajectoryBodyState GetCurrentStateVector( TrajectoryTransform trajectoryTransform );

        void SetCurrentStateVector( TrajectoryTransform trajectoryTransform, TrajectoryBodyState stateVector );
    }

    public class TrajectorySimulator : IReadonlyTrajectorySimulator
    {
        // Timestepper (simulator):
        private ITrajectoryIntegrator[] _attractors;
        private TrajectoryBodyState[] _currentAttractors;
        private TrajectoryBodyState[] _nextAttractors;
        private ITrajectoryIntegrator[] _followers;
        private TrajectoryBodyState[] _currentFollowers;
        private TrajectoryBodyState[] _nextFollowers;
        private double _ut;

        private double _step;

        // Calculated ephemerides:
        Ephemeris[] _attractorEphemerides;
        Ephemeris[] _followerEphemerides;

        protected virtual void Reset( Ephemeris[] attractorEphemerides, Ephemeris[] followerEphemerides, double ut )
        {
            // assume the ephemerides are the 'ground truth', sync the timestepper to them.
            throw new NotImplementedException();
        }

        public virtual TrajectoryBodyState GetCurrentStateVector( TrajectoryTransform trajectoryTransform )
        {
            throw new NotImplementedException();

        }

        public virtual void SetCurrentStateVector( TrajectoryTransform trajectoryTransform, TrajectoryBodyState stateVector )
        {
            throw new NotImplementedException();

            if( trajectoryTransform.IsAttractor )
            {
                // reset ephemerides for all.
            }
        }

        /// <summary>
        /// Prolongs the ephemerides up to the specified UT.
        /// </summary>
        public virtual void Simulate( double endUT )
        {
#warning TODO - ensure that the simulation runs long enough to update every ephemeris.

            // run forward or backward, depending on endUT
            // theoretical max length of the ephemeris is fixed

            while( _ut < endUT )
            {
                // prolong
                double minStep = double.MaxValue;
                for( int i = 0; i < _currentAttractors.Length; i++ )
                {
                    var body = _attractors[i];
                    double step = body.Step( _step, _currentAttractors[i], accelerationProviders, out _nextAttractors[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                for( int i = 0; i < _currentFollowers.Length; i++ )
                {
                    var body = _attractors[i];
                    double step = body.Step( _step, _currentFollowers, accelerationProviders, out _nextFollowers[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                _ut += _step;
                _step = minStep;

                // when ran far enough, store the points as ephemerides in the corresponding ephemeris structs.
                for( int i = 0; i < _attractorEphemerides.Length; i++ )
                {
                    if( _ut > _attractorEphemerides[i].LastPoint + _attractorEphemerides[i].TimeResolution )
                    {
                        _attractorEphemerides[i].SetClosestPoint( _ut, _currentAttractors[i] );
                    }
                }



                var temp = _currentAttractors;
                _currentAttractors = _nextAttractors;
                _nextAttractors = temp;

                temp = _currentFollowers;
                _currentFollowers = _nextFollowers;
                _nextFollowers = temp;
            }

            _ut = endUT; // Setting to the actual value prevents accumulation of small precision errors due to repeated addition.
        }
    }
}