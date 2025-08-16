using HSP.Time;
using HSP.Vanilla.Trajectories;
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
        private const int SIMULATOR_INDEX = 0;
        private const int PREDICTION_SIMULATOR_INDEX = 1;
        public static TrajectorySimulator Simulator => instance._simulators[SIMULATOR_INDEX];
        public static TrajectorySimulator PredictionSimulator => instance._simulators[PREDICTION_SIMULATOR_INDEX];

        private TrajectorySimulator[] _simulators;

        private double _flightPlanDuration = 365 * 86400 * 6;
        public static double FlightPlanDuration
        {
            get => instance._flightPlanDuration;
            set
            {
                if( value <= 0 )
                    throw new System.ArgumentOutOfRangeException( nameof( value ), "Flight plan duration must be greater than zero." );

                instance._flightPlanDuration = value;
                instance._simulators[PREDICTION_SIMULATOR_INDEX].SetEphemerisLength( FlightPlanDuration );

            }
        }

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

            instance.EnsureSimulatorsExist();
            bool wasAdded = instance._transforms.Add( transform );
            if( wasAdded )
            {
#warning TODO - prediction ephemeris depends on user, 'ground truth' is config dependant.
                instance._simulators[SIMULATOR_INDEX].AddBody( transform );
                instance._simulators[PREDICTION_SIMULATOR_INDEX].AddBody( transform );
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
            if( !instanceExists )
                return;

            instance.EnsureSimulatorsExist();
            foreach( var simulator in instance._simulators )
            {
                if( simulator.HasBody( transform ) )
                {
                    simulator.MarkBodyDirty( transform );
                }
            }
        }

        /// <summary>
        /// Clears all attractors and followers from the simulation.
        /// </summary>
        public static void Clear()
        {
            if( !instanceExists )
                return;

            instance.EnsureSimulatorsExist();
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
                    new TrajectorySimulator( TimeManager.UT, 0.1, 1.0, 100.0 ),
                    new TrajectorySimulator( TimeManager.UT, FlightPlanDuration / 1000.0, FlightPlanDuration / 1000.0, FlightPlanDuration )
                };
                _simulators[PREDICTION_SIMULATOR_INDEX].MaxStepSize = FlightPlanDuration / 1000.0;
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
                if( trajectoryTransform.TrajectoryNeedsUpdating() )
                {
                    foreach( var simulator in instance._simulators )
                    {
                        simulator.ResetStateVector( trajectoryTransform );
                    }
                }
            }

            instance._simulators[SIMULATOR_INDEX].Simulate( TimeManager.UT );
            instance._simulators[PREDICTION_SIMULATOR_INDEX].Simulate( TimeManager.UT + FlightPlanDuration );

            foreach( var trajectoryTransform in instance._transforms )
            {
                TrajectoryStateVector stateVector = Simulator.GetCurrentStateVector( trajectoryTransform );

                if( trajectoryTransform.TrajectoryNeedsUpdating() )
                {
                    instance._posAndVelCache[trajectoryTransform] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity, Vector3Dbl.zero);
                    //trajectoryTransform.SuppressValueChanged(); // for some reason, suppressing it here makes engines not work right.
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = stateVector.AbsoluteVelocity;
                    //trajectoryTransform.AllowValueChanged();
                }
                else
                {
                    // If the transform is synchronized, make the velocity what it should be to make it move to the target location.
                    // This - at least in theory - should make PhysX happier, because the position is not being reset, in turn resetting some physics scene stuff.
                    Vector3Dbl interpolatedVel = (stateVector.AbsolutePosition - trajectoryTransform.ReferenceFrameTransform.AbsolutePosition) / TimeManager.FixedDeltaTime;

                    instance._posAndVelCache[trajectoryTransform] = (stateVector.AbsolutePosition, stateVector.AbsoluteVelocity, interpolatedVel);

                    trajectoryTransform.SuppressValueChanged();
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = interpolatedVel;
                    trajectoryTransform.AllowValueChanged();
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

                if( !trajectoryTransform.TrajectoryNeedsUpdating() || trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity == interpolatedVel )
                {
                    trajectoryTransform.SuppressValueChanged();
                    trajectoryTransform.ReferenceFrameTransform.AbsoluteVelocity = vel;
                    trajectoryTransform.AllowValueChanged();
                }
            }
        }
    }
}