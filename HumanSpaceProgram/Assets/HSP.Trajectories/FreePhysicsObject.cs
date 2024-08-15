using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories
{
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FreePhysicsObject : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get => this.transform.position;
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
            get => this.transform.rotation;
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
            get => this._rb.velocity;
            set
            {
                this._absoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( value );
                this._rb.velocity = value;
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => _absoluteVelocity;
            set
            {
                this._absoluteVelocity = value;
                ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( _rb, value );
            }
        }

        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set
            {
                this._rb.angularVelocity = value;
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => _absoluteAngularVelocity;
            set
            {
                this._absoluteAngularVelocity = value;
                ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( _rb, value );
            }
        }

        public Vector3 Acceleration => (Vector3)_acceleration;
        public Vector3Dbl AbsoluteAcceleration { get; private set; }

        public Vector3 AngularAcceleration => (Vector3)_angularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration { get; private set; }

        [SerializeField] Vector3Dbl _acceleration;
        [SerializeField] Vector3Dbl _angularAcceleration;

        [SerializeField] Vector3Dbl _absolutePosition;
        [SerializeField] QuaternionDbl _absoluteRotation;

        [SerializeField] Vector3Dbl _absoluteVelocity;
        [SerializeField] Vector3Dbl _absoluteAngularVelocity;

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;

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
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );

            this._rb.AddForce( force, ForceMode.Force );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( Vector3Dbl.Cross( force, leverArm ) / Mass );

            // TODO - possibly cache the values across a frame and apply it once instead of n-times.
            this._rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        public void AddTorque( Vector3 torque )
        {
            _absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( (Vector3Dbl)torque / Mass );

            this._rb.AddTorque( torque, ForceMode.Force );
        }

        private void MoveScenePositionAndRotation( IReferenceFrame referenceFrame )
        {
            var pos = (Vector3)referenceFrame.InverseTransformPosition( _absolutePosition );
            var rot = (Quaternion)referenceFrame.InverseTransformRotation( _absoluteRotation );
            this._rb.Move( pos, rot );

            var vel = (Vector3)referenceFrame.InverseTransformVelocity( _absoluteVelocity );
            var angVel = (Vector3)referenceFrame.InverseTransformAngularVelocity( _absoluteAngularVelocity );
            this._rb.velocity = vel;
            this._rb.angularVelocity = angVel;
        }

        private void RecalculateAbsoluteValues( IReferenceFrame referenceFrame )
        {
            this._absolutePosition = referenceFrame.TransformPosition( this._rb.position );
            this._absoluteRotation = referenceFrame.TransformRotation( this._rb.rotation );
            this._absoluteVelocity = referenceFrame.TransformVelocity( this._rb.velocity );
            this._absoluteAngularVelocity = referenceFrame.TransformAngularVelocity( this._rb.angularVelocity );

            this.AbsoluteAcceleration = referenceFrame.TransformAcceleration( this.Acceleration );
            this.AbsoluteAngularAcceleration = referenceFrame.TransformAcceleration( this.AngularAcceleration );
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FreePhysicsObject )} to a game object that already has a {nameof( IPhysicsTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = false;
        }

        void FixedUpdate()
        {
#warning TODO - gravity code doesn't belong here.
            Vector3Dbl airfGravityForce = GravityUtils.GetNBodyGravityForce( this.AbsolutePosition, this.Mass );
            this.AddForce( (Vector3)airfGravityForce );

            if( SceneReferenceFrameManager.ReferenceFrame is INonInertialReferenceFrame frame )
            {
                Vector3Dbl localPos = frame.InverseTransformPosition( this.AbsolutePosition );
                Vector3Dbl localVel = this.Velocity;
                Vector3Dbl localAngVel = this.AngularVelocity;
                Vector3 linAcc = (Vector3)frame.GetFicticiousAcceleration( localPos, localVel );
                Vector3 angAcc = (Vector3)frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                this._acceleration += linAcc;
                this._angularAcceleration += angAcc;
                this._rb.AddForce( linAcc, ForceMode.Acceleration );
                this._rb.AddTorque( angAcc, ForceMode.Acceleration );
            }


            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                this._acceleration = ((Vector3Dbl)(Velocity - _oldVelocity)) / TimeManager.FixedDeltaTime;
                this._angularAcceleration = ((Vector3Dbl)(AngularVelocity - _oldAngularVelocity)) / TimeManager.FixedDeltaTime;
            }
            else
            {
                // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
                this._acceleration = _absoluteAccelerationSum;
                this._angularAcceleration = _absoluteAngularAccelerationSum;
            }

            RecalculateAbsoluteValues( SceneReferenceFrameManager.ReferenceFrame );

            this._oldVelocity = Velocity;
            this._oldAngularVelocity = AngularVelocity;
            this._absoluteAccelerationSum = Vector3.zero;
            this._absoluteAngularAccelerationSum = Vector3.zero;
        }

        // The faster something goes in scene space when colliding with another thing, it gets laggier for physics processing (contacts creation)

        // when switching while resting on something, the object jumps. possibly due to pinned updating before the celestial frame it uses to transform has correct values or something?
        // possibly the same or related to rovers in RSS/RO jumping while driving
        // - only happens with continuous speculative collision (continuous and continuous dynamic don't jump).

#warning TODO - needs something to enable continuous when a something in the scene is not resting and is moving fast relative to something else.

#warning TODO - celestial bodies need something that will replace the buildin parenting of colliders with 64-bit parents and update their scene position at all times (fixedupdate + update + lateupdate).

#warning TODO - atmosphere renderer(s) need to be attached to a body and follow it.

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            RecalculateAbsoluteValues( data.OldFrame );

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
            ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( _rb, _absoluteVelocity );
            ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( _rb, _absoluteAngularVelocity );
        }

        void OnEnable()
        {
            _rb.isKinematic = false; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
        }

        void OnDisable()
        {
            _rb.isKinematic = true; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
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

        [MapsInheritingFrom( typeof( FreePhysicsObject ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FreePhysicsObject>()
            {
                ("mass", new Member<FreePhysicsObject, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FreePhysicsObject, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FreePhysicsObject, bool>( o => false, (o, value) => o._rb.isKinematic = false)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<FreePhysicsObject, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<FreePhysicsObject, QuaternionDbl>( o => o.AbsoluteRotation )),
                ("absolute_velocity", new Member<FreePhysicsObject, Vector3Dbl>( o => o.AbsoluteVelocity )),
                ("absolute_angular_velocity", new Member<FreePhysicsObject, Vector3Dbl>( o => o.AbsoluteAngularVelocity ))
            };
        }
    }
}