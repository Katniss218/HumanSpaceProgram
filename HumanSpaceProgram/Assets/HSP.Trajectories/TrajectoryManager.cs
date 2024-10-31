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
            PlayerLoopUtils.InsertSystemAfter<FixedUpdate>( in _afterPlayerLoopSystem, typeof( FixedUpdate.Physics2DFixedUpdate ) );
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

        private Dictionary<ITrajectory, (Vector3Dbl pos, Vector3Dbl vel)> _posAndVelCache = new();

        private static void BeforePhysicsProcessing()
        {
            if( !instanceExists )
                return;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                TrajectoryBodyState stateVector = new TrajectoryBodyState(
                    trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity, // velocity before rigidbody forces from this frame are applied.
                    Vector3Dbl.zero,
                    trajectoryTransform.PhysicsTransform.Mass );

                // (3727.62, -29514.69, 0.00)
                // the entire thing kind of gets fucked up though

#warning TODO - Seems that the state vector to keplerian gives wrong results still... ugh...
                // the anomaly is fucked up somewhere between the first and 2nd simulate

                trajectory.SetCurrentState( stateVector );
            }

            double time = instance._simulator.EndUT;
            instance._simulator.Simulate( TimeManager.UT );
            double deltaTime = instance._simulator.EndUT - time;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                TrajectoryBodyState stateVector = trajectory.GetCurrentState();
                instance._posAndVelCache[trajectory] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity);

                if( trajectoryTransform.IsSynchronized() )
                {
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = (stateVector.AbsolutePosition - trajectoryTransform.ReferenceFrameTransform.AbsolutePosition) / TimeManager.FixedDeltaTime;

                    Debug.Log( trajectoryTransform.gameObject.name + "  S:  " + trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity + "  :  " + stateVector.AbsoluteVelocity );
                }
                else
                {
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
                    Debug.Log( trajectoryTransform.gameObject.name + "  NS:  " + trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity + "  :  " + stateVector.AbsoluteVelocity );
                }
            }
        }

        private static void AfterPhysicsProcessing()
        {
            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                // If it is STILL synchronized after physicsprocessing
                if( trajectoryTransform.IsSynchronized() )
                {
                    //trajectoryTransform.ReferenceFrameTransform.AbsolutePosition = instance._posAndVelCache[trajectory].pos; // Experimental testing seems to indicate that this is indeed unnecessary for countering drift.
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = instance._posAndVelCache[trajectory].vel;
                }
            }

            i++;
        }
    }
}