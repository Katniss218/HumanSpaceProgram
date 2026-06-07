using HSP.ReferenceFrames;
using HSP.Time;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;
using Ctx = UnityPlus.Serialization.Ctx;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <remarks>
    /// A physics transform that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
    /// </remarks>
	[RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class PinnedReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
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

        IReferenceFrameTransform _referenceTransform = null;
        Vector3Dbl _referencePosition = Vector3.zero;
        QuaternionDbl _referenceRotation = QuaternionDbl.identity;

        public IReferenceFrameTransform ReferenceTransform
        {
            get => _referenceTransform;
            set
            {
                _referenceTransform = value;
                MakeCacheInvalid();
                SetPositionAndRotation();
                OnStateChanged?.Invoke();
            }
        }

        public Vector3Dbl ReferencePosition
        {
            get => _referencePosition;
            set
            {
                _referencePosition = value;
                MakeCacheInvalid();
                SetPositionAndRotation();
                OnStateChanged?.Invoke();
            }
        }

        public QuaternionDbl ReferenceRotation
        {
            get => _referenceRotation;
            set
            {
                _referenceRotation = value;
                MakeCacheInvalid();
                SetPositionAndRotation();
                OnStateChanged?.Invoke();
            }
        }

        public void SetReference( IReferenceFrameTransform referenceTransform, Vector3Dbl referencePosition, QuaternionDbl referenceRotation )
        {
            _referenceTransform = referenceTransform;
            _referencePosition = referencePosition;
            _referenceRotation = referenceRotation;
            MakeCacheInvalid();
            SetPositionAndRotation();
            OnStateChanged?.Invoke();
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
            ReferencePosition = _referenceTransform == null ? _state.Position : _referenceTransform.OrientedReferenceFrame().InverseTransformPosition( _state.Position );
            ReferenceRotation = _referenceTransform == null ? _state.Rotation : _referenceTransform.OrientedReferenceFrame().InverseTransformRotation( _state.Rotation );
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
            ReferencePosition = _referenceTransform == null ? _state.Position : _referenceTransform.OrientedReferenceFrame().InverseTransformPosition( _state.Position );
            ReferenceRotation = _referenceTransform == null ? _state.Rotation : _referenceTransform.OrientedReferenceFrame().InverseTransformRotation( _state.Rotation );
            MakeCacheValid();
            OnStateChanged?.Invoke();
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
            IReferenceFrame bodyFrame = _referenceTransform == null
                ? new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero )
                : _referenceTransform.NonInertialReferenceFrame(); // Needs to be a non-inertial frame to have angular velocity.

            _state.Position = bodyFrame.TransformPosition( _referencePosition );
            _state.Rotation = bodyFrame.TransformRotation( _referenceRotation );
            _state.Velocity = bodyFrame.TransformVelocity( Vector3Dbl.zero );
            _state.AngularVelocity = bodyFrame.TransformAngularVelocity( Vector3Dbl.zero );

            if( bodyFrame is INonInertialReferenceFrame nirf )
            {
                _state.Velocity += nirf.GetTangentialVelocity( _referencePosition );
            }

            _state.Acceleration = bodyFrame.TransformAcceleration( Vector3Dbl.zero );
            _state.AngularAcceleration = bodyFrame.TransformAngularAcceleration( Vector3Dbl.zero );
            _cachedSceneReferenceFrame = sceneReferenceFrame;

            if( _referenceTransform != null )
            {
                _lastCachedRefPosition = _referenceTransform.GetAbsolutePosition();
                _lastCachedRefRotation = _referenceTransform.GetAbsoluteRotation();
            }
        }

        Vector3Dbl _lastCachedRefPosition;
        QuaternionDbl _lastCachedRefRotation;
        IReferenceFrame _cachedSceneReferenceFrame;
        double _lastCachedUT = -1;

        protected virtual bool IsCacheValid() => _cachedSceneReferenceFrame != null
            && TimeManager.UT == _lastCachedUT
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame )
            && (_referenceTransform == null
                || (_lastCachedRefPosition.x == _referenceTransform.GetAbsolutePosition().x
                    && _lastCachedRefPosition.y == _referenceTransform.GetAbsolutePosition().y
                    && _lastCachedRefPosition.z == _referenceTransform.GetAbsolutePosition().z
                    && _lastCachedRefRotation.x == _referenceTransform.GetAbsoluteRotation().x
                    && _lastCachedRefRotation.y == _referenceTransform.GetAbsoluteRotation().y
                    && _lastCachedRefRotation.z == _referenceTransform.GetAbsoluteRotation().z
                    && _lastCachedRefRotation.w == _referenceTransform.GetAbsoluteRotation().w));

        protected virtual void MakeCacheValid()
        {
            _lastCachedUT = TimeManager.UT;
        }

        protected virtual void MakeCacheInvalid() => _lastCachedUT = -1;

        public event Action OnStateChanged;

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
            return;
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            return;
        }

        public void AddTorque( Vector3 torque )
        {
            return;
        }

        public void AddAbsoluteForce( Vector3 force )
        {
            return;
        }

        public void AddAbsoluteForceAtPosition( Vector3 force, Vector3Dbl position )
        {
            return;
        }

        public void AddAbsoluteTorque( Vector3 torque )
        {
            return;
        }

        private void SetPositionAndRotation()
        {
            IReferenceFrame bodyFrame = _referenceTransform == null
                ? new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero )
                : _referenceTransform.OrientedInertialReferenceFrame();

            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            Vector3 pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( bodyFrame.TransformPosition( _referencePosition ) );
            Quaternion rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( bodyFrame.TransformRotation( _referenceRotation ) );

            _rb.position = pos;
            transform.position = pos;

            _rb.rotation = rot;
            transform.rotation = rot;
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
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
            _rb.drag = 0;
            _rb.angularDrag = 0;
            _rb.maxAngularVelocity = 9000;
        }

        protected virtual void FixedUpdate()
        {
            IReferenceFrame bodyFrame = _referenceTransform == null
                ? new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero )
                : _referenceTransform.NonInertialReferenceFrame().AtUT( TimeManager.UT ); // moves to where the body will be, but this might be inaccurate
                                                                                          // - ideally we need to get the actual target pos just before physics step?
                                                                                          // - (if something runs in fixed update after this runs, then it can change the returned reference frame and make it no longer valid)

            // ReferenceFrame.AtUT is used because we want to access the frame for the end of the frame, and FixedUpdate (caller) is called before ReferenceFrame updates.
            var sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT );

            Vector3 pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( bodyFrame.TransformPosition( _referencePosition ) );
            Quaternion rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( bodyFrame.TransformRotation( _referenceRotation ) );
            _rb.Move( pos, rot );
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // `_referenceBody.OrientedReferenceFrame` Guarantees up-to-date reference frame, regardless of update order.

            SetPositionAndRotation();
        }

        protected virtual void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            _rb.isKinematic = true; // Force kinematic.
        }

        protected virtual void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
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

        [MapsInheritingFrom( typeof( PinnedReferenceFrameTransform ) )]
        public static IDescriptor PinnedPhysicsObjectMapping()
        {
            return new MemberwiseDescriptor<PinnedReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => true, ( o, value ) => o._rb.isKinematic = true )

                .WithMember( "reference_transform", typeof( Ctx.Ref ), o => o.ReferenceTransform )
                .WithMember( "reference_position", o => o.ReferencePosition )
                .WithMember( "reference_rotation", o => o.ReferenceRotation );
        }
    }
}