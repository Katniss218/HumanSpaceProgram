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
    public class PhysicsObject : MonoBehaviour, IReferenceFrameSwitchResponder
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

        Rigidbody _rb;

        public Vector3Dbl AIRFPosition { get; private set; }

        /// <summary>
        /// Sets the position of the vessel in Absolute Inertial Reference Frame coordinates. Units in [m].
        /// </summary>
        public void SetPosition( Vector3Dbl airfPosition )
        {
            this.AIRFPosition = airfPosition;

            Vector3 scenePos = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( airfPosition );
            this._rb.position = scenePos; // this is important.
            this.transform.position = scenePos;
        }

        /// <summary>
        /// Sets the rotation of the vessel in Absolute Inertial Reference Frame coordinates.
        /// </summary>
        public void SetRotation( Quaternion airfRotation )
        {
            Quaternion sceneRotation = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformRotation( airfRotation );
            this._rb.rotation = sceneRotation; // this is important.
            this.transform.rotation = sceneRotation;
        }

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

        /// <summary>
        /// Callback to the event.
        /// </summary>
        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // Kinda ugly tbh. Maybe just subscribe to it, and use the interface as a marker to prevent auto-update position?
            this.transform.position = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( this.AIRFPosition );
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
#warning TODO - this get called before OnSceneReferenceFrameSwitch is called, and thus sets the AIRF Position to an incorrect value (this.transform.position is already moved?).
            this.AIRFPosition = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position );
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