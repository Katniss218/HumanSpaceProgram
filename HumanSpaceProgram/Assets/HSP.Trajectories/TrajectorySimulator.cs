using System.Collections.Generic;
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

        private double _ut;

        /// <summary>
        /// Simulates the trajectories all the way to the specified UT.
        /// </summary>
        /// <param name="endUT">The UT at which to finish the simulation.</param>
        public void Simulate( double endUT )
        {
            double dt = 1.0 / 200;

            for( ; _ut < endUT; _ut += dt )
            {
                // attractors go first, because they attract each other.

                foreach( var attractor in Attractors )
                {
                    if( attractor.HasCacheForUT( _ut ) ) // attractor trajectory data has been computed up to specified UT (in case of keplerian, it's always available).
                    {
                        continue;
                    }

                    // sim.
                    attractor.Step( Attractors, dt ); // calculate the cached values for the next timestep based on current cached values of the system.
                }

                // followers go second.

                foreach( var follower in Followers )
                {
                    follower.Step( Attractors, dt );
                }
            }

            _simulationEndUT = endUT;
        }
    }
}