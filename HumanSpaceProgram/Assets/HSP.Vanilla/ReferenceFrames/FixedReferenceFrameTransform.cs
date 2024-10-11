using HSP.ReferenceFrames;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is fixed to a point in space and doesn't move (in the absolute frame).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FixedReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        Vector3Dbl _absolutePosition;
        QuaternionDbl _absoluteRotation;

        public Vector3 Position
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _rb.position;
            }
            set
            {
                this._absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
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
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, value );
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
                this._absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
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
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, value );
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
            set { }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => Vector3Dbl.zero;
            set { }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAngularVelocity;
            }
            set { }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => Vector3Dbl.zero;
            set { }
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

        public Vector3 MomentsOfInertia => this._rb.inertiaTensor;

        public Matrix3x3 MomentOfInertiaTensor
        {
            get
            {
                Matrix3x3 R = Matrix3x3.Rotate( this._rb.inertiaTensorRotation );
                Matrix3x3 S = Matrix3x3.Scale( this._rb.inertiaTensor );
                return R * S * R.transpose;
            }
            set
            {
                (Vector3 eigenvector, float eigenvalue)[] eigen = value.Diagonalize().OrderByDescending( m => m.eigenvalue ).ToArray();
                this._rb.inertiaTensor = new Vector3( eigen[0].eigenvalue, eigen[1].eigenvalue, eigen[2].eigenvalue );
                Matrix3x3 m = new Matrix3x3( eigen[0].eigenvector.x, eigen[0].eigenvector.y, eigen[0].eigenvector.z,
                    eigen[1].eigenvector.x, eigen[1].eigenvector.y, eigen[1].eigenvector.z,
                    eigen[2].eigenvector.x, eigen[2].eigenvector.y, eigen[2].eigenvector.z );
                this._rb.inertiaTensorRotation = m.rotation;
            }
        }

        public bool IsColliding { get; private set; }

        Rigidbody _rb;

        public void AddForce( Vector3 force )
        {
            return;
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            return;
        }

        public void AddTorque( Vector3 force )
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

            MoveScenePositionAndRotation( SceneReferenceFrameManager.ReferenceFrame );
            RecalculateCache( SceneReferenceFrameManager.ReferenceFrame );
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
            && SceneReferenceFrameManager.ReferenceFrame.EqualsIgnoreUT( _cachedSceneReferenceFrame );

        private void MakeCacheValid() => _oldPosition = _rb.position;

        private void MakeCacheInvalid() => _oldPosition = -_rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FixedReferenceFrameTransform )} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            MoveScenePositionAndRotation( SceneReferenceFrameManager.ReferenceFrame ); // Move, because the scene might be moving, and move ensures that the body is swept instead of teleported.
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // This one is already idempotent as it simply recalculates the same absolute values to scene space.
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
            RecalculateCache( data.NewFrame );
        }

        void OnEnable()
        {
            _rb.isKinematic = true; // Force kinematic.
        }

        void OnDisable()
        {
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
            {
                ("mass", new Member<FixedReferenceFrameTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FixedReferenceFrameTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FixedReferenceFrameTransform, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<FixedReferenceFrameTransform, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<FixedReferenceFrameTransform, QuaternionDbl>( o => o.AbsoluteRotation ))
            };
        }
    }
}