using HSP.Core.Components;
using HSP.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Core.Physics
{
	/// <remarks>
	/// A physobj that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
	/// </remarks>
	[RequireComponent( typeof( RootObjectTransform ) )]
	[RequireComponent( typeof( Rigidbody ) )]
	[DisallowMultipleComponent]
	public class PinnedPhysicsObject : MonoBehaviour, IPhysicsObject
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

		CelestialBody _referenceBody = null;
		public CelestialBody ReferenceBody
		{
			get => _referenceBody;
			set { _referenceBody = value; UpdateAIRFPositionFromReference(); }
		}

		Vector3Dbl _referencePosition = Vector3.zero;
		public Vector3Dbl ReferencePosition
		{
			get => _referencePosition;
			set { _referencePosition = value; UpdateAIRFPositionFromReference(); }
		}

		QuaternionDbl _referenceRotation = QuaternionDbl.identity;
		public QuaternionDbl ReferenceRotation
		{
			get => _referenceRotation;
			set { _referenceRotation = value; UpdateAIRFPositionFromReference(); }
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
			//this._rb.AddForce( force, ForceMode.Force );
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
			//this._rb.AddForceAtPosition( force, position, ForceMode.Force );
		}

		public void AddTorque( Vector3 force )
		{
			_angularAccelerationSum += force / Mass;
			//this._rb.AddTorque( force, ForceMode.Force );
		}

		internal void UpdateAIRFPositionFromReference()
		{
			if( ReferenceBody == null )
				return;

			var frame = ReferenceBody.OrientedReferenceFrame;
			this._rootObjTransform.AIRFPosition = frame.TransformPosition( ReferencePosition );
			this._rootObjTransform.AIRFRotation = frame.TransformRotation( ReferenceRotation );
		}

		void Awake()
		{
			if( this.HasComponentOtherThan<IPhysicsObject>( this ) )
			{
				Debug.LogWarning( $"Tried to add a {nameof( PinnedPhysicsObject )} to a game object that already has a {nameof( IPhysicsObject )}. This is not allowed. Remove the previous physics object first." );
				Destroy( this );
				return;
			}

			_rb = this.GetComponent<Rigidbody>();
			_rootObjTransform = this.gameObject.GetOrAddComponent<RootObjectTransform>();
			_rootObjTransform.RefreshCachedRigidbody();

			_rb.useGravity = false;
			_rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			_rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
			_rb.isKinematic = true;
		}

		void FixedUpdate()
		{
			// Reference can be moving, and we aren't parented (due to precision), thus we continuously update.
			this.UpdateAIRFPositionFromReference();

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

        [MapsInheritingFrom( typeof( PinnedPhysicsObject ) )]
        public static SerializationMapping PinnedPhysicsObjectMapping()
        {
			return new MemberwiseSerializationMapping<PinnedPhysicsObject>()
			{
				("mass", new Member<PinnedPhysicsObject, float>( o => o.Mass )),
				("local_center_of_mass", new Member<PinnedPhysicsObject, Vector3>( o => o.LocalCenterOfMass )),

				("DO_NOT_TOUCH", new Member<PinnedPhysicsObject, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("velocity", new Member<PinnedPhysicsObject, Vector3>( o => o.Velocity, (o, value) => { } )),
				("angular_velocity", new Member<PinnedPhysicsObject, Vector3>( o => o.AngularVelocity, (o, value) => { } )),
				("reference_body", new Member<PinnedPhysicsObject, CelestialBody>( ObjectContext.Ref, o => o.ReferenceBody )),
				("reference_position", new Member<PinnedPhysicsObject, Vector3Dbl>( o => o.ReferencePosition )),
				("reference_rotation", new Member<PinnedPhysicsObject, QuaternionDbl>( o => o.ReferenceRotation ))
			};
        }
	}
}