using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is free to move around but doesn't respond to collisions (other objects can still collide with it).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class KinematicReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _rb.position;
            }
            set
            {
                _absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                MakeCacheInvalid();
                _rb.position = value;
                transform.position = value;
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
                _absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                MakeCacheInvalid();
                _rb.rotation = value;
                transform.rotation = value;
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
            }
        }

        public Vector3 Velocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedVelocity;
            }
            set
            {
                _absoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( value );
                MakeCacheInvalid();
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => _absoluteVelocity;
            set
            {
                _absoluteVelocity = value;
                MakeCacheInvalid();
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAngularVelocity;
            }
            set
            {
                _absoluteAngularVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( value );
                MakeCacheInvalid();
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => _absoluteAngularVelocity;
            set
            {
                _absoluteAngularVelocity = value;
                MakeCacheInvalid();
            }
        }

        public Vector3 Acceleration => _cachedAcceleration;
        public Vector3Dbl AbsoluteAcceleration => _cachedAbsoluteAcceleration;
        public Vector3 AngularAcceleration => _cachedAngularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration => _cachedAbsoluteAngularAcceleration;

        Vector3Dbl _absolutePosition;
        QuaternionDbl _absoluteRotation;
        Vector3Dbl _absoluteVelocity;
        Vector3Dbl _absoluteAngularVelocity;

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;
        //Vector3 _cachedPosition;
        //Quaternion _cachedRotation;
        Vector3 _cachedVelocity;
        Vector3 _cachedAngularVelocity;
        Vector3 _cachedAcceleration;
        Vector3Dbl _cachedAbsoluteAcceleration;
        Vector3 _cachedAngularAcceleration;
        Vector3Dbl _cachedAbsoluteAngularAcceleration;

        Vector3Dbl _oldAbsolutePosition;

        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;

        //
        //
        //

        public float Mass { get; set; }

        public Vector3 LocalCenterOfMass { get; set; }

        public Vector3 MomentsOfInertia { get; set; }

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
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );

            this._absoluteVelocity += (force / Mass) * TimeManager.FixedDeltaTime;
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            /*Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( Vector3Dbl.Cross( force, leverArm ) / Mass );

            // TODO - possibly cache the values across a frame and apply it once instead of n-times.
            this._rb.AddForceAtPosition( force / Mass, position, ForceMode.VelocityChange );*/
        }

        public void AddTorque( Vector3 torque )
        {
            //_absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( (Vector3Dbl)torque / Mass );

            //this._rb.AddTorque( torque / Mass, ForceMode.VelocityChange );
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
            // Don't cache acceleration, since it's impossible to compute it here for a dynamic body. Acceleration is recalculated on every fixedupdate instead.
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        // Exact comparison of the axes catches the most cases (and it's gonna be set to match exactly so it's okay)
        // Vector3's `==` operator does approximate comparison.
        private bool IsCacheValid() => (_absolutePosition.x == _oldAbsolutePosition.x && _absolutePosition.y == _oldAbsolutePosition.y && _absolutePosition.z == _oldAbsolutePosition.z
            && SceneReferenceFrameManager.ReferenceFrame.Equals( _cachedSceneReferenceFrame ));

        private void MakeCacheValid() => _oldAbsolutePosition = _absolutePosition;

        private void MakeCacheInvalid() => _oldAbsolutePosition = -_absolutePosition + new Vector3Dbl( 1234.56789, 12345678.9, 1.23456789 );

        void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( KinematicReferenceFrameTransform )} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        int i = 0;

        void FixedUpdate()
        {
            QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.2957795131 );
            _absolutePosition = _absolutePosition + _absoluteVelocity * TimeManager.FixedDeltaTime;
            _absoluteRotation = deltaRotation * _absoluteRotation;
            MoveScenePositionAndRotation( SceneReferenceFrameManager.ReferenceFrame );

            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
            _cachedAbsoluteAcceleration = _absoluteAccelerationSum;
            _cachedAbsoluteAngularAcceleration = _absoluteAngularAccelerationSum;

            _cachedAcceleration = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAcceleration( _cachedAbsoluteAcceleration );
            _cachedAngularAcceleration = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularAcceleration( _cachedAbsoluteAngularAcceleration );

            this._absoluteAccelerationSum = Vector3.zero;
            this._absoluteAngularAccelerationSum = Vector3.zero;
            i++;
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
            RecalculateCache( data.NewFrame );

            var vessel = FindObjectOfType<FreeReferenceFrameTransform>();
            var launchpad = FindObjectOfType<PinnedReferenceFrameTransform>().GetComponent<Rigidbody>();
            Debug.Log( "W" + TimeManager.UT + " :: " + this.AbsolutePosition.x + " :: " + (vessel.Position.x - launchpad.position.x) );
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

        [MapsInheritingFrom( typeof( KinematicReferenceFrameTransform ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<KinematicReferenceFrameTransform>()
            {
                ("mass", new Member<KinematicReferenceFrameTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<KinematicReferenceFrameTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<KinematicReferenceFrameTransform, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<KinematicReferenceFrameTransform, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<KinematicReferenceFrameTransform, QuaternionDbl>( o => o.AbsoluteRotation )),
                ("absolute_velocity", new Member<KinematicReferenceFrameTransform, Vector3Dbl>( o => o.AbsoluteVelocity )),
                ("absolute_angular_velocity", new Member<KinematicReferenceFrameTransform, Vector3Dbl>( o => o.AbsoluteAngularVelocity ))
            };
        }
    }
}