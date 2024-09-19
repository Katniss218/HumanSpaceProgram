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
        private Dictionary<ITrajectory, TrajectoryTransform> _trajectoryMap = new();

        public static bool TryRegisterAttractor( ITrajectory attractorTrajectory, TrajectoryTransform transform )
        {
            if( !instanceExists )
                return false;
            if( attractorTrajectory == null || transform == null )
                return false;
            if( instance._trajectoryMap.ContainsKey( attractorTrajectory ) )
                return false;

            instance._simulator.Attractors.Add( attractorTrajectory );
            instance._trajectoryMap.Add( attractorTrajectory, transform );
            return true;
        }

        public static bool TryUnregisterAttractor( ITrajectory attractorTrajectory )
        {
            if( !instanceExists )
                return false;
            if( attractorTrajectory == null )
                return false;
            if( !instance._trajectoryMap.ContainsKey( attractorTrajectory ) )
                return false;

            instance._simulator.Attractors.Remove( attractorTrajectory );
            instance._trajectoryMap.Remove( attractorTrajectory );
            return true;
        }

        public static bool TryRegisterFollower( ITrajectory followerTrajectory, TrajectoryTransform transform )
        {
            if( !instanceExists )
                return false;
            if( followerTrajectory == null || transform == null )
                return false;
            if( instance._trajectoryMap.ContainsKey( followerTrajectory ) )
                return false;

            instance._simulator.Followers.Add( followerTrajectory );
            instance._trajectoryMap.Add( followerTrajectory, transform );
            return true;
        }

        public static bool TryUnregisterFollower( ITrajectory followerTrajectory )
        {
            if( !instanceExists )
                return false;
            if( followerTrajectory == null )
                return false;
            if( !instance._trajectoryMap.ContainsKey( followerTrajectory ) )
                return false;

            instance._simulator.Followers.Remove( followerTrajectory );
            instance._trajectoryMap.Remove( followerTrajectory );
            return true;
        }

        public static void Clear()
        {
            if( !instanceExists )
                return;

            instance._trajectoryMap.Clear();
            instance._simulator.Attractors.Clear();
            instance._simulator.Followers.Clear();
        }

        void OnEnable()
        {
            PlayerLoopUtils.InsertSystemAfter<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.PhysicsFixedUpdate ) );
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

            /*
            simulation will compute the position of the object, that's correct IF IT HASN'T COLLIDED WITH ANYTHING or it hasn't had any other forces than gravity applied to it.

            so the position could be updated *after* the fixedupdate/physicsupdate, assuming some conditions are met that'll guarantee
                that the simulation data is correct for what's happening to the physicsobject.

            the correctness is when: someone didn't move it, and it didn't collide with anything.
            */

            // after physics has moved it, so the transform should now have the correct position and won't move.

#warning TODO - forces include gravitational forces (there's no way to distinguish them really, and we do want to use trajectories to do gravity anyway so there won't be gravity forces on the vessels)

            // do we want to do this checking via events or flags?

            // events can be hooked by the trajectory follower and then exposed as flags.



#warning TODO - maybe expose some HSPEvents at specific points during the frame?? (via playerloop) I don't want hspevents to be related to the frame, not HSP things happening though.

            // what if I put the sync code here:

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                if( !trajectoryTransform.IsSynchronized() )
                {
                    // synchronize
                    var stateVector = new TrajectoryBodyState(
                        trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                        trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity,
                        trajectoryTransform.ReferenceFrameTransform.AbsoluteAcceleration,
                        trajectoryTransform.PhysicsTransform.Mass );
                    trajectory.SetCurrentState( stateVector );
                    // apply the velocity that would result from the trajectory or something?
                }
            }
            // if not synchronized - set the trajectory to current, then simulate, and set again?
#warning syncing before causes velocity to be integrated twice (because velocity has been applied to the position by the physics object, and we're now reusing that position).
            // can't use old position because collision might've happened.

            instance._simulator.Simulate( TimeManager.UT );
            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                //if( trajectoryTransform.IsSynchronized() )
                //{
                TrajectoryBodyState stateVector = trajectory.GetCurrentState();
#warning TODO - use MovePosition?
                trajectoryTransform.ReferenceFrameTransform.AbsolutePosition = stateVector.AbsolutePosition;
                trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
                // }
                /* else
                {
 #warning TODO - if we stop applying trajectory, then the gravity will also stop being applied if you fire the engine.
 #warning        passing forces into the trajectory won't solve it because we need to apply gravity when colliding as well, but then we can split the sim into scene or trajectory based on collision state.

                     // update trajectory for the next position.
                     var stateVector = new TrajectoryBodyState(
                         trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                         trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity,
                         trajectoryTransform.ReferenceFrameTransform.AbsoluteAcceleration,
                         trajectoryTransform.PhysicsTransform.Mass );
                     trajectory.SetCurrentState( stateVector );

                     // apply the velocity that would result from the trajectory or something?
                 }*/
            }

#warning TODO - can I somehow apply the trajectory even if it's not synchronized?
            // or synchronize it and then simulate?

            // if not, we need to switch between manual and trajectory-based gravity whenever the trajectory based can't be used.
            // and manual gravity must be excluded from the acceleration calculation for the purpose of syncing.

        }
    }
}