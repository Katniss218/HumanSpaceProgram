using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityPlus.PlayerLoop;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <summary>
    /// A physics transform that is free to move and collide with the environment.
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FreeReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        private ISceneReferenceFrameProvider _sceneReferenceFrameProvider;
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider
        {
            get => _sceneReferenceFrameProvider;
            set
            {
                if( _sceneReferenceFrameProvider == value )
                    return;

                _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
                _sceneReferenceFrameProvider = value;
                _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            }
        }

        private KinematicState _state = KinematicState.GetIdentity();

        public KinematicState GetState( IReferenceFrame requestedFrame )
        {
            RecalculateCacheIfNeeded();
            return _state.InFrame( requestedFrame );
        }

        public ref readonly KinematicState GetStateRef( out IReferenceFrame referenceFrame )
        {
            RecalculateCacheIfNeeded();
            referenceFrame = null;
            return ref _state;
        }

        public void SetState( in KinematicState state )
        {
            _state = state.InFrame( null );
            MakeCacheValid(); // Ensure it doesn't recalculate immediately
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
            ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.Velocity );
            ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.AngularVelocity );
            OnStateChanged?.Invoke();
        }

        public void ModifyState( IReferenceFrame requestedFrame, KinematicStateMutator mutator )
        {
            RecalculateCacheIfNeeded();
            if( requestedFrame == null )
            {
                mutator( ref _state );
            }
            else
            {
                var localState = _state.InFrame( requestedFrame );
                mutator( ref localState );
                _state = localState.InFrame( null );
            }
            MakeCacheValid(); // Ensure it doesn't recalculate immediately
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
            ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.Velocity );
            ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.AngularVelocity );
            OnStateChanged?.Invoke();
        }

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame = null;

        Vector3 _lastCachedPosition = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );
        Vector3 _lastCachedVelocity = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );
        Quaternion _lastCachedRotation = new Quaternion( 0.21454141f, -23465435.352342f, 231.6354523f, 45.3412435f );
        Vector3 _lastCachedAngularVelocity = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;
        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;


        public event Action OnStateChanged;

        //
        //
        //

        public float Mass
        {
            get => this._rb.mass;
            set => this._rb.mass = value;
        }

        public Vector3 LocalCenterOfMass
        {
            get => this._rb.centerOfMass;
            set => this._rb.centerOfMass = value;
        }

        public Vector3 MomentsOfInertia
        {
            get => this._rb.inertiaTensor;
            set => this._rb.inertiaTensor = value;
        }

        public Quaternion MomentsOfInertiaRotation
        {
            get => this._rb.inertiaTensorRotation;
            set => this._rb.inertiaTensorRotation = value;
        }

        public bool IsColliding { get; private set; }

        Rigidbody ___rb;
        Rigidbody _rb
        {
            get
            {
                if( ___rb == null )
                    ___rb = this.GetComponent<Rigidbody>();
                return ___rb;
            }
        }

        public void AddForce( Vector3 force )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            _state.Acceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAcceleration( (Vector3Dbl)force / Mass );

            this._rb.AddForce( force, ForceMode.Force );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            var referenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            _state.Acceleration += referenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            this._rb.AddForce( force, ForceMode.Force );

            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            Vector3 torque = Vector3.Cross( leverArm, force );
            if( torque.sqrMagnitude > 1e-6 )
            {
                _state.AngularAcceleration += referenceFrame.TransformAngularAcceleration( torque / (float)this.GetInertia( torque.normalized ) );
                this._rb.AddTorque( torque, ForceMode.Force );
            }
        }

        public void AddTorque( Vector3 torque )
        {
            if( torque.sqrMagnitude < 1e-6 )
                return;

            _state.AngularAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularAcceleration( (Vector3Dbl)torque / this.GetInertia( torque.normalized ) );

            this._rb.AddTorque( torque, ForceMode.Force );
        }

        public void AddAbsoluteForce( Vector3 force )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            _state.Acceleration += (Vector3Dbl)force / Mass;

            this._rb.AddForce( SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformDirection( force ), ForceMode.Force );
        }

        public void AddAbsoluteForceAtPosition( Vector3 force, Vector3Dbl position )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            var referenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            _state.Acceleration += (Vector3Dbl)force / Mass;
            this._rb.AddForce( referenceFrame.InverseTransformDirection( force ), ForceMode.Force );

            Vector3Dbl leverArm = position - referenceFrame.TransformPosition( this._rb.worldCenterOfMass );
            Vector3Dbl torque = Vector3Dbl.Cross( leverArm, force );
            if( torque.sqrMagnitude > 1e-6 )
            {
                _state.AngularAcceleration += torque / this.GetInertia( torque.NormalizeToVector3() );
                this._rb.AddTorque( referenceFrame.InverseTransformDirection( (Vector3)torque ), ForceMode.Force );
            }
        }

        public void AddAbsoluteTorque( Vector3 torque )
        {
            if( torque.sqrMagnitude < 1e-6 )
                return;

            _state.AngularAcceleration += (Vector3Dbl)torque / this.GetInertia( torque.normalized );

            this._rb.AddTorque( SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformDirection( torque ), ForceMode.Force );
        }

        protected void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            MakeCacheValid();
        }

        protected void RecalculateCache( IReferenceFrame sceneReferenceFrame )
        {
            bool active = this.gameObject.activeInHierarchy;
            _state.Position = sceneReferenceFrame.TransformPosition( active ? _rb.position : transform.position );
            _state.Rotation = sceneReferenceFrame.TransformRotation( active ? _rb.rotation : transform.rotation ); // Apparently, rigidbody values get set to 0 when disabled...
            if( active )
            {
                _state.Velocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
                _state.AngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
            }
            // Don't cache acceleration, since it's impossible to compute it here for a dynamic body. Acceleration is recalculated on every fixedupdate instead.
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        // Exact comparison of the axes catches the most cases (and it's gonna be set to match exactly so it's okay)
        // Vector3's `==` operator does approximate comparison.
        protected virtual bool IsCacheValid() => _cachedSceneReferenceFrame != null
            && (_rb.position.x == _lastCachedPosition.x && _rb.position.y == _lastCachedPosition.y && _rb.position.z == _lastCachedPosition.z)
            && (_rb.rotation.x == _lastCachedRotation.x && _rb.rotation.y == _lastCachedRotation.y && _rb.rotation.z == _lastCachedRotation.z && _rb.rotation.w == _lastCachedRotation.w)
            && (_rb.velocity.x == _lastCachedVelocity.x && _rb.velocity.y == _lastCachedVelocity.y && _rb.velocity.z == _lastCachedVelocity.z)
            && (_rb.angularVelocity.x == _lastCachedAngularVelocity.x && _rb.angularVelocity.y == _lastCachedAngularVelocity.y && _rb.angularVelocity.z == _lastCachedAngularVelocity.z)
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame );

        protected virtual void MakeCacheValid()
        {
            _lastCachedPosition = _rb.position;
            _lastCachedRotation = _rb.rotation;
            _lastCachedVelocity = _rb.velocity;
            _lastCachedAngularVelocity = _rb.angularVelocity;
        }

        protected virtual void MakeCacheInvalid() => _lastCachedPosition = _rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        protected virtual void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {this.GetType().Name} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = false;
            _rb.drag = 0;
            _rb.angularDrag = 0;
            _rb.maxAngularVelocity = 9000;
        }

        protected virtual void FixedUpdate()
        {
            if( SceneReferenceFrameProvider.GetSceneReferenceFrame() is INonInertialReferenceFrame frame )
            {
                RecalculateCacheIfNeeded();
                Vector3Dbl localPos = frame.InverseTransformPosition( _state.Position );
                Vector3Dbl localVel = (Vector3Dbl)frame.InverseTransformVelocity( _state.Velocity );
                Vector3Dbl localAngVel = (Vector3Dbl)frame.InverseTransformAngularVelocity( _state.AngularVelocity );
                Vector3 linAcc = (Vector3)frame.GetFicticiousAcceleration( localPos, localVel );
                Vector3 angAcc = (Vector3)frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                this._rb.AddForce( linAcc, ForceMode.Acceleration );
                this._rb.AddTorque( angAcc, ForceMode.Acceleration );
            }

#warning TODO - modify it so that this runs after physics step and computes the correct values.
            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                var sceneRef = SceneReferenceFrameProvider.GetSceneReferenceFrame();
                var currentSceneVel = (Vector3)sceneRef.InverseTransformVelocity( _state.Velocity );
                var currentSceneAngVel = (Vector3)sceneRef.InverseTransformAngularVelocity( _state.AngularVelocity );

                var sceneAcc = (currentSceneVel - _oldVelocity) / TimeManager.FixedDeltaTime;
                var sceneAngAcc = (currentSceneAngVel - _oldAngularVelocity) / TimeManager.FixedDeltaTime;

                _state.Acceleration = sceneRef.TransformAcceleration( sceneAcc );
                _state.AngularAcceleration = sceneRef.TransformAngularAcceleration( sceneAngAcc );
            }

            var sceneRefForOld = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            this._oldVelocity = (Vector3)sceneRefForOld.InverseTransformVelocity( _state.Velocity );
            this._oldAngularVelocity = (Vector3)sceneRefForOld.InverseTransformAngularVelocity( _state.AngularVelocity );
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            RecalculateCache( data.OldFrame );
            _cachedSceneReferenceFrame = data.OldFrame;
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, _rb, _state.Rotation );
            ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( data.NewFrame, _rb, _state.Velocity );
            ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( data.NewFrame, _rb, _state.AngularVelocity );
        }

        protected virtual void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            _activeFreeTransforms.Add( this );
            _rb.isKinematic = false; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
        }

        protected virtual void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
            _activeFreeTransforms.Remove( this );
            _rb.isKinematic = true; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
        }

        protected virtual void OnCollisionEnter( Collision collision )
        {
            IsColliding = true;
        }

        protected virtual void OnCollisionStay( Collision collision )
        {
            // `OnCollisionEnter` / Exit are called for every collider.
            // I've tried using an incrementing/decrementing int with enter/exit, but it wasn't updating correctly, and after some time, there were too many collisions.
            // Using `OnCollisionStay` prevents desynchronization.

            IsColliding = true;
        }

        protected virtual void OnCollisionExit( Collision collision )
        {
            IsColliding = false;
        }

        private static List<FreeReferenceFrameTransform> _activeFreeTransforms = new();

        [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.PhysicsStep ), Before = new[] { typeof( HSP.Trajectories.TrajectoryManager.TrajectoryManagerPostPhysicsStepSystem ) } )] // InsidePhysicsStep
        public sealed class FreeReferenceFrameTransformSystem : IPlayerLoopSystem
        {
            public void Run()
            {
                // Assume that other objects aren't allowed to get the absolute position/velocity *in* the physics step, as it is undefined (changes) during it.
                foreach( var t in _activeFreeTransforms )
                {
                    if( !t.IsColliding )
                    {
                        t._state.Acceleration = Vector3Dbl.zero;
                        t._state.AngularAcceleration = Vector3Dbl.zero;
                    }
                }
            }
        }

        [MapsInheritingFrom( typeof( FreeReferenceFrameTransform ) )]
        public static IDescriptor FreePhysicsObjectMapping()
        {
            return new MemberwiseDescriptor<FreeReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => false, ( o, value ) => o._rb.isKinematic = false ) // TODO - isKinematic member is a hack.

                .WithMember( "absolute_position", o => o.GetAbsolutePosition(), ( o, v ) => o.SetAbsolutePosition( v ) )
                .WithMember( "absolute_rotation", o => o.GetAbsoluteRotation(), ( o, v ) => o.SetAbsoluteRotation( v ) )
                .WithMember( "absolute_velocity", o => o.GetAbsoluteVelocity(), ( o, v ) => o.SetAbsoluteVelocity( v ) )
                .WithMember( "absolute_angular_velocity", o => o.GetAbsoluteAngularVelocity(), ( o, v ) => o.SetAbsoluteAngularVelocity( v ) );
        }
    }
}