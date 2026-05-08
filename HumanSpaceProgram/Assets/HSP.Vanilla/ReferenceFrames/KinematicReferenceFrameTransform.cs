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
    /// A physics transform that is free to move around and respond to forces, but doesn't respond to collisions (other objects can still collide with it).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class KinematicReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        private ISceneReferenceFrameProvider _sceneReferenceFrameProvider;
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider
        {
            get => _sceneReferenceFrameProvider;
            set
            {
                _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
                _sceneReferenceFrameProvider = value;
                _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            }
        }

        private KinematicState _state = KinematicState.AbsoluteIdentity;
        private KinematicState _requestedState = KinematicState.AbsoluteIdentity;

        public ref readonly KinematicState GetStateRef( out IReferenceFrame referenceFrame )
        {
            RecalculateCacheIfNeeded();
            referenceFrame = null;
            return ref _state;
        }

        public KinematicState GetState( IReferenceFrame requestedFrame )
        {
            RecalculateCacheIfNeeded();
            return _state.InFrame( requestedFrame );
        }

        public void SetState( in KinematicState state )
        {
            _state = state.InFrame( null );
            _requestedState = _state;
            MakeCacheValid();
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
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

            _requestedState = _state;
            MakeCacheValid();
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
            OnStateChanged?.Invoke();
        }

        IReferenceFrame _cachedSceneReferenceFrame;
        Vector3 _lastCachedPosition = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );
        Quaternion _lastCachedRotation = new Quaternion( 0.21454141f, -23465435.352342f, 231.6354523f, 45.3412435f );

        protected void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            MakeCacheValid();
        }

        protected void RecalculateCache( IReferenceFrame sceneReferenceFrame )
        {
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        protected virtual bool IsCacheValid() => _cachedSceneReferenceFrame != null
            && (_rb.position.x == _lastCachedPosition.x && _rb.position.y == _lastCachedPosition.y && _rb.position.z == _lastCachedPosition.z)
            && (_rb.rotation.x == _lastCachedRotation.x && _rb.rotation.y == _lastCachedRotation.y && _rb.rotation.z == _lastCachedRotation.z && _rb.rotation.w == _lastCachedRotation.w)
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame );

        protected virtual void MakeCacheValid()
        {
            _lastCachedPosition = _rb.position;
            _lastCachedRotation = _rb.rotation;
        }

        protected virtual void MakeCacheInvalid() => _lastCachedPosition = _rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        public event Action OnStateChanged;

        //
        //
        //

        public float Mass { get; set; }

        public Vector3 LocalCenterOfMass { get; set; }

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
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            var referenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            _state.Acceleration += referenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );

            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            Vector3Dbl torque = Vector3Dbl.Cross( leverArm, force );
            if( torque.sqrMagnitude > 1e-6 )
                _state.AngularAcceleration += referenceFrame.TransformAngularAcceleration( torque / this.GetInertia( torque.NormalizeToVector3() ) );
        }

        public void AddTorque( Vector3 torque )
        {
            if( torque.sqrMagnitude < 1e-6 )
                return;

            _state.AngularAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularAcceleration( (Vector3Dbl)torque / this.GetInertia( torque.normalized ) );
        }

        public void AddAbsoluteForce( Vector3 force )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            _state.Acceleration += (Vector3Dbl)force / Mass;
        }

        public void AddAbsoluteForceAtPosition( Vector3 force, Vector3Dbl position )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            var referenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            _state.Acceleration += (Vector3Dbl)force / Mass;

            Vector3Dbl leverArm = position - referenceFrame.TransformPosition( this._rb.worldCenterOfMass );
            Vector3Dbl torque = Vector3Dbl.Cross( leverArm, force );
            if( torque.sqrMagnitude > 1e-6 )
                _state.AngularAcceleration += torque / this.GetInertia( torque.NormalizeToVector3() );
        }

        public void AddAbsoluteTorque( Vector3 torque )
        {
            if( torque.sqrMagnitude < 1e-6 )
                return;

            _state.AngularAcceleration += (Vector3Dbl)torque / this.GetInertia( torque.normalized );
        }

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
            _rb.isKinematic = true;
            _rb.drag = 0;
            _rb.angularDrag = 0;
            _rb.maxAngularVelocity = 9000;
        }

        protected virtual void FixedUpdate()
        {
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            Vector3Dbl absolutePosition = _state.Position;
            Vector3 scenePos = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( absolutePosition );
            _rb.position = scenePos;
            transform.position = scenePos;
            _state.Position = absolutePosition;

            QuaternionDbl absoluteRotation = _state.Rotation;
            Quaternion sceneRot = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( absoluteRotation );
            _rb.rotation = sceneRot;
            transform.rotation = sceneRot;
            _state.Rotation = absoluteRotation;
        }

        protected virtual void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            _activeKinematicTransforms.Add( this );
            _rb.isKinematic = true; // Force kinematic.
        }

        protected virtual void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
            _activeKinematicTransforms.Remove( this );
            _rb.isKinematic = true;
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


        private static List<KinematicReferenceFrameTransform> _activeKinematicTransforms = new();

        [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.PostFixedUpdate ) )] // we update it here (after all fixed behaviour updates)
                                                                                    // because otherwise the execution order might fuck things,
                                                                                    // and I don't want to change the order manually.
        public sealed class KinematicReferenceFrameTransformFixedUpdateSystem : IPlayerLoopSystem
        {
            public void Run()
            {
                foreach( var t in _activeKinematicTransforms )
                {
                    IReferenceFrame sceneReferenceFrameAfterPhysicsProcessing = t.SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT );

                    // _state.Position/Rotation should be up to date due to the callback inside physics step, which was invoked in the previous frame.

                    var vel = t._state.Velocity + t._state.Acceleration * TimeManager.FixedDeltaTime;
                    var angvel = t._state.AngularVelocity + t._state.AngularAcceleration * TimeManager.FixedDeltaTime;

                    t._requestedState.Position = t._state.Position + vel * TimeManager.FixedDeltaTime;
                    QuaternionDbl deltaRotation = QuaternionDbl.AngleAxis( angvel.magnitude * TimeManager.FixedDeltaTime * 57.29577951308232, angvel );
                    t._requestedState.Rotation = deltaRotation * t._state.Rotation;

                    var requestedPos = (Vector3)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformPosition( t._requestedState.Position );
                    var requestedRot = (Quaternion)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformRotation( t._requestedState.Rotation );

                    t._rb.Move( requestedPos, requestedRot );
                }
            }
        }

        [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.PhysicsStep ), Before = new[] { typeof( HSP.Trajectories.TrajectoryManager.TrajectoryManagerPostPhysicsStepSystem ) } )] // InsidePhysicsStep
        public sealed class KinematicReferenceFrameTransformSystem : IPlayerLoopSystem
        {
            public void Run()
            {
                // Assume that other objects aren't allowed to get the absolute position/velocity *in* the physics step, as it is undefined (changes) during it.
                foreach( var t in _activeKinematicTransforms )
                {
                    t._state.Velocity += t._state.Acceleration * TimeManager.FixedDeltaTime;
                    t._state.AngularVelocity += t._state.AngularAcceleration * TimeManager.FixedDeltaTime;

                    t._state.Acceleration = Vector3Dbl.zero;
                    t._state.AngularAcceleration = Vector3Dbl.zero;

                    t._state.Position = t._requestedState.Position;
                    t._state.Rotation = t._requestedState.Rotation;
                }
            }
        }

        [MapsInheritingFrom( typeof( KinematicReferenceFrameTransform ) )]
        public static IDescriptor FreePhysicsObjectMapping()
        {
            return new MemberwiseDescriptor<KinematicReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => true, ( o, value ) => o._rb.isKinematic = true ) // TODO - isKinematic member is a hack.

                .WithMember( "absolute_position", o => o.GetAbsolutePosition(), ( o, v ) => o.SetAbsolutePosition( v ) )
                .WithMember( "absolute_rotation", o => o.GetAbsoluteRotation(), ( o, v ) => o.SetAbsoluteRotation( v ) )
                .WithMember( "absolute_velocity", o => o.GetAbsoluteVelocity(), ( o, v ) => o.SetAbsoluteVelocity( v ) )
                .WithMember( "absolute_angular_velocity", o => o.GetAbsoluteAngularVelocity(), ( o, v ) => o.SetAbsoluteAngularVelocity( v ) );
        }
    }
}