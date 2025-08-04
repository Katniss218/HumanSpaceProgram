using HSP.ReferenceFrames;
using HSP.Trajectories;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Trajectories
{
    /// <summary>
    /// Makes an object follow a trajectory.
    /// </summary>
    public class TrajectoryTransform : MonoBehaviour, ITrajectoryTransform
    {
        private IPhysicsTransform _physicsTransform;
        /// <summary>
        /// Gets the physics transform associated with this game object.
        /// </summary>
        public IPhysicsTransform PhysicsTransform
        {
            get
            {
                if( _physicsTransform.IsUnityNull() )
                {
                    _physicsTransform = this.GetComponent<IPhysicsTransform>();
                }
                return _physicsTransform;
            }
        }

        private IReferenceFrameTransform _referenceFrameTransform;
        /// <summary>
        /// Gets the reference frame transform associated with this game object.
        /// </summary>
        public IReferenceFrameTransform ReferenceFrameTransform
        {
            get
            {
                if( _referenceFrameTransform.IsUnityNull() )
                {
                    _referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
                    _referenceFrameTransform.OnAnyValueChanged += OnValueChanged;
                }
                return _referenceFrameTransform;
            }
        }

        private ITrajectoryIntegrator _trajectory;
        /// <summary>
        /// Gets or sets the trajectory that this object will follow.
        /// </summary>
        public ITrajectoryIntegrator TrajectoryIntegrator
        {
            get => _trajectory;
            set
            {
                if( value == null )
                {
                    throw new ArgumentNullException( nameof( value ), "The trajectory can't be null." );
                }

                TryUnregister();
                _trajectory = value;

                //TrajectoryBodyState trajectoryState = value.Step(...);
                //_referenceFrameTransform.AbsolutePosition = trajectoryState.AbsolutePosition;
                //_referenceFrameTransform.AbsoluteVelocity = trajectoryState.AbsoluteVelocity;

                TryRegister();
            }
        }

        private IAccelerationProvider[] _accelerationProviders;
        /// <summary>
        /// Gets or sets the trajectory that this object will follow.
        /// </summary>
        public IReadOnlyList<IAccelerationProvider> AccelerationProviders
        {
            get => _accelerationProviders;
        }

        /// <remarks>
        /// The array will be copied.
        /// </remarks>
        public void SetAccelerationProviders( IEnumerable<IAccelerationProvider> accelerationProviders )
        {
            if( accelerationProviders == null )
                throw new ArgumentNullException( nameof( accelerationProviders ), "The acceleration provider collection can't be null." );

            if( !accelerationProviders.Any() )
                throw new ArgumentException( "The acceleration provider collection can't be empty.", nameof( accelerationProviders ) );

            if( accelerationProviders.Any( t => t == null ) )
                throw new ArgumentException( "The acceleration provider collection can't contain null elements.", nameof( accelerationProviders ) );

            TryUnregister();
            _accelerationProviders = accelerationProviders.ToArray(); // copy to prevent later edits.
            TryRegister();
        }

        private bool _isAttractor;
        /// <summary>
        /// If true, the object will act like a gravitational attractor.
        /// </summary>
        public bool IsAttractor
        {
            get => _isAttractor;
            set
            {
                if( _isAttractor == value )
                    return;

                TryUnregister();
                _isAttractor = value;
                TryRegister();
            }
        }

        private bool _forceResyncWithTrajectory = false;
        private IReferenceFrameTransform _lastSynchronizedTransform;

        /// <summary>
        /// Checks if the object has more up-to-date (more correct) information than the trajectory.
        /// </summary>
        /// <returns>
        /// True if the trajectory and reference frame transform are in sync. <br/>
        /// False if the trajectory needs to be updated using the reference frame transform's values. <br/>
        /// Related to <see cref="RequestForcedResynchronization"/>.
        /// </returns>
        public bool TrajectoryDoesntNeedUpdating()
        {
            bool value = _lastSynchronizedTransform == this.ReferenceFrameTransform // Because we use an event to check the manual reset of values, it would be possible to swap
                                                                                    // the reference frame transform on the GameObject to a different instance and change its
                                                                                    // position before the event is re-registered to that new instance.

                && !HasCollidedWithSomething() // Because trajectories shouldn't ignore object's collision response.
                && !HadForcesApplied()         // Because trajectories shouldn't ignore external forces you apply to the object.
                && !_hadValuesChangedByHand    // Because trajectories shouldn't ignore when you move the object by hand.
                && !_forceResyncWithTrajectory;

            _lastSynchronizedTransform = this.ReferenceFrameTransform;
            _forceResyncWithTrajectory = false;
            _hadValuesChangedByHand = false;

            return value;
        }

        /// <summary>
        /// Forces the trajectory to update using the reference transform's values at the next available time.
        /// </summary>
        public void RequestForcedResynchronization()
        {
            _forceResyncWithTrajectory = true;
        }

        private bool HasCollidedWithSomething() => this.PhysicsTransform.IsColliding;

        private bool HadForcesApplied() => this.ReferenceFrameTransform.AbsoluteAcceleration != Vector3Dbl.zero;

        private bool _hadValuesChangedByHand;

        void OnEnable()
        {
            RequestForcedResynchronization();
            _referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>(); // The assignment is needed, otherwise the event will also be added (again) by the getter.
            _referenceFrameTransform.OnAnyValueChanged += OnValueChanged;
            TryRegister();
        }

        void OnDisable()
        {
            _referenceFrameTransform.OnAnyValueChanged -= OnValueChanged;
            TryUnregister();
        }

        private void OnValueChanged() => _hadValuesChangedByHand = true;

        private bool TryRegister()
        {
            if( _trajectory == null )
                return false;

            return TrajectoryManager.TryAddBody( this );
        }

        private bool TryUnregister()
        {
            return TrajectoryManager.TryRemoveBody( this );
        }

        [MapsInheritingFrom( typeof( TrajectoryTransform ) )]
        public static SerializationMapping TrajectoryTransformMapping()
        {
            return new MemberwiseSerializationMapping<TrajectoryTransform>()
                .WithMember( "trajectory", o => o._trajectory )
                .WithMember( "is_attractor", o => o._isAttractor );
        }
    }
}