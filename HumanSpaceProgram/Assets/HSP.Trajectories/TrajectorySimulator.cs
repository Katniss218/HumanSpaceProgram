using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Trajectories
{
    public class TrajectorySimulator
    {
        public List<ITrajectory> Attractors { get; } = new();
        public List<ITrajectory> Followers { get; } = new();

        private double _estimatedFuturePositionUTSpacing = 50.0; // 50 seconds between sample points
        Vector3[][] _estimatedFollowerFuturePositions; // lower accuracy but available for future ut values, sampled at fixed UT offsets.


        private double _simulationEndUT; // The last ut for which the sim has been computed.
        public double EndUT => _simulationEndUT;

        private double _ut;

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