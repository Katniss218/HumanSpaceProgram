using HSP.Time;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;

namespace HSP.Trajectories
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
            PlayerLoopUtils.InsertSystemBefore<FixedUpdate>( in _beforePlayerLoopSystem, typeof( FixedUpdate.PhysicsFixedUpdate ) );
            PlayerLoopUtils.InsertSystemAfter<FixedUpdate>( in _afterPlayerLoopSystem, typeof( FixedUpdate.PhysicsFixedUpdate ) );
        }

        void OnDisable()
        {
            PlayerLoopUtils.RemoveSystem<FixedUpdate>( in _beforePlayerLoopSystem );
            PlayerLoopUtils.RemoveSystem<FixedUpdate>( in _afterPlayerLoopSystem );
        }

        private static PlayerLoopSystem _beforePlayerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( TrajectoryManager ),
            updateDelegate = BeforePhysicsProcessing,
            subSystemList = null
        };

        private static PlayerLoopSystem _afterPlayerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( TrajectoryManager ),
            updateDelegate = AfterPhysicsProcessing,
            subSystemList = null
        };

        public static int i = 0;

        private Dictionary<ITrajectory, Vector3Dbl> _velocityCache = new();

        private static void BeforePhysicsProcessing()
        {
            if( !instanceExists )
                return;

            // simulation scheme is as follows:
            // Before physics engine is run - take the position and velocity, run the sim, and apply the result back.

            // This simulation code DOESN'T have to be in any specific point during the frame I think.

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                TrajectoryBodyState stateVector = new TrajectoryBodyState(
                    trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity, // velocity before rigidbody forces from this frame are applied.
                    Vector3Dbl.zero,
                    trajectoryTransform.PhysicsTransform.Mass );

                trajectory.SetCurrentState( stateVector );
            }

            double time = instance._simulator.EndUT;
            instance._simulator.Simulate( TimeManager.UT );
            double deltaTime = instance._simulator.EndUT - time;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                TrajectoryBodyState stateVector = trajectory.GetCurrentState();

                if( trajectoryTransform.ReferenceFrameTransform.gameObject.name == "tempname_vessel" )
                {
                    Debug.Log( i + "  :  " + deltaTime + " : " + stateVector.AbsoluteVelocity );
                }

                // 160 frames with velocity, so it's even weirder.
                // 210 with resetting position
                // 210 with gravity applier (previous)


                // subtracting velocity kind of works as a correction, but can fuck with collision, because we *are* moving the object around.
                // setting Rigidbody velocity directly resets queued forces

                instance._velocityCache[trajectory] = stateVector.AbsolutePosition;
                //trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = (stateVector.AbsolutePosition - trajectoryTransform.ReferenceFrameTransform.AbsolutePosition) / TimeManager.FixedDeltaTime;
                //trajectoryTransform.ReferenceFrameTransform.AbsolutePosition = stateVector.AbsolutePosition - (stateVector.AbsoluteVelocity * TimeManager.FixedDeltaTime);
                trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
            }
        }

        private static void AfterPhysicsProcessing()
        {
            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                if( trajectoryTransform.ReferenceFrameTransform.gameObject.name == "tempname_vessel" )
                {
                    Debug.Log( i + "  :  " + (trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity) );
                }
                if( trajectoryTransform.IsSynchronized() )
                {
#warning TODO - velocity is being accumulated twice after the tanks detach, for some reason.
                     trajectoryTransform.ReferenceFrameTransform.AbsolutePosition = instance._velocityCache[trajectory];
                }
            }

            i++;
        }
    }
}