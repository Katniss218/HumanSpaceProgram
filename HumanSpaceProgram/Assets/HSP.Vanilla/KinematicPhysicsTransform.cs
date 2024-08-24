using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is free to move around but doesn't respond to collisions (other objects can still collide with it).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class KinematicPhysicsTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( _absolutePosition );
            set
            {
                this._absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                this._rb.position = value;
                this.transform.position = value;
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get => _absolutePosition;
            set
            {
                _absolutePosition = value;
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, value );
            }
        }

        public Quaternion Rotation
        {
            get => (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( _absoluteRotation );
            set
            {
                this._absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                this._rb.rotation = value;
                this.transform.rotation = value;
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get => _absoluteRotation;
            set
            {
                _absoluteRotation = value;
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, value );
            }
        }

        public Vector3 Velocity
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( _absoluteVelocity );
            set
            {
                this._absoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( value );
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => _absoluteVelocity;
            set
            {
                this._absoluteVelocity = value;
            }
        }

        public Vector3 AngularVelocity
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( _absoluteAngularVelocity );
            set
            {
                this._absoluteAngularVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( value );
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => _absoluteAngularVelocity;
            set
            {
                this._absoluteAngularVelocity = value;
            }
        }

        public Vector3 Acceleration => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAcceleration( this._absoluteAcceleration );
        public Vector3Dbl AbsoluteAcceleration => this._absoluteAcceleration;

        public Vector3 AngularAcceleration => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAcceleration( this._absoluteAngularAcceleration );
        public Vector3Dbl AbsoluteAngularAcceleration => this._absoluteAngularAcceleration;

        [SerializeField] Vector3Dbl _absoluteAcceleration;
        [SerializeField] Vector3Dbl _absoluteAngularAcceleration;

        [SerializeField] Vector3Dbl _absolutePosition;
        [SerializeField] QuaternionDbl _absoluteRotation;

        [SerializeField] Vector3Dbl _absoluteVelocity;
        [SerializeField] Vector3Dbl _absoluteAngularVelocity;

        Vector3Dbl _oldAbsoluteVelocity;
        Vector3Dbl _oldAbsoluteAngularVelocity;

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

        private void MoveScenePositionAndRotation( IReferenceFrame referenceFrame )
        {
            var pos = (Vector3)referenceFrame.InverseTransformPosition( _absolutePosition );
            var rot = (Quaternion)referenceFrame.InverseTransformRotation( _absoluteRotation );
            this._rb.Move( pos, rot );

            //var vel = (Vector3)referenceFrame.InverseTransformVelocity( _absoluteVelocity );
            //var angVel = (Vector3)referenceFrame.InverseTransformAngularVelocity( _absoluteAngularVelocity );
            //this._rb.velocity = vel;
            //this._rb.angularVelocity = angVel;
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FreePhysicsTransform )} to a game object that already has a {nameof( IPhysicsTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.2957795131 );
            _absolutePosition = _absolutePosition + _absoluteVelocity * TimeManager.FixedDeltaTime;
            _absoluteRotation = deltaRotation * _absoluteRotation;
            MoveScenePositionAndRotation( SceneReferenceFrameManager.ReferenceFrame );
            //ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            //ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );

            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                this._absoluteAcceleration = (Velocity - _oldAbsoluteVelocity) / TimeManager.FixedDeltaTime;
                this._absoluteAngularAcceleration = (AngularVelocity - _oldAbsoluteAngularVelocity) / TimeManager.FixedDeltaTime;
            }
            else
            {
                // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
                this._absoluteAcceleration = _absoluteAccelerationSum;
                this._absoluteAngularAcceleration = _absoluteAngularAccelerationSum;
            }

            this._oldAbsoluteVelocity = AbsoluteVelocity;
            this._oldAbsoluteAngularVelocity = AbsoluteAngularVelocity;
            this._absoluteAccelerationSum = Vector3.zero;
            this._absoluteAngularAccelerationSum = Vector3.zero;
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
#warning TODO - after fixedupdate the position is "higher", but before, it's the same as here?

#warning TODO - THAT'S IT
            // reference frame switching code is called before the vessel's absolute position had a chance to recache itself.
            // recaching everything upon calling fixes it. but we shouldn't need to cache all the time, that's expensive.

#warning TODO - and kinematic needs the same recalc-on-demand treatment for local scene position values.

            // I think this also means the line in pinned phys transform is unnecessary, because it should recalculate itself on demand now.

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
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

        [MapsInheritingFrom( typeof( KinematicPhysicsTransform ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<KinematicPhysicsTransform>()
            {
                ("mass", new Member<KinematicPhysicsTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<KinematicPhysicsTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<KinematicPhysicsTransform, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<KinematicPhysicsTransform, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<KinematicPhysicsTransform, QuaternionDbl>( o => o.AbsoluteRotation )),
                ("absolute_velocity", new Member<KinematicPhysicsTransform, Vector3Dbl>( o => o.AbsoluteVelocity )),
                ("absolute_angular_velocity", new Member<KinematicPhysicsTransform, Vector3Dbl>( o => o.AbsoluteAngularVelocity ))
            };
        }
    }
}