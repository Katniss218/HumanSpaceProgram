using KatnisssSpaceSimulator.Core.Managers;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// Any object that calculates its own physics. This is a wrapper for some kind of internal physics solver / rigidbody.
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    public class PhysicsObject : MonoBehaviour
    {
        // this class is basically either a celestial body of some kind, or a vessel. Something that moves on its own and is not parented to anything else.

        Rigidbody rb;

        /// <summary>
        /// Use this to add a force acting on the center of mass. Does not apply any torque.
        /// </summary>
        public void AddForce( Vector3 force )
        {
            this.rb.AddForce( force, ForceMode.Force );
        }

        /// <summary>
        /// Use this to add a force at a specified position instead of at the center of mass.
        /// </summary>
        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            this.rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        public float Mass
        {
            get => this.rb.mass;
            set => this.rb.mass = value;
        }

        public Vector3 LocalCenterOfMass
        {
            get => this.rb.centerOfMass;
            set => this.rb.centerOfMass = value;
        }

        public Vector3 Velocity
        {
            get => this.rb.velocity;
            set => this.rb.velocity = value;
        }

        void Awake()
        {
            rb = this.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 5;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Extrapolate;
        }

        void FixedUpdate()
        {
            Vector3 gravityDir = Vector3.down;

            CelestialBody cb = CelestialBodyManager.Bodies[0];

            Vector3Large toBody = cb.GIRFPosition - ReferenceFrameManager.CurrentReferenceFrame.TransformPosition( this.transform.position );

            double distanceSq = toBody.sqMagnitude;

            const double G = 6.67430e-11;

            float forceMagn = (float)(G * ((rb.mass * cb.Mass) / distanceSq));

            // F = m*a
            AddForce( toBody.NormalizeToVector3() * forceMagn );


            // ---------------------

            // There's also multi-scene physics, which apparently might be used to put the origin of the simulation at 2 different vessels, and have their positions accuratly updated???
            // doesn't seem like that to me reading the docs tho, but idk.



        }
    }
}
