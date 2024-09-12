using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Trajectories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;

namespace Assets.HSP.Trajectories
{
    public class TrajectoryManager : SingletonMonoBehaviour<TrajectoryManager>
    {
        private TrajectorySimulator _simulator = new();
        private Dictionary<ITrajectory, IReferenceFrameTransform> _trajectoryMap = new();

        public static void RegisterAttractor( ITrajectory attractorTrajectory, IReferenceFrameTransform transform )
        {
            if( instance._trajectoryMap.ContainsKey( attractorTrajectory ) )
                throw new ArgumentException( $"The trajectory '{attractorTrajectory}' has already been registered.", nameof( attractorTrajectory ) );

            instance._simulator.Attractors.Add( attractorTrajectory );
            instance._trajectoryMap.Add( attractorTrajectory, transform );
        }

        public static void UnregisterAttractor( ITrajectory attractorTrajectory )
        {
            instance._simulator.Attractors.Remove( attractorTrajectory );
            instance._trajectoryMap.Remove( attractorTrajectory );
        }

        public static void RegisterFollower( ITrajectory followerTrajectory, IReferenceFrameTransform transform )
        {
            if( instance._trajectoryMap.ContainsKey( followerTrajectory ) )
                throw new ArgumentException( $"The trajectory '{followerTrajectory}' has already been registered.", nameof( followerTrajectory ) );

            instance._simulator.Followers.Add( followerTrajectory );
            instance._trajectoryMap.Add( followerTrajectory, transform );
        }

        public static void UnregisterFollower( ITrajectory followerTrajectory )
        {
            instance._simulator.Followers.Remove( followerTrajectory );
            instance._trajectoryMap.Remove( followerTrajectory );
        }

        public static void Clear()
        {
            instance._trajectoryMap.Clear();
            instance._simulator.Attractors.Clear();
            instance._simulator.Followers.Clear();
        }

        void OnEnable()
        {
            PlayerLoopUtils.InsertSystemBefore<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.Physics2DFixedUpdate ) );
        }

        void OnDisable()
        {
            PlayerLoopUtils.RemoveSystem<FixedUpdate>( in _playerLoopSystem );
        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( TrajectoryManager ),
            updateDelegate = PlayerLoopFixedUpdate,
            subSystemList = null
        };

        private static void PlayerLoopFixedUpdate()
        {
            if( !instanceExists )
                return;

            // TODO - feed back the current positions and velocities of the objects if they have collided with anything in the previous frame.
            {

            }

            instance._simulator.Simulate( TimeManager.UT );
            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
               // OrbitalStateVector stateVector = trajectory.GetStateVectorAtUT( TimeManager.UT );
               // trajectoryTransform.AbsolutePosition = stateVector.AbsolutePosition;
               // trajectoryTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
            }
        }
    }
}