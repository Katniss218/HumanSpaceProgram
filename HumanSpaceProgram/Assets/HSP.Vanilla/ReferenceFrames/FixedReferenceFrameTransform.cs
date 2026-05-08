using HSP.ReferenceFrames;
using HSP.Time;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <summary>
    /// A physics transform that is fixed to a point in space and doesn't move (in the absolute frame).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public sealed class FixedReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
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
            // Force velocities and accelerations to zero for 'fixed' transform
            _state = new KinematicState( null, _state.Position, _state.Rotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
            MakeCacheValid();
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
            // Force velocities and accelerations to zero for 'fixed' transform
            _state = new KinematicState( null, _state.Position, _state.Rotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
            MakeCacheValid();
            OnStateChanged?.Invoke();
        }

        Vector3 _lastCachedPosition;

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;

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
            return; // 'Fixed' is always stationary.
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            return; // 'Fixed' is always stationary.
        }

        public void AddTorque( Vector3 torque )
        {
            return; // 'Fixed' is always stationary.
        }

        public void AddAbsoluteForce( Vector3 force )
        {
            return; // 'Fixed' is always stationary.
        }

        public void AddAbsoluteForceAtPosition( Vector3 force, Vector3Dbl position )
        {
            return; // 'Fixed' is always stationary.
        }

        public void AddAbsoluteTorque( Vector3 torque )
        {
            return; // 'Fixed' is always stationary.
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
            var pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( _state.Position );
            var rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( _state.Rotation );
            _rb.Move( pos, rot );
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        protected bool IsCacheValid() => _cachedSceneReferenceFrame != null
            && (_rb.position.x == _lastCachedPosition.x && _rb.position.y == _lastCachedPosition.y && _rb.position.z == _lastCachedPosition.z)
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame );

        protected void MakeCacheValid() => _lastCachedPosition = _rb.position;

        protected void MakeCacheInvalid() => _lastCachedPosition = -_rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {this.GetType().Name} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
            _rb.drag = 0;
            _rb.angularDrag = 0;
            _rb.maxAngularVelocity = 9000;
        }

        void FixedUpdate()
        {
            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT ) ); // Move, because the scene might be moving, and move ensures that the body is swept instead of teleported.
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, _rb, _state.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, _rb, _state.Rotation );
            // RecalculateCache handles updates, however we don't need it because properties compute on the fly
        }

        void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            _rb.isKinematic = true; // Force kinematic.
        }

        void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
            _rb.isKinematic = true;
        }

        void OnCollisionEnter( Collision collision )
        {
            IsColliding = true;
        }

        void OnCollisionStay( Collision collision )
        {
            // `OnCollisionEnter` / Exit are called for every collider.
            // I've tried using an incrementing/decrementing int with enter/exit, but it wasn't updating correctly, and after some time, there were too many collisions.
            // Using `OnCollisionStay` prevents desynchronization.

            IsColliding = true;
        }

        void OnCollisionExit( Collision collision )
        {
            IsColliding = false;
        }


        [MapsInheritingFrom( typeof( FixedReferenceFrameTransform ) )]
        public static IDescriptor FixedPhysicsObjectMapping()
        {
            return new MemberwiseDescriptor<FixedReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => true, ( o, value ) => o._rb.isKinematic = true )

                .WithMember( "absolute_position", o => o.GetAbsolutePosition(), ( o, v ) => o.SetAbsolutePosition( v ) )
                .WithMember( "absolute_rotation", o => o.GetAbsoluteRotation(), ( o, v ) => o.SetAbsoluteRotation( v ) );
        }
    }
}