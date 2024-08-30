using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <remarks>
    /// A physics transform that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
    /// </remarks>
	[RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class PinnedReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        CelestialBody _referenceBody = null;
        Vector3Dbl _referencePosition = Vector3.zero;
        QuaternionDbl _referenceRotation = QuaternionDbl.identity;

        public CelestialBody ReferenceBody
        {
            get => _referenceBody;
            set
            {
                _referenceBody = value;
                if( value != null )
                {
                    MakeCacheInvalid();
                    ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
                    ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
                }
            }
        }

        public Vector3Dbl ReferencePosition
        {
            get => _referencePosition;
            set
            {
                _referencePosition = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
            }
        }

        public QuaternionDbl ReferenceRotation
        {
            get => _referenceRotation;
            set
            {
                _referenceRotation = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
            }
        }

        public void SetReference( CelestialBody referenceBody, Vector3Dbl referencePosition, QuaternionDbl referenceRotation )
        {
            _referenceBody = referenceBody;
            _referencePosition = referencePosition;
            _referenceRotation = referenceRotation;
            MakeCacheInvalid();
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
        }


        public Vector3 Position
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _rb.position;
            }
            set
            {
                Vector3Dbl absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                ReferencePosition = _referenceBody.OrientedReferenceFrame.InverseTransformPosition( absolutePosition );
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
                QuaternionDbl absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                ReferenceRotation = _referenceBody.OrientedReferenceFrame.InverseTransformRotation( absoluteRotation );
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsolutePosition;
            }
            set
            {
                ReferencePosition = _referenceBody.OrientedReferenceFrame.InverseTransformPosition( value );
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteRotation;
            }
            set
            {
                ReferenceRotation = _referenceBody.OrientedReferenceFrame.InverseTransformRotation( value );
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
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteVelocity;
            }
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
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteAngularVelocity;
            }
            set { }
        }

        public Vector3 Acceleration => _cachedAcceleration;
        public Vector3Dbl AbsoluteAcceleration => _cachedAbsoluteAcceleration;
        public Vector3 AngularAcceleration => _cachedAngularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration => _cachedAbsoluteAngularAcceleration;

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;
        //Vector3 _cachedPosition;
        Vector3Dbl _cachedAbsolutePosition;
        //Quaternion _cachedRotation;
        QuaternionDbl _cachedAbsoluteRotation;
        Vector3 _cachedVelocity;
        Vector3Dbl _cachedAbsoluteVelocity;
        Vector3 _cachedAngularVelocity;
        Vector3Dbl _cachedAbsoluteAngularVelocity;
        Vector3 _cachedAcceleration;
        Vector3Dbl _cachedAbsoluteAcceleration;
        Vector3 _cachedAngularAcceleration;
        Vector3Dbl _cachedAbsoluteAngularAcceleration;

        Vector3 _oldPosition;

        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;

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
            if( ReferenceBody == null )
                return;

            var bodyFrame = ReferenceBody.OrientedReferenceFrame;
            var pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( bodyFrame.TransformPosition( _referencePosition ) );
            var rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( bodyFrame.TransformRotation( _referenceRotation ) );
            _rb.Move( pos, rot );
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

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
            IReferenceFrame bodyFrame = _referenceBody.OrientedReferenceFrame;
            _cachedAbsolutePosition = bodyFrame.TransformPosition( _referencePosition );
            _cachedAbsoluteRotation = bodyFrame.TransformRotation( _referenceRotation );
            _cachedAbsoluteVelocity = bodyFrame.TransformVelocity( Vector3Dbl.zero );
            _cachedAbsoluteAngularVelocity = bodyFrame.TransformAngularVelocity( Vector3Dbl.zero );

            if( bodyFrame is INonInertialReferenceFrame nonInertialBodyFrame )
            {
                _cachedAbsoluteVelocity += nonInertialBodyFrame.GetTangentialVelocity( _referencePosition );
            }

            _cachedVelocity = (Vector3)sceneReferenceFrame.InverseTransformAngularVelocity( _cachedAbsoluteVelocity );
            _cachedAngularVelocity = (Vector3)sceneReferenceFrame.InverseTransformAngularVelocity( _cachedAbsoluteVelocity );

            _cachedAbsoluteAcceleration = bodyFrame.TransformAcceleration( Vector3Dbl.zero );
            _cachedAbsoluteAngularAcceleration = bodyFrame.TransformAngularAcceleration( Vector3Dbl.zero );
            _cachedAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAngularAcceleration( _cachedAbsoluteAcceleration );
            _cachedAngularAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAngularAcceleration( _cachedAbsoluteAngularAcceleration );
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        // Exact comparison of the axes catches the most cases (and it's gonna be set to match exactly so it's okay)
        // Vector3's `==` operator does approximate comparison.
        private bool IsCacheValid() => (_rb.position.x == _oldPosition.x && _rb.position.y == _oldPosition.y && _rb.position.z == _oldPosition.z);

        private void MakeCacheValid() => _oldPosition = _rb.position;

        private void MakeCacheInvalid() => _oldPosition = -_rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( PinnedReferenceFrameTransform )} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
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
            // Move, because the scene (or reference body) might be moving, and move ensures that the body is swept instead of teleported.
            this.MoveScenePositionAndRotation( SceneReferenceFrameManager.ReferenceFrame );
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // `_referenceBody.OrientedReferenceFrame` Guarantees up-to-date reference frame, regardless of update order.
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
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

        [MapsInheritingFrom( typeof( PinnedReferenceFrameTransform ) )]
        public static SerializationMapping PinnedPhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<PinnedReferenceFrameTransform>()
            {
                ("mass", new Member<PinnedReferenceFrameTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<PinnedReferenceFrameTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<PinnedReferenceFrameTransform, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("reference_body", new Member<PinnedReferenceFrameTransform, string>( o => o.ReferenceBody == null ? null : o.ReferenceBody.ID, (o, value) => o.ReferenceBody = value == null ? null : CelestialBodyManager.Get( value ) )),
                ("reference_position", new Member<PinnedReferenceFrameTransform, Vector3Dbl>( o => o.ReferencePosition )),
                ("reference_rotation", new Member<PinnedReferenceFrameTransform, QuaternionDbl>( o => o.ReferenceRotation ))
            };
        }
    }
}