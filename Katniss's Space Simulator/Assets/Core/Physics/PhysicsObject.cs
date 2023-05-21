using KatnisssSpaceSimulator.Core.Managers;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Physics
{
    /// <summary>
    /// Any object that calculates its own physics.
    /// </summary>
    /// <remarks>
    /// This is a wrapper for some kind of internal physics solver and collision resolver.
    /// </remarks>
    [RequireComponent( typeof( Rigidbody ) )]
    [RequireComponent( typeof( RootObjectTransform ) )] // IMPORTANT: Changing the order here changes the order in which Awake() fires (setting the position of objects in the first frame depends on the fact that RB is added before root transform).
    public class PhysicsObject : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the physics object's mass in [kg].
        /// </summary>
        public float Mass
        {
            get => this._rb.mass;
            set => this._rb.mass = value;
        }

        /// <summary>
        /// Gets or sets the physics object's local center of mass (relative to the physics object).
        /// </summary>
        public Vector3 LocalCenterOfMass
        {
            get => this._rb.centerOfMass;
            set => this._rb.centerOfMass = value;
        }

        /// <summary>
        /// Gets or sets the physics object's velocity in scene space in [m/s].
        /// </summary>
        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set => this._rb.velocity = value;
        }

        /// <summary>
        /// Gets or sets the physics object's velocity in scene space in [m/s].
        /// </summary>
        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set => this._rb.angularVelocity = value;
        }

        /// <summary>
        /// Gets the acceleration that this physics object is under at this instant.
        /// </summary>
        public Vector3 Acceleration { get; private set; }

        /// <summary>
        /// Gets the angular acceleration that this physics object is under at this instant.
        /// </summary>
        public Vector3 AngularAcceleration { get; private set; }

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3 _accelerationSum = Vector3.zero;
        Vector3 _angularAccelerationSum = Vector3.zero;

        Rigidbody _rb;
        RootObjectTransform _rootTransform;

        /// <summary>
        /// Adds a force acting on the center of mass of the physics object. Does not apply any torque.
        /// </summary>
        public void AddForce( Vector3 force )
        {
            _accelerationSum += force / Mass;
            this._rb.AddForce( force, ForceMode.Force );
        }

        /// <summary>
        /// Adds a force at a specified position instead of at the center of mass.
        /// </summary>
        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            // I think force / mass is still correct for this,
            // - because 2 consecutive impulses in the same direction, but opposite positions (so that the angular accelerations cancel out)
            // - should still produce linear acceleration of 2 * force / mass.
            _accelerationSum += force / Mass;

            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _angularAccelerationSum += Vector3.Cross( force, leverArm ) / Mass;

            this._rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        void Awake()
        {
            _rb = this.GetComponent<Rigidbody>();
            _rootTransform = this.GetComponent<RootObjectTransform>();

            _rb.useGravity = false;
            _rb.mass = 5;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.Extrapolate;
        }

        void FixedUpdate()
        {
            // I'm not a huge fan of the physics being calculated in scene-space, but that's the only way to handle collisions properly.
            this._rootTransform.SetAIRFPosition( SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position ) );

            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.

            if( IsColliding )
            {
                this.Acceleration = (Velocity - _oldVelocity) / Time.fixedDeltaTime;
                this.AngularAcceleration = (AngularVelocity - _oldAngularVelocity) / Time.fixedDeltaTime;
            }
            else
            {
                // accSum will be whatever that was accumulated over the time from the previous frame (when it was zeroed out) to this frame.
                // I think it should work fine.
                this.Acceleration = _accelerationSum;
                this.AngularAcceleration = _angularAccelerationSum;
            }

            this._oldVelocity = Velocity;
            this._oldAngularVelocity = AngularVelocity;
            this._accelerationSum = Vector3.zero;
            this._angularAccelerationSum = Vector3.zero;
        }

        /// <summary>
        /// Gets a value indicating whether or not the physics object or any of its children colliders is currently colliding with something.
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
            _rb.isKinematic = false;
        }

        void OnDisable()
        {
            _rb.isKinematic = true;
        }
    }
}