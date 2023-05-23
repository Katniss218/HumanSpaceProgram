using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.Core
{
    public sealed partial class Part : MonoBehaviour, IMassCallback
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

        /// <summary>
        /// The mass of the part in [kg].
        /// </summary>
        [field: SerializeField]
        public float Mass { get; set; } = 1000f;

        /// <summary>
        /// Checks whether or not this part is a root of its <see cref="Vessel"/>.
        /// </summary>
        public bool IsRootOfVessel { get => this.Vessel.RootPart == this; }

        [field: SerializeField]
        public Part Parent { get; internal set; }

        [field: SerializeField]
        public List<Part> Children { get; private set; } = new List<Part>();

        /// <summary>
        /// Sets the local position of the part relative to its <see cref="Vessel"/>.
        /// </summary>
        public void SetLocalPosition( Vector3 pos, bool moveChildren = true )
        {
            Vector3 delta = pos - this.transform.localPosition;

            this.transform.localPosition = pos;

            if( moveChildren )
            {
                foreach( var cp in this.Children )
                {
                    // Potentially replace with iterative later, if it proves too slow.
                    cp.SetLocalPosition( cp.transform.position + delta, moveChildren );
                }
            }
        }

        /// <summary>
        /// Sets the local rotation of the part relative to its <see cref="Vessel"/>.
        /// </summary>
        public void SetLocalRotation( Quaternion rot, bool moveChildren = true )
        {
            Quaternion delta = Quaternion.Inverse( this.transform.localRotation ) * rot; // I hope I did the math right.

            this.transform.localRotation = rot;

            if( moveChildren )
            {
                foreach( var cp in this.Children )
                {
                    // Potentially replace with iterative later, if it proves too slow.
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
                // TODO - this also gets called if the part is a prefab and being set up.
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