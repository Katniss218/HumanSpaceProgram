using HSP.ReferenceFrames;
using HSP.Time;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
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

        Vector3Dbl _absolutePosition = Vector3Dbl.zero;
        QuaternionDbl _absoluteRotation = QuaternionDbl.identity;

        public Vector3 Position
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _rb.position;
            }
            set
            {
                this._absolutePosition = SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( value );
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _absolutePosition );
                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get => _absolutePosition;
            set
            {
                _absolutePosition = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, value );
                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Quaternion Rotation
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _rb.rotation;
            }
            set
            {
                this._absoluteRotation = SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformRotation( value );
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _absoluteRotation );
                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get => _absoluteRotation;
            set
            {
                _absoluteRotation = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, value );
                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }


        public Vector3 Velocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedVelocity;
            }
            set { } // 'Fixed' is always stationary, so it makes no sense to 'set' it to anything.
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => Vector3Dbl.zero;
            set { } // 'Fixed' is always stationary, so it makes no sense to 'set' it to anything.
        }

        public Vector3 AngularVelocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAngularVelocity;
            }
            set { } // 'Fixed' is always stationary, so it makes no sense to 'set' it to anything.
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => Vector3Dbl.zero;
            set { } // 'Fixed' is always stationary, so it makes no sense to 'set' it to anything.
        }

        public Vector3 Acceleration
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAcceleration;
            }
        }

        public Vector3Dbl AbsoluteAcceleration
        {
            get => Vector3Dbl.zero;
        }

        public Vector3 AngularAcceleration
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAngularAcceleration;
            }
        }

        public Vector3Dbl AbsoluteAngularAcceleration
        {
            get => Vector3Dbl.zero;
        }

        Vector3 _oldPosition;

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;
        //Vector3 _cachedPosition;
        //Quaternion _cachedRotation;
        Vector3 _cachedVelocity;
        Vector3 _cachedAngularVelocity;
        Vector3 _cachedAcceleration;
        Vector3 _cachedAngularAcceleration;


        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

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

        public void AddTorque( Vector3 force )
        {
            return; // 'Fixed' is always stationary.
        }

        public void AddAbsoluteForce( Vector3 force )
        {
            return;
        }

        public void AddAbsoluteForceAtPosition( Vector3 force, Vector3Dbl position )
        {
            return;
        }

        public void AddAbsoluteTorque( Vector3 force )
        {
            return;
        }

        private void MoveScenePositionAndRotation( IReferenceFrame sceneReferenceFrame )
        {
            var pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( _absolutePosition );
            var rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( _absoluteRotation );
            _rb.Move( pos, rot );
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        /// <summary>
        /// Checks if the cacheable values need to be recalculated, and recalculates them if needed.
        /// </summary>
        private void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            MoveScenePositionAndRotation( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            MakeCacheValid();
        }

        private void RecalculateCache( IReferenceFrame sceneReferenceFrame )
        {
            _cachedVelocity = (Vector3)sceneReferenceFrame.InverseTransformVelocity( AbsoluteVelocity );
            _cachedAngularVelocity = (Vector3)sceneReferenceFrame.InverseTransformAngularVelocity( AbsoluteAngularVelocity );
            _cachedAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAcceleration( AbsoluteAcceleration );
            _cachedAngularAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAngularAcceleration( AbsoluteAngularAcceleration );
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        // Exact comparison of the axes catches the most cases (and it's gonna be set to match exactly so it's okay)
        // Vector3's `==` operator does approximate comparison.
        private bool IsCacheValid() => (_rb.position.x == _oldPosition.x && _rb.position.y == _oldPosition.y && _rb.position.z == _oldPosition.z)
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame );

        private void MakeCacheValid() => _oldPosition = _rb.position;

        private void MakeCacheInvalid() => _oldPosition = -_rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

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
        }

        void FixedUpdate()
        {
            MoveScenePositionAndRotation( SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT ) ); // Move, because the scene might be moving, and move ensures that the body is swept instead of teleported.
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
#warning TODO - let the ReferenceFrameTransformUtils.SetScenePositionFromAbsolute use a custom frame. It's equal, but it would be better to use the event data.
            // This one is already idempotent as it simply recalculates the same absolute values to scene space.
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, _rb, _absoluteRotation );
            RecalculateCache( data.NewFrame );
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
        public static SerializationMapping FixedPhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FixedReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => true, ( o, value ) => o._rb.isKinematic = true )

                .WithMember( "absolute_position", o => o.AbsolutePosition )
                .WithMember( "absolute_rotation", o => o.AbsoluteRotation );
        }
    }
}