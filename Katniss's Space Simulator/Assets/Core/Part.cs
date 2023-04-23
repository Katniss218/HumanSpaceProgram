using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public sealed class Part : MonoBehaviour
    {
        [field: SerializeField]
        public Vessel Vessel { get; private set; }

        /// <summary>
        /// Sets the vessel of this part and any of its children to the specified value.
        /// </summary>
        /// <remarks>
        /// DO NOT USE. This is intended for internal use and can create an invalid state. Use <see cref="VesselStateUtils.SetParent(Part, Part)"/> instead.
        /// </remarks>
        internal void SetVesselRecursive( Vessel vessel )
        {
            Vessel = vessel;
            foreach( var chp in this.Children ) // kinda ugly, but we need to make sure the children are always part of the same vessel.
            {
                chp.SetVesselRecursive( vessel );
            }
        }

        [SerializeField]
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; this.gameObject.name = value; }
        }

        public float Mass { get; set; }

        public bool IsRootOfVessel { get => this.Vessel.RootPart == this; }

        [field: SerializeField]
        public Part Parent { get; internal set; }

        [field: SerializeField]
        public List<Part> Children { get; private set; } = new List<Part>();

        [field: SerializeField]
        public List<Functionality> Modules { get; private set; } = new List<Functionality>();

        public void RegisterModule( Functionality module )
        {
            module.Part = this;
            this.Modules.Add( module );
        }

        public void SetLocalPosition( Vector3 pos, bool moveChildren = true )
        {
            Vector3 delta = pos - this.transform.localPosition;

            this.transform.localPosition = pos;

            if( moveChildren )
            {
                foreach( var cp in this.Children )
                {
                    cp.SetLocalPosition( cp.transform.position + delta, moveChildren );
                }
            }
        }

        public void SetLocalRotation( Quaternion rot, bool moveChildren = true )
        {
            Quaternion delta = Quaternion.Inverse( this.transform.localRotation ) * rot; // I hope I did the math right.

            this.transform.localRotation = rot;

            if( moveChildren )
            {
                foreach( var cp in this.Children )
                {
                    cp.SetLocalRotation( cp.transform.localRotation * delta, moveChildren );
                }
            }
        }



        // -=-=-=-=-=-=-=-=-=-


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere( this.transform.position, 0.1f );
            if( this.Vessel == null )
            {
                Debug.LogWarning( $"Invalid State: Part '{this}' is orphaned and doesn't have a vessel." );
                Gizmos.color = Color.red;
                Gizmos.DrawSphere( this.transform.position, 0.25f );
                return;
            }

            if( this.Parent == null )
            {
                if( this.IsRootOfVessel )
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere( this.transform.position, 0.2f );
                }
                else
                {
                    Debug.LogWarning( $"Invalid State: Part '{this}' doesn't have a parent, and is not the root of a vessel." );
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere( this.transform.position, 0.1f );
                }
            }
            else
            {
                if( this.IsRootOfVessel )
                {
                    Debug.LogWarning( $"Invalid State: Part '{this}' has a parent, and is the root of a vessel." );
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere( this.transform.position, 0.1f );
                }

                Vector3 lineEnd = this.transform.position - ((this.transform.position - this.Parent.transform.position).normalized * 0.3f);
                Gizmos.DrawLine( this.Parent.transform.position, lineEnd );

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere( lineEnd, 0.05f );
            }
        }
    }
}