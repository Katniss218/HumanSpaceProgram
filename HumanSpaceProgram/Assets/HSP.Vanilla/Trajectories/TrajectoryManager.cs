using HSP.Time;
using HSP.Vanilla.Trajectories;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private double _flightPlanDuration = 100 * 86400;
        public static double FlightPlanDuration
        {
            get => instance._flightPlanDuration;
            set
            {
                if( value <= 0 )
                    throw new System.ArgumentOutOfRangeException( nameof( value ), "Flight plan duration must be greater than zero." );

                instance._flightPlanDuration = value;
            }
        }

        private TrajectorySimulator[] _simulators;

        private HashSet<TrajectoryTransform> _transforms = new();

        private Dictionary<TrajectoryTransform, (Vector3Dbl pos, Vector3Dbl vel, Vector3Dbl interpolatedVel)> _posAndVelCache = new();

        /// <summary>
        /// Tries to add the specified trajectory to the simulation as an attractor.
        /// </summary>
        /// <returns>True if the trajectory was successfully added to the simulation, otherwise false.</returns>
        public static bool TryAddBody( TrajectoryTransform transform )
        {
            if( !instanceExists )
                return false;

            if( transform == null )
                return false;

            bool wasAdded = instance._transforms.Add( transform );
            if( wasAdded )
            {
                foreach( var simulator in instance._simulators )
                {
#warning TODO - prediction ephemeris depends on user, 'ground truth' is config dependant.
                    simulator.AddBody( transform, null );
                }
            }
            return wasAdded;
        }

        /// <summary>
        /// Tries to remove the specified trajectory (attractor) from the simulation.
        /// </summary>
        /// <returns>True if the trajectory was successfully removed, otherwise false.</returns>
        public static bool TryRemoveBody( TrajectoryTransform transform )
        {
            if( !instanceExists )
                return false;

            if( (object)transform == null ) // Allows unregistering destroyed.
                return false;

            instance.EnsureSimulatorsExist();
            bool wasRemoved = instance._transforms.Remove( transform );
            if( wasRemoved )
            {
                foreach( var simulator in instance._simulators )
                {
                    simulator.RemoveBody( transform );
                }
            }
            return wasRemoved;
        }

        public static void MarkBodyDirty( TrajectoryTransform transform )
        {
            foreach( var simulator in instance._simulators )
            {
                simulator.MarkBodyDirty( transform );
            }
        }

        /// <summary>
        /// Clears all attractors and followers from the simulation.
        /// </summary>
        public static void Clear()
        {
            if( !instanceExists )
                return;

            instance._transforms.Clear();
            foreach( var simulator in instance._simulators )
            {
                simulator.Clear();
            }
        }

        private void EnsureSimulatorsExist()
        {
            if( _simulators == null )
            {
                _simulators = new[]
                {
                    new TrajectorySimulator( TimeManager.UT ),
                    new TrajectorySimulator( TimeManager.UT )
                };
            }
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

            instance.EnsureSimulatorsExist();

            foreach( var trajectoryTransform in instance._transforms )
            {
                if( !trajectoryTransform.TrajectoryDoesntNeedUpdating() )
                {
                    foreach( var simulator in instance._simulators )
                    {
                        simulator.ResetCurrentStateVector( trajectoryTransform );
                    }
                }
            }

            foreach( var simulator in instance._simulators )
            {
                //double time = sim.EndUT;
                simulator.Simulate( TimeManager.UT );
                //double deltaTime = sim.EndUT - time;
            }

            foreach( var trajectoryTransform in instance._transforms )
            {
                TrajectoryStateVector stateVector = Simulator.GetCurrentStateVector( trajectoryTransform );

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