using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    [RequireComponent( typeof( PhysicsObject ) )]
    public class Vessel : MonoBehaviour
    {
        [SerializeField]
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; this.gameObject.name = value; }
        }

        public PhysicsObject PhysicsObject { get; private set; }

        [field: SerializeField]
        public Part RootPart { get; private set; }

        [field: SerializeField]
        public IEnumerable<Part> Parts { get; private set; }

        [field: SerializeField]
        public int PartCount { get; private set; }

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
        /// Returns the local space center of mass.
        /// </summary>
        public Vector3 GetLocalCenterOfMass()
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
            return com;
        }

        void Awake()
        {
            this.PhysicsObject = this.GetComponent<PhysicsObject>();
        }

        void Start()
        {
            this.PhysicsObject.LocalCenterOfMass = this.GetLocalCenterOfMass();
        }

        void FixedUpdate()
        {
            this.PhysicsObject.LocalCenterOfMass = this.GetLocalCenterOfMass();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube( this.transform.TransformPoint( this.PhysicsObject.LocalCenterOfMass), Vector3.one * 0.25f );
        }
    }
}