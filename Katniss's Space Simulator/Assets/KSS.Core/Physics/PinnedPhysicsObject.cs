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
	/// A physobj that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
	/// </remarks>
	[RequireComponent( typeof( RootObjectTransform ) )]
	[RequireComponent( typeof( Rigidbody ) )]
	[DisallowMultipleComponent]
	public class PinnedPhysicsObject : MonoBehaviour, IPhysicsObject, IPersistent
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

		Matrix3x3 _inertiaTensor;
		public Matrix3x3 MomentOfInertiaTensor
		{
			get => _inertiaTensor;
			set
			{
				_inertiaTensor = value;

				(Vector3 eigenvector, float eigenvalue)[] eigen = value.Diagonalize().OrderByDescending( m => m.eigenvalue ).ToArray();
				this._rb.inertiaTensor = new Vector3( eigen[0].eigenvalue, eigen[1].eigenvalue, eigen[2].eigenvalue );
				this._rb.inertiaTensorRotation = value.rotation;

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

		public SerializedData GetData( IReverseReferenceMap s )
		{
			return new SerializedObject()
			{
				{ "mass", this.Mass },
				{ "local_center_of_mass", s.WriteVector3( this.LocalCenterOfMass ) },
				{ "velocity", s.WriteVector3( this.Velocity ) },
				{ "angular_velocity", s.WriteVector3( this.AngularVelocity ) },
				{ "reference_body", s.WriteObjectReference( this.ReferenceBody ) },
				{ "reference_position", s.WriteVector3Dbl( this.ReferencePosition ) },
				{ "reference_rotation", s.WriteQuaternionDbl( this.ReferenceRotation ) }
			};
		}

		public void SetData( IForwardReferenceMap l, SerializedData data )
		{
			if( data.TryGetValue( "mass", out var mass ) )
				this.Mass = (float)mass;

			if( data.TryGetValue( "local_center_of_mass", out var localCenterOfMass ) )
				this.LocalCenterOfMass = l.ReadVector3( localCenterOfMass );

			if( data.TryGetValue( "velocity", out var velocity ) )
				this.Velocity = l.ReadVector3( velocity );

			if( data.TryGetValue( "angular_velocity", out var angularVelocity ) )
				this.AngularVelocity = l.ReadVector3( angularVelocity );

			// this is the culprit
			if( data.TryGetValue( "reference_body", out var referenceBody ) )
				this.ReferenceBody = (CelestialBody)l.ReadObjectReference( referenceBody );

			if( data.TryGetValue( "reference_position", out var referencePosition ) )
				this.ReferencePosition = l.ReadVector3Dbl( referencePosition );

			if( data.TryGetValue( "reference_rotation", out var referenceRotation ) )
				this.ReferenceRotation = l.ReadQuaternionDbl( referenceRotation );
		}
	}
}