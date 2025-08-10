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

        private ITrajectoryIntegrator _integrator;
        /// <summary>
        /// Gets or sets the trajectory that this object will follow.
        /// </summary>
        public ITrajectoryIntegrator Integrator
        {
            get => _integrator;
            set
            {
                if( value == null )
                {
                    throw new ArgumentNullException( nameof( value ), "The trajectory can't be null." );
                }

                _integrator = value;

                //TrajectoryBodyState trajectoryState = value.Step(...);
                //_referenceFrameTransform.AbsolutePosition = trajectoryState.AbsolutePosition;
                //_referenceFrameTransform.AbsoluteVelocity = trajectoryState.AbsoluteVelocity;

                TrajectoryManager.MarkBodyDirty( this );
            }
        }

        private ITrajectoryStepProvider[] _accelerationProviders;
        /// <summary>
        /// Gets or sets the trajectory that this object will follow.
        /// </summary>
        public IReadOnlyList<ITrajectoryStepProvider> AccelerationProviders
        {
            get => _accelerationProviders;
        }

        /// <remarks>
        /// The array will be copied.
        /// </remarks>
        public void SetAccelerationProviders( IEnumerable<ITrajectoryStepProvider> accelerationProviders )
        {
            if( accelerationProviders == null )
                throw new ArgumentNullException( nameof( accelerationProviders ), "The acceleration provider collection can't be null." );

            if( accelerationProviders.Any( t => t == null ) )
                throw new ArgumentException( "The acceleration provider collection can't contain null elements.", nameof( accelerationProviders ) );

            _accelerationProviders = accelerationProviders.ToArray();
            TrajectoryManager.MarkBodyDirty( this );
        }

        /// <remarks>
        /// The array will NOT be copied.
        /// </remarks>
        public void SetAccelerationProviders( params ITrajectoryStepProvider[] accelerationProviders )
        {
            if( accelerationProviders == null )
                throw new ArgumentNullException( nameof( accelerationProviders ), "The acceleration provider collection can't be null." );

            if( accelerationProviders.Any( t => t == null ) )
                throw new ArgumentException( "The acceleration provider collection can't contain null elements.", nameof( accelerationProviders ) );

            _accelerationProviders = accelerationProviders;
            TrajectoryManager.MarkBodyDirty( this );
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

                _isAttractor = value;
                TrajectoryManager.MarkBodyDirty( this );
            }
        }

        public double EphemerisTimeResolution { get; set; }
        public double EphemerisDuration { get; set; }

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
        public bool TrajectoryNeedsUpdating()
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

            return !value;
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

        private void OnValueChanged() => _hadValuesChangedByHand = _allowValueChanged;
        private bool _allowValueChanged = true;

        // SuppressValueChanged and AllowValueChanged can be used to update the transform without marking the transform as stale.
        public void SuppressValueChanged()
        {
            _allowValueChanged = false;
        }
        public void AllowValueChanged()
        {
            _allowValueChanged = true;
        }

        private bool TryRegister()
        {
            if( _integrator == null )
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
                .WithMember( "is_attractor", o => o._isAttractor )
                .WithMember( "integrator", o => o._integrator )
                .WithMember( "acceleration_providers", o => o._accelerationProviders );
        }
    }
}