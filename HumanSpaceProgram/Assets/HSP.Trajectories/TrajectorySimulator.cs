using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    public sealed class TrajectorySimulator
    {
        public ITrajectory[] Attractors { get; private set; }
        public ITrajectory[] Followers { get; private set; }

        Vector3[][] _predictedFollowerFuturePositions; // lower accuracy but available for future ut values


        private double _simulationEndUT; // ut at which the sim ends.

        private double _ut;

        public void Simulate( double endUT )
        {
            double dt = 1.0 / 200;

            while( _ut < endUT )
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

                _ut += dt;
            }

            _simulationEndUT = endUT;
        }
    }
}