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
    /// <summary>
    /// Any object that interacts with the collision/physics system.
    /// </summary>
    /// <remarks>
    /// This is a wrapper for a rigidbody.
    /// </remarks>
    [RequireComponent( typeof( Rigidbody ) )]
    public class PhysicsObject : MonoBehaviour, IPersistent
    {
        /// <summary>
        /// Gets or sets the physics object's mass, in [kg].
        /// </summary>
        public float Mass
        {
            get => this._rb.mass;
            set => this._rb.mass = value;
        }

        /// <summary>
        /// Gets or sets the physics object's local center of mass (in physics object's coordinate space).
        /// </summary>
        public Vector3 LocalCenterOfMass
        {
            get => this._rb.centerOfMass;
            set => this._rb.centerOfMass = value;
        }

        /// <summary>
        /// Gets or sets the physics object's velocity in scene space, in [m/s].
        /// </summary>
        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set => this._rb.velocity = value;
        }

        /// <summary>
        /// Gets or sets the physics object's angular velocity in scene space, in [Rad/s].
        /// </summary>
        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set => this._rb.angularVelocity = value;
        }

        bool _isKinematic;
        public bool IsKinematic
        {
            get => _isKinematic;
            set
            {
                _isKinematic = value;
                this._rb.isKinematic = value;
            }
        }

        /// <summary>
        /// Gets the acceleration that this physics object is under at this instant, in [m/s^2].
        /// </summary>
        public Vector3 Acceleration { get; private set; }

        /// <summary>
        /// Gets the angular acceleration that this physics object is under at this instant, in [Rad/s^2].
        /// </summary>
        public Vector3 AngularAcceleration { get; private set; }

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3 _accelerationSum = Vector3.zero;
        Vector3 _angularAccelerationSum = Vector3.zero;

        Rigidbody _rb;

        /// <summary>
        /// Applies a force at the center of mass, in [N].
        /// </summary>
        public void AddForce( Vector3 force )
        {
            _accelerationSum += force / Mass;
            this._rb.AddForce( force, ForceMode.Force );
        }

        /// <summary>
        /// Applies a force at the specified position, in [N].
        /// </summary>
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

        void Awake()
        {
            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.mass = 5;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.Extrapolate;
        }

        void FixedUpdate()
        {
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

        /// <summary>
        /// True if the physics object is colliding with any other objects in the current frame, false otherwise.
        /// </summary>
        [field: SerializeField]
        public bool IsColliding { get; private set; }

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
            _rb.isKinematic = _isKinematic; // Rigidbody doesn't have `enabled`, so we set it to kinematic.
        }

        void OnDisable()
        {
            _rb.isKinematic = true; // Rigidbody doesn't have `enabled`, so we set it to kinematic.
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "mass", this.Mass },
                { "local_center_of_mass", s.WriteVector3( this.LocalCenterOfMass ) },
                { "velocity", s.WriteVector3( this.Velocity ) },
                { "angular_velocity", s.WriteVector3( this.AngularVelocity ) },
                { "is_kinematic", this.IsKinematic }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "mass", out var mass ) )
                this.Mass = (float)mass;

            if( data.TryGetValue( "local_center_of_mass", out var localCenterOfMass ) )
                this.LocalCenterOfMass = l.ReadVector3( localCenterOfMass );

            if( data.TryGetValue( "is_kinematic", out var isKinematic ) )
                this.IsKinematic = (bool)isKinematic;

            if( !this.IsKinematic )
            {
                if( data.TryGetValue( "velocity", out var velocity ) )
                    this.Velocity = l.ReadVector3( velocity );

                if( data.TryGetValue( "angular_velocity", out var angularVelocity ) )
                    this.AngularVelocity = l.ReadVector3( angularVelocity );
            }
        }
    }
}