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
            PlayerLoopUtils.InsertSystemBefore<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.PhysicsFixedUpdate ) );
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


        static int i = 0;

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
                //if( !trajectoryTransform.IsSynchronized() )
                //{
                // synchronize
                var stateVector = new TrajectoryBodyState(
                    trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity,
                    Vector3Dbl.zero,
                    trajectoryTransform.PhysicsTransform.Mass );
                trajectory.SetCurrentState( stateVector );
                //Debug.Log( trajectoryTransform.gameObject.name + " 1 " + trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity.magnitude );
                //}
            }

            double time = instance._simulator.EndUT;
            instance._simulator.Simulate( TimeManager.UT );
            double deltaTime = instance._simulator.EndUT - time;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                TrajectoryBodyState stateVector = trajectory.GetCurrentState();
                Vector3Dbl delta = stateVector.AbsolutePosition - trajectoryTransform.ReferenceFrameTransform.AbsolutePosition;
                //Vector3Dbl vel = delta2 / TimeManager.FixedDeltaTime;
                Vector3Dbl vel2 = delta / TimeManager.FixedDeltaTime;
                Vector3Dbl vel3 = delta / deltaTime;

                Vector3Dbl vel = stateVector.AbsoluteVelocity; // temporary fix
                                                               // We want to use the delta between the start and end, not the last sub-frame's velocity.

                // if( !double.IsNaN( vel3.magnitude ) )
                // {
                trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = vel;
               // }
                if( trajectoryTransform.gameObject.name != "celestialbody" )
                {
                    Debug.Log( " T" + i + " " + trajectoryTransform.gameObject.name + " ::: " + vel.magnitude + " ::: " + vel2.magnitude + " ::: " + vel3.magnitude );
                }
#warning TODO - vel (delta) doesn't want to accumulate correctly.
                // something inside trajectory simulator and related to delta time, timemanager's UT, etc.

                // it's set to weird values between frames?

                // input vel = 15, output vel = 17
                // input vel = 17, output vel = 15 (WTF?)
#warning TODO - only happens when I use the absolute position delta based velocity.

            }
            i++;
        }
    }
}