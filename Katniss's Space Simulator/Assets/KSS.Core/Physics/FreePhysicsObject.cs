using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core.Physics
{
    /// <remarks>
    /// This is a wrapper for a rigidbody.
    /// </remarks>
    [RequireComponent( typeof( RootObjectTransform ) )]
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FreePhysicsObject : MonoBehaviour, IPhysicsObject, IPersistsData
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

        RootObjectTransform _rootObjTransform;
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
            _rootObjTransform = this.gameObject.GetOrAddComponent<RootObjectTransform>();
            _rootObjTransform.RefreshCachedRigidbody();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = false;
        }

        void FixedUpdate()
        {
            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.

            if( IsColliding )
            {
                this.Acceleration = (Velocity - _oldVelocity) / TimeStepManager.FixedDeltaTime;
                this.AngularAcceleration = (AngularVelocity - _oldAngularVelocity) / TimeStepManager.FixedDeltaTime;
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

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "mass", this.Mass.AsSerialized() },
                { "local_center_of_mass", this.LocalCenterOfMass.GetData() },
                { "velocity", this.Velocity.GetData() },
                { "angular_velocity", this.AngularVelocity.GetData() }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            _rb.isKinematic = false; // FreePhysicsObject is never kinematic. This is needed because it may be called first.

            if( data.TryGetValue( "mass", out var mass ) )
                this.Mass = mass.AsFloat();

            if( data.TryGetValue( "local_center_of_mass", out var localCenterOfMass ) )
                this.LocalCenterOfMass = localCenterOfMass.AsVector3();

            if( data.TryGetValue( "velocity", out var velocity ) )
                this.Velocity = velocity.AsVector3();

            if( data.TryGetValue( "angular_velocity", out var angularVelocity ) )
                this.AngularVelocity = angularVelocity.AsVector3();
        }
    }
}