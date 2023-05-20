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
        // this class is basically either a celestial body of some kind, or a vessel. Something that moves on its own and is not parented to anything else.

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

        public Vector3 Acceleration { get; private set; }
        Vector3 _oldVelocity;

        Rigidbody _rb;
        RootObjectTransform _rootTransform;

        /// <summary>
        /// Adds a force acting on the center of mass of the physics object. Does not apply any torque.
        /// </summary>
        public void AddForce( Vector3 force )
        {
            this._rb.AddForce( force, ForceMode.Force );
        }

        /// <summary>
        /// Adds a force at a specified position instead of at the center of mass.
        /// </summary>
        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            this._rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        public Vector3 ClosestPointOnBounds( Vector3 worldSpacePosition )
        {
            return this._rb.ClosestPointOnBounds( worldSpacePosition );
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
            // I'm not a fan of the physics being calculated in scene-space, but that's the only way to handle collisions properly.
            this._rootTransform.SetAIRFPosition( SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position ) );

#warning TODO - Big values of velocity lack precision which in turn causes fluids to flow in freefall, because the vessel's acceleration is wrong.
            this.Acceleration = ((Velocity / Time.fixedDeltaTime) - (_oldVelocity / Time.fixedDeltaTime));
            this._oldVelocity = Velocity;
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