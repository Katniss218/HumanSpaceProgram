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

        private double _ut;

        /// <summary>
        /// Simulates the trajectories all the way to the specified UT.
        /// </summary>
        /// <param name="endUT">The UT at which to finish the simulation.</param>
        public void Simulate( double endUT )
        {
            double delta = endUT - _simulationEndUT;
            double dt = delta / 10;

            //double attr1vel = Followers[0].GetCurrentState().AbsoluteVelocity.magnitude;
           // int i = 0;
#warning TODO - last step needs to be such that the time matches the desired time.
            for( ; _ut < endUT; _ut += dt )
            {
                //i++;
                // Copy the states to ensure that the simulation doesn't use values calculated for the next step on elements after the first element.
                List<TrajectoryBodyState> attractorStates = new List<TrajectoryBodyState>( Attractors.Count );
                List<TrajectoryBodyState> followerStates = new List<TrajectoryBodyState>( Followers.Count );
                foreach( var t in Attractors )
                {
                    attractorStates.Add( t.GetCurrentState() );
                }
                foreach( var t in Followers )
                {
                    followerStates.Add( t.GetCurrentState() );
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
            }
            //double attr1vel2 = Followers[0].GetCurrentState().AbsoluteVelocity.magnitude;
            //Debug.Log( attr1vel + " " + attr1vel2 );

            _simulationEndUT = _ut;
        }
    }
}