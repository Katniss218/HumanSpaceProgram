using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KSS.Core
{
    // Do not add RootObjectTransform explicitly *before* PhysicsObject. it messes up the initialization and rigidbody is not cached at the right time.
    [RequireComponent( typeof( PhysicsObject ) )]
    public sealed partial class Vessel : MonoBehaviour
    {
        // Root objects have to store their AIRF positions, children natively store their local coordinates, which as long as they're not obscenely large, will be fine.
        // - An object with a child at 0.00125f can be sent to 10e25 and brought back, and its child will remain at 0.00125f

        [SerializeField]
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; this.gameObject.name = value; }
        }

        [field: SerializeField]
        public Part RootPart { get; private set; }

        [field: SerializeField]
        public IEnumerable<Part> Parts { get; private set; }

        [field: SerializeField]
        public int PartCount { get; private set; }

        public PhysicsObject PhysicsObject { get; private set; }
        public RootObjectTransform RootObjTransform { get; private set; }

        public Vector3Dbl AIRFPosition { get => this.RootObjTransform.GetAIRFPosition(); }

        /// <remarks>
        /// DO NOT USE. This is for internal use, and can produce an invalid state. Use <see cref="VesselStateUtils.SetParent(Part, Part)"/> instead.
        /// </remarks>
        internal void SetRootPart( Part part )
        {
            if( part != null && part.Vessel != this )
            {
                throw new ArgumentException( $"Can't set the part '{part}' from vessel '{part.Vessel}' as root. The part is not a part of this vessel." );
            }

            RootPart = part;
        }

#warning TODO - vessels' position sometimes glitches out when far away from the origin/active vessel.

        public void RecalculateParts()
        {
            // take root and traverse its hierarchy to add up all parts.

            if( RootPart == null )
            {
                PartCount = 0;
                return;
            }

            int count = 0;
            Stack<Part> stack = new Stack<Part>();
            stack.Push( RootPart );
            List<Part> parts = new List<Part>();

            while( stack.Count > 0 )
            {
                Part p = stack.Pop();
                parts.Add( p );
                count++;

                foreach( Part cp in p.Children )
                {
                    stack.Push( cp );
                }
            }

            this.Parts = parts;
            PartCount = count;
        }

        /// <summary>
        /// Returns the local space center of mass, and the mass [kg] itself.
        /// </summary>
        public (Vector3 localCenterOfMass, float mass) CalculateMassInfo()
        {
            Vector3 com = Vector3.zero;
            float mass = 0;
            foreach( var part in this.Parts )
            {
                // physicsless parts may be `continue`'d here.

                com += part.transform.localPosition * part.Mass;
                mass += part.Mass;
            }
            if( mass > 0 )
            {
                com /= mass;
            }
            return (com, mass);
        }

        /// <summary>
        /// Sets the position of the vessel in Absolute Inertial Reference Frame coordinates. Units in [m].
        /// </summary>
        public void SetPosition( Vector3Dbl airfPosition )
        {
            this.RootObjTransform.SetAIRFPosition( airfPosition );
        }

        /// <summary>
        /// Sets the rotation of the vessel in Absolute Inertial Reference Frame coordinates.
        /// </summary>
        public void SetRotation( Quaternion airfRotation )
        {
            this.RootObjTransform.SetAIRFRotation( airfRotation );
        }

        /// <summary>
        /// Calculates the scene world-space point at the very bottom of the vessel. Useful when placing it at launchsites and such.
        /// </summary>
        public Vector3 GetBottomPosition()
        {
            Vector3 dir = this.transform.position - (this.transform.up * 500f);
            Vector3 min = this.transform.position;
            float minDist = float.MaxValue;
            Collider[] cols = this.GetComponentsInChildren<Collider>();
            foreach( var col in cols )
            {
                Vector3 closestBound = col.ClosestPointOnBounds( dir );
                float dst = Vector3.Distance( dir, closestBound );
                if( dst < minDist )
                {
                    minDist = dst;
                    min = closestBound;
                }
            }
            return min;

            //Vector3 closestBound = this.PhysicsObject.ClosestPointOnBounds( this.transform.position - (this.transform.up * 500f) );
            //return closestBound;
        }

        void Awake()
        {
            this.RootObjTransform = this.GetComponent<RootObjectTransform>();
            this.PhysicsObject = this.GetComponent<PhysicsObject>();
        }

        void SetPhysicsObjectParameters()
        {
            (Vector3 comLocal, float mass) = this.CalculateMassInfo();
            this.PhysicsObject.LocalCenterOfMass = comLocal;
            this.PhysicsObject.Mass = mass;
        }

        void Start()
        {
            SetPhysicsObjectParameters();
        }

        void FixedUpdate()
        {
            SetPhysicsObjectParameters();

            Vector3Dbl airfGravityForce = GravityUtils.GetGravityForce( PhysicsObject.Mass, this.AIRFPosition ); // Move airfposition to PhysicsObject maybe?

            PhysicsObject.AddForce( (Vector3)airfGravityForce );


            // ---------------------

            // There's also multi-scene physics, which apparently might be used to put the origin of the simulation at 2 different vessels, and have their positions accuratly updated???
            // doesn't seem like that to me reading the docs tho, but idk.
        }


        // -=-=-=-=-=-=-=-


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube( this.transform.TransformPoint( this.PhysicsObject.LocalCenterOfMass ), Vector3.one * 0.25f );
        }
    }
}