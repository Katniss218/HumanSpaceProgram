using HSP.Time;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;

namespace HSP.Trajectories
{
    /// <summary>
    /// Manages the celestial simulation, updates the game objects based on that simulation.
    /// </summary>
    public class TrajectoryManager : SingletonMonoBehaviour<TrajectoryManager>
    {
        private TrajectorySimulator _simulator = new();
        private Dictionary<ITrajectory, TrajectoryTransform> _trajectoryMap = new();

        /// <summary>
        /// Tries to add the specified trajectory to the simulation as an attractor.
        /// </summary>
        /// <returns>True if the trajectory was successfully added to the simulation, otherwise false.</returns>
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

        /// <summary>
        /// Tries to remove the specified trajectory (attractor) from the simulation.
        /// </summary>
        /// <returns>True if the trajectory was successfully removed, otherwise false.</returns>
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

        /// <summary>
        /// Tries to add the specified trajectory to the simulation as a follower.
        /// </summary>
        /// <returns>True if the trajectory was successfully added to the simulation, otherwise false.</returns>
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

        /// <summary>
        /// Tries to remove the specified trajectory (follower) from the simulation.
        /// </summary>
        /// <returns>True if the trajectory was successfully removed, otherwise false.</returns>
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

        /// <summary>
        /// Clears all attractors and followers from the simulation.
        /// </summary>
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
            updateDelegate = ImmediatelyBeforeUnityPhysicsStep,
            subSystemList = null
        };

        private static PlayerLoopSystem _afterPlayerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( TrajectoryManager ),
            updateDelegate = ImmediatelyAfterUnityPhysicsStep,
            subSystemList = null
        };

        private Dictionary<ITrajectory, (Vector3Dbl pos, Vector3Dbl vel, Vector3Dbl interpolatedVel)> _posAndVelCache = new();

        // Simulation works as follows:

        //          FIXED UPDATE

        // 1. Update the trajectories of objects that are not synchronized
        //      (e.g. someone manually set some value on the transforms,
        //       or a rocket engine has applied a force directly to the game object/vessel)

        // 2. Advance the simulation to the current frame's UT.

        // 3. Set the velocity to a value such that the object will get to the desired location after the Unity's physics step.

        //          UNITY PHYSICS STEP

        // 4. If the object is still synchronized, set the velocity back to what it should be so that other systems can use it,
        // 4.1 If it was desynchronized (e.g. collided with something), it will be resynchronized at next frame's (1.).

        //          UPDATE
        //          LATE UPDATE

        private static void ImmediatelyBeforeUnityPhysicsStep()
        {
            if( !instanceExists )
                return;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                if( !trajectoryTransform.IsSynchronized() )
                {
                    TrajectoryBodyState stateVector = new TrajectoryBodyState(
                        trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                        trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity, // velocity before rigidbody forces from this frame are applied.
                        Vector3Dbl.zero,
                        trajectoryTransform.PhysicsTransform.Mass );

                    trajectory.SetCurrentState( stateVector );
                }
            }

            double time = instance._simulator.EndUT;
            instance._simulator.Simulate( TimeManager.UT );
            double deltaTime = instance._simulator.EndUT - time;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                TrajectoryBodyState stateVector = trajectory.GetCurrentState();

                if( trajectoryTransform.IsSynchronized() )
                {
                    // If the transform is synchronized, make the velocity what it should be to make it move to the target location.
                    // This - at least in theory - should make PhysX happier, because the position is not being reset, in turn resetting some physics scene stuff.
                    Vector3Dbl interpolatedVel = (stateVector.AbsolutePosition - trajectoryTransform.ReferenceFrameTransform.AbsolutePosition) / TimeManager.FixedDeltaTime;

                    instance._posAndVelCache[trajectory] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity, interpolatedVel);

                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = interpolatedVel;
                }
                else
                {
                    instance._posAndVelCache[trajectory] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity, Vector3Dbl.zero);
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
                }
            }
        }

        private static void ImmediatelyAfterUnityPhysicsStep()
        {
            if( !instanceExists )
                return;

            foreach( var (trajectory, trajectoryTransform) in instance._trajectoryMap )
            {
                var (_, vel, interpolatedVel) = instance._posAndVelCache[trajectory];

                /*if( trajectoryTransform.IsSynchronized() ) // Experimental testing seems to indicate that this is unnecessary for countering drift.
                {
                 TODO - enabling the position reset makes keplerian trajectories act weird
                    trajectoryTransform.ReferenceFrameTransform.AbsolutePosition = pos;
                }*/

                // IsSynchronized() will return false if a kinematic object (e.g. planet) is colliding,
                //   in such a case, the object didn't change its velocity so it's technically still synchronized.
                if( trajectoryTransform.IsSynchronized() || trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity == interpolatedVel )
                {
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = vel;
                }
            }
        }
    }
}