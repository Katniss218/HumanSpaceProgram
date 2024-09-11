using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    public class TrajectorySimulator : SingletonMonoBehaviour<TrajectorySimulator>
    {
        private List<ITrajectory> _attractors = new();
        private List<ITrajectory> _followers = new();

        Vector3[][] _predictedFollowerFuturePositions; // lower accuracy but available for future ut values


        private double _simulationEndUT; // The last ut for which the sim has been computed.

        private double _ut;

        public static void RegisterAttractor( ITrajectory attractorTrajectory )
        {
            instance._attractors.Add( attractorTrajectory );
        }

        public static void UnregisterAttractor( ITrajectory attractorTrajectory )
        {
            instance._attractors.Remove( attractorTrajectory );
        }

        public static void RegisterFollower( ITrajectory followerTrajectory )
        {
            instance._followers.Add( followerTrajectory );
        }

        public static void UnregisterFollower( ITrajectory followerTrajectory )
        {
            instance._followers.Remove( followerTrajectory );
        }

        public static void Clear()
        {
            instance._attractors.Clear();
            instance._followers.Clear();
        }

        public void Simulate( double endUT )
        {
            double dt = 1.0 / 200;

            while( _ut < endUT )
            {
                // attractors go first, because they attract each other.

                foreach( var attractor in _attractors )
                {
                    if( attractor.HasCacheForUT( _ut ) ) // attractor trajectory data has been computed up to specified UT (in case of keplerian, it's always available).
                    {
                        continue;
                    }

                    // sim.
                    attractor.Step( _attractors, dt ); // calculate the cached values for the next timestep based on current cached values of the system.
                }


                // followers go second.

                foreach( var follower in _followers )
                {
                    follower.Step( _attractors, dt );
                }

                _ut += dt;
            }

            _simulationEndUT = endUT;
        }
    }
}