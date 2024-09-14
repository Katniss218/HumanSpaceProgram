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
            simulation will compute the position of the object, that's correct IF IT HASN'T COLLIDED WITH ANYTHING (during fixedupdate or physicsupdate).
            - so if it hasn't collided with anything, then move it, but if it has, then don't?
            - also, if the velocity has changed (forces applied outside the trajectory), then we also don't want to apply the trajectory, 
                or the force could be passed into the trajectory as well.

            so the position could be updated *after* the fixedupdate/physicsupdate, assuming some conditions are met that'll guarantee
                that the simulation data is correct for what's happening to the physicsobject.

            the correctness is when: someone didn't move it, and it didn't collide with anything.
            */

            // after physics has moved it, so the transform should now have the correct position and won't move.

#warning TODO - forces include gravitational forces (there's no way to distinguish them really, and we do want to use trajectories to do gravity anyway so there won't be gravity forces on the vessels)

            // do we want to do this checking via events or flags?

            // events can be hooked by the trajectory follower and then exposed as flags.



#warning TODO - maybe expose some HSPEvents at specific points during the frame?? (via playerloop) I don't want hspevents to be related to the frame, not HSP things happening though.

            instance._simulator.Simulate( TimeManager.UT );
            foreach( var (trajectory, trajectoryFollower) in instance._trajectoryMap )
            {
                if( trajectoryFollower.IsSynchronized() )
                {
                    OrbitalStateVector stateVector = trajectory.GetCurrentStateVector();
#warning TODO - use MovePosition?
                    trajectoryFollower.ReferenceFrameTransform.AbsolutePosition = stateVector.AbsolutePosition;
                    trajectoryFollower.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
                }
                else
                {
                    // update trajectory for the next position.
                    var stateVector = new OrbitalStateVector( TimeManager.UT, trajectoryFollower.ReferenceFrameTransform.AbsolutePosition, trajectoryFollower.ReferenceFrameTransform.AbsoluteVelocity );
                    trajectoryFollower.Trajectory.SetCurrentStateVector( stateVector );
                }
            }

            // We can check if the position/velocity before physicsupdate is still the same as the previous frame's position/velocity after physicsupdate.
            // would that require that all movements are done during physicsupdate?

            // if we exclusively use the trajectory transforms, we can have more control over it.
#warning TODO - if we don't use custom trajectory transforms, it's easier to handle pinning, unpinning, and so on. It decouples things.
            // we need a custom trajectory-holding component for that but that's okay. It also decouples things.


        }
    }
}