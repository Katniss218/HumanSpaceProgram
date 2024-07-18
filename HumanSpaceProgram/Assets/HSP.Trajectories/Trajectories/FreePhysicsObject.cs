using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Core.Physics
{
    /// <remarks>
    /// This is a wrapper for a rigidbody.
    /// </remarks>
    [RequireComponent( typeof( ReferenceFrameTransform ) )]
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FreePhysicsObject : MonoBehaviour, IPhysicsObject
    {
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

        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set => this._rb.velocity = value;
        }

        public Vector3 Acceleration { get; private set; }

        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set => this._rb.angularVelocity = value;
        }

        public Vector3 AngularAcceleration { get; private set; }

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

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3 _accelerationSum = Vector3.zero;
        Vector3 _angularAccelerationSum = Vector3.zero;

        ReferenceFrameTransform _rootObjTransform;
        Rigidbody _rb;

        public void AddForce( Vector3 force )
        {
            _accelerationSum += force / Mass;
            this._rb.AddForce( force, ForceMode.Force );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            // I think force / mass is still correct for this,
            // - because 2 consecutive impulses in the same direction, but opposite positions (so that the angular accelerations cancel out)
            // - should still produce linear acceleration of 2 * force / mass.
            _accelerationSum += force / Mass;

            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _angularAccelerationSum += Vector3.Cross( force, leverArm ) / Mass;

            // TODO - possibly cache the values across a frame and apply it once instead of n-times.
            this._rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        public void AddTorque( Vector3 force )
        {
            _accelerationSum += force / Mass;
            this._rb.AddTorque( force, ForceMode.Force );
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsObject>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FreePhysicsObject )} to a game object that already has a {nameof( IPhysicsObject )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();
            _rootObjTransform = this.gameObject.GetOrAddComponent<ReferenceFrameTransform>();
            _rootObjTransform.RefreshCachedRigidbody();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = false;
        }

        void FixedUpdate()
        {
#warning TODO - this should act on the trajectory, vessel shouldn't care.
            Vector3Dbl airfGravityForce = GravityUtils.GetNBodyGravityForce( this._rootObjTransform.AIRFPosition, this.Mass );
            this.AddForce( (Vector3)airfGravityForce );


            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                this.Acceleration = (Velocity - _oldVelocity) / TimeManager.FixedDeltaTime;
                this.AngularAcceleration = (AngularVelocity - _oldAngularVelocity) / TimeManager.FixedDeltaTime;
            }
            else
            {
                // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
                this.Acceleration = _accelerationSum;
                this.AngularAcceleration = _angularAccelerationSum;
            }

            this._oldVelocity = Velocity;
            this._oldAngularVelocity = AngularVelocity;
            this._accelerationSum = Vector3.zero;
            this._angularAccelerationSum = Vector3.zero;
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

        void OnEnable()
        {
            _rb.isKinematic = false; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
        }

        void OnDisable()
        {
            _rb.isKinematic = true; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
        }


        [MapsInheritingFrom( typeof( FreePhysicsObject ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FreePhysicsObject>()
            {
                ("mass", new Member<FreePhysicsObject, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FreePhysicsObject, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FreePhysicsObject, bool>( o => false, (o, value) => o._rb.isKinematic = false)), // TODO - isKinematic member is a hack.

                ("velocity", new Member<FreePhysicsObject, Vector3>( o => o.Velocity )),
                ("angular_velocity", new Member<FreePhysicsObject, Vector3>( o => o.AngularVelocity ))
            };
        }
    }
}