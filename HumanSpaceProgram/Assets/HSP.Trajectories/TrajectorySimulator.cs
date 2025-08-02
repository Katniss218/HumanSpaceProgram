using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Trajectories
{
    public class TrajectorySimulator
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

        private virtual void Reset( TrajectorySimulator other )
        {

        }

        /// <summary>
        /// Prolongs the ephemerides up to the specified UT.
        /// </summary>
        public virtual void Run( double endUT )
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

        //public List<ITrajectory> Attractors { get; } = new();
        //public List<ITrajectory> Followers { get; } = new();

        //private double _estimatedFuturePositionUTSpacing = 50.0; // 50 seconds between sample points
        //Vector3[][] _estimatedFollowerFuturePositions; // lower accuracy but available for future ut values, sampled at fixed UT offsets.


        //private double _simulationEndUT; // The last ut for which the sim has been computed.
        //public double EndUT => _simulationEndUT;

        //private double _ut;

        /// <summary>
        /// Simulates the trajectories all the way to the specified UT.
        /// </summary>
        /// <param name="endUT">The UT at which to finish the simulation.</param>
        public void Simulate( double endUT )
        {
            double TotalDelta = endUT - _simulationEndUT;
            int stepCount = 10; // TODO - dynamic step count.
            double dt = TotalDelta / (double)stepCount;

            // Copy the states to ensure that the simulation doesn't use values calculated for the next step on elements after the first element.
            TrajectoryBodyState[] attractorStates = new TrajectoryBodyState[Attractors.Count];

            for( int i = 0; i < stepCount; i++ )
            {
                for( int j = 0; j < Attractors.Count; j++ )
                {
                    attractorStates[j] = Attractors[j].GetCurrentState();
                }

                // attractors go first, because they attract each other.

                foreach( var attractor in Attractors )
                {
                    if( attractor.HasCacheForUT( _ut ) ) // attractor trajectory data has been computed up to specified UT (in case of keplerian, it's always available).
                    {
                        continue;
                    }

                    attractor.Step( attractorStates, dt ); // calculate the cached values for the next timestep based on current cached values of the system.
                }

                // followers go second.

                foreach( var follower in Followers )
                {
                    follower.Step( attractorStates, dt );
                }

                _ut += dt;
            }

            _ut = endUT;
            _simulationEndUT = _ut;
        }
    }
}