using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is free to move around and respond to forces, but doesn't respond to collisions (other objects can still collide with it).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class KinematicReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get
            {
                return _rb.position; // Rigidbody value is synchronized with absolute values so we can use it directly and both will be consistnent.
            }
            set
            {
                AbsolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get
            {
                // To synchronize, we keep track of where the rigidbody is in fixedupdate and where it is going to be after physicsprocessing (at the end of fixed frame)
                // We return whichever value matches.
                return _rb.position == _actualPosition
                        ? _actualAbsolutePosition
                        : _requestedAbsolutePosition;
            }
            set
            {
                // When setting, both values are set.
                Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( value );
                _rb.position = scenePos;
                transform.position = scenePos;
                _actualPosition = scenePos;
                _actualAbsolutePosition = value;
                _requestedAbsolutePosition = value;

                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return _rb.rotation; // Rigidbody value is synchronized with absolute values so we can use it directly and both will be consistnent.
            }
            set
            {
                AbsoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                return _rb.rotation == _actualRotation
                        ? _actualAbsoluteRotation
                        : _requestedAbsoluteRotation;
            }
            set
            {
                Quaternion sceneRot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( value );
                _rb.rotation = sceneRot;
                transform.rotation = sceneRot;
                _actualRotation = sceneRot;
                _actualAbsoluteRotation = value;
                _requestedAbsoluteRotation = value;

                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return _rb.velocity;
            }
            set
            {
                _absoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( value );

                OnAbsoluteVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                return _absoluteVelocity;
            }
            set
            {
                _absoluteVelocity = value;

                OnAbsoluteVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                return _rb.angularVelocity;
            }
            set
            {
                _absoluteAngularVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( value );

                OnAbsoluteAngularVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                return _absoluteAngularVelocity;
            }
            set
            {
                _absoluteAngularVelocity = value;

                OnAbsoluteAngularVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 Acceleration => _cachedAcceleration;
        public Vector3Dbl AbsoluteAcceleration => _cachedAbsoluteAcceleration;
        public Vector3 AngularAcceleration => _cachedAngularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration => _cachedAbsoluteAngularAcceleration;

        Vector3 _actualPosition;
        Vector3Dbl _actualAbsolutePosition;
        Vector3Dbl _requestedAbsolutePosition;

        Quaternion _actualRotation = Quaternion.identity;
        QuaternionDbl _actualAbsoluteRotation = QuaternionDbl.identity;
        QuaternionDbl _requestedAbsoluteRotation = QuaternionDbl.identity;

        Vector3Dbl _absoluteVelocity;
        Vector3Dbl _absoluteAngularVelocity;

        Vector3 _cachedAcceleration;
        Vector3Dbl _cachedAbsoluteAcceleration;
        Vector3 _cachedAngularAcceleration;
        Vector3Dbl _cachedAbsoluteAngularAcceleration;

        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;


        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

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

        protected new Rigidbody rigidbody => _rb;
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

        protected virtual void Awake()
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

        protected virtual void FixedUpdate()
        {
            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameManager.ReferenceFrame;
            IReferenceFrame sceneReferenceFrameAfterPhysicsProcessing = SceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT );

            _actualAbsolutePosition = _requestedAbsolutePosition;
            _actualAbsoluteRotation = _requestedAbsoluteRotation;

            QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.2957795131 );
            _requestedAbsolutePosition = _actualAbsolutePosition + _absoluteVelocity * TimeManager.FixedDeltaTime;
            _requestedAbsoluteRotation = deltaRotation * _actualAbsoluteRotation;

            // Queue Move Rigidbody To Requested
            //var pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( _actualAbsolutePosition );
            //var rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( _actualAbsoluteRotation );

            var requestedPos = (Vector3)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformPosition( _requestedAbsolutePosition );
            var requestedRot = (Quaternion)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformRotation( _requestedAbsoluteRotation );
            
            _rb.Move( requestedPos, requestedRot );
            _actualPosition = _rb.position;
            _actualRotation = _rb.rotation;


            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
            _cachedAbsoluteAcceleration = _absoluteAccelerationSum;
            _cachedAbsoluteAngularAcceleration = _absoluteAngularAccelerationSum;

            _cachedAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAcceleration( _cachedAbsoluteAcceleration );
            _cachedAngularAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAngularAcceleration( _cachedAbsoluteAngularAcceleration );

            this._absoluteAccelerationSum = Vector3.zero;
            this._absoluteAngularAccelerationSum = Vector3.zero;
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            AbsolutePosition = AbsolutePosition;
            AbsoluteRotation = AbsoluteRotation;
        }

        protected virtual void OnEnable()
        {
            _rb.isKinematic = true; // Force kinematic.
        }

        protected virtual void OnDisable()
        {
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