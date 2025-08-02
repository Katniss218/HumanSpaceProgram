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
        public static IReadonlyTrajectorySimulator Simulator => instance._simulators[0];
        public static IReadonlyTrajectorySimulator PredictionSimulator => instance._simulators[1];

        private TrajectorySimulator[] _simulators;

        private HashSet<TrajectoryTransform> _transforms = new();

        private bool _isStale = false;
        private bool _attractorChanged = false;

        private Dictionary<TrajectoryTransform, (Vector3Dbl pos, Vector3Dbl vel, Vector3Dbl interpolatedVel)> _posAndVelCache = new();

        /// <summary>
        /// Tries to add the specified trajectory to the simulation as an attractor.
        /// </summary>
        /// <returns>True if the trajectory was successfully added to the simulation, otherwise false.</returns>
        public static bool TryRegister( TrajectoryTransform transform )
        {
            if( !instanceExists )
                return false;

            if( transform == null )
                return false;

            bool ret = instance._transforms.Add( transform );
            instance._isStale = ret;
            instance._attractorChanged = transform.IsAttractor;
            return ret;
        }

        /// <summary>
        /// Tries to remove the specified trajectory (attractor) from the simulation.
        /// </summary>
        /// <returns>True if the trajectory was successfully removed, otherwise false.</returns>
        public static bool TryUnregister( TrajectoryTransform transform )
        {
            if( !instanceExists )
                return false;
            
            if( (object)transform == null ) // Allows unregistering destroyed.
                return false;

            bool ret = instance._transforms.Remove( transform );
            instance._isStale = ret;
            instance._attractorChanged = transform.IsAttractor;
            return ret;
        }

        /// <summary>
        /// Clears all attractors and followers from the simulation.
        /// </summary>
        public static void Clear()
        {
            if( !instanceExists )
                return;

            instance._isStale = true;
            instance._transforms.Clear();
        }

        private void FixStale()
        {
            if( _attractorChanged )
            {
                // reset every ephemeris because accelerations changed.
            }
            // if anything that was added was an attractor
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

            if( instance._isStale )
            {
                instance.FixStale();
            }

            foreach( var trajectoryTransform in instance._transforms )
            {
                if( !trajectoryTransform.TrajectoryDoesntNeedUpdating() )
                {
                    TrajectoryBodyState stateVector = new TrajectoryBodyState(
                        trajectoryTransform.ReferenceFrameTransform.AbsolutePosition,
                        trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity, // velocity before rigidbody forces from this frame are applied.
                        Vector3Dbl.zero,
                        trajectoryTransform.PhysicsTransform.Mass );

                    foreach( var sim in instance._simulators )
                        sim.SetCurrentStateVector( trajectoryTransform, stateVector );
                }
            }

            foreach( var sim in instance._simulators )
            {
                //double time = sim.EndUT;
                sim.Simulate( TimeManager.UT );
                //double deltaTime = sim.EndUT - time;
            }

            foreach( var trajectoryTransform in instance._transforms )
            {
                TrajectoryBodyState stateVector = Simulator.GetCurrentStateVector( trajectoryTransform );

                if( trajectoryTransform.TrajectoryDoesntNeedUpdating() )
                {
                    // If the transform is synchronized, make the velocity what it should be to make it move to the target location.
                    // This - at least in theory - should make PhysX happier, because the position is not being reset, in turn resetting some physics scene stuff.
                    Vector3Dbl interpolatedVel = (stateVector.AbsolutePosition - trajectoryTransform.ReferenceFrameTransform.AbsolutePosition) / TimeManager.FixedDeltaTime;

                    instance._posAndVelCache[trajectoryTransform] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity, interpolatedVel);

                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = interpolatedVel;
                }
                else
                {
                    instance._posAndVelCache[trajectoryTransform] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity, Vector3Dbl.zero);
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
                }
            }
        }

        private static void ImmediatelyAfterUnityPhysicsStep()
        {
            if( !instanceExists )
                return;

            foreach( var trajectoryTransform in instance._transforms )
            {
                var (_, vel, interpolatedVel) = instance._posAndVelCache[trajectoryTransform];

                /*if( trajectoryTransform.IsSynchronized() ) // Experimental testing seems to indicate that this is unnecessary for countering drift.
                {
                 TODO - enabling the position reset makes keplerian trajectories act weird
                    trajectoryTransform.ReferenceFrameTransform.AbsolutePosition = pos;
                }*/

                // IsSynchronized() will return false if a kinematic object (e.g. planet) is colliding,
                //   in such a case, the object didn't change its velocity so it's technically still synchronized.
                if( trajectoryTransform.TrajectoryDoesntNeedUpdating() || trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity == interpolatedVel )
                {
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = vel;
                }
            }
        }
    }
}