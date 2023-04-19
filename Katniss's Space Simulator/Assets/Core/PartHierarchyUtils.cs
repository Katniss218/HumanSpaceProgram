using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// Helper class responsible for validating and managing the Unity hierarchy of parts.
    /// </summary>
    public static class PartHierarchyUtils
    {
        /// <summary>
        /// Validates that the hierarchy is actually valid, and returns all of the parts by searching the hierarchy.
        /// </summary>
        public static List<Part> GetParts( this Vessel vessel )
        {
            // Parts are a hierarchy of elements.
            List<Part> parts = new List<Part>();

            Part rootPart = GetRootPart( vessel );
            parts.Add( rootPart );
            parts.AddRange( rootPart.GetDescendants() );

            return parts;
        }

        /// <summary>
        /// Validates that the hierarchy is actually valid (only at the root part depth), and returns the root part by searching the hierarchy.
        /// </summary>
        public static Part GetRootPart( this Vessel vessel )
        {
            // A Vessel should have a tree of its parts as its only children.

            if( vessel.transform.childCount != 1 )
            {
                throw new InvalidOperationException( $"The vessel '{vessel}' somehow had more or less than exactly 1 root part." );
            }

            Part rootPart = vessel.transform.GetChild( 0 ).GetComponent<Part>();

            if( rootPart == null )
            {
                throw new InvalidOperationException( $"The vessel '{vessel}' was missing its root part." );
            }

            return rootPart;
        }

        /// <summary>
        /// Validates that the hierarchy is actually valid, and returns the parent by searching the hierarchy.
        /// </summary>
        public static Part GetParent( this Part part )
        {
            if( part.transform.parent == null )
            {
                throw new InvalidOperationException( $"The part '{part}' was not a child of a vessel or a part." );
            }
            Part parentPart = part.transform.parent.GetComponent<Part>();

            if( parentPart == null && part.transform.parent.GetComponent<Vessel>() == null )
            {
                throw new InvalidOperationException( $"The part '{part}' was not a child of a vessel or a part." );
            }

            return parentPart;
        }

        public static List<Part> GetChildren( this Part part )
        {
            List<Part> parts = new List<Part>( part.transform.childCount );

            foreach( Transform child in part.transform )
            {
                Part cp = child.GetComponent<Part>();
                if( cp != null ) // parts can have non-part children.
                {
                    parts.Add( cp );
                }
            }
            return parts;
        }

        /// <summary>
        /// Validates that the descendants (children, children of children, and so on) of the part are actually parts, and returns them by searching the hierarchy.
        /// </summary>
        public static List<Part> GetDescendants( this Part part )
        {
            // This should return the descendants of the part flattened into a single list.

            List<Part> parts = new List<Part>();

            Queue<Part> nodeQueue = new Queue<Part>();
            nodeQueue.Enqueue( part );

            while( nodeQueue.Count > 0 )
            {
                Part cp = nodeQueue.Dequeue();
                foreach( var child in cp.GetChildren() )
                {
                    parts.Add( child );
                    nodeQueue.Enqueue( child );
                }
            }

            return parts;
        }

        public static Vessel GetVessel( this Part part )
        {
            Transform parent = part.transform.parent;

            while( parent.parent != null )
            {
                Vessel vessel = parent.GetComponent<Vessel>();
                if( vessel != null )
                {
                    return vessel;
                }

                parent = parent.parent;
            }

            throw new InvalidOperationException( $"The part '{part}' was missing its vessel." );
        }

        public static void SetParent( this Part part, Part parent )
        {
            part.transform.SetParent( parent?.transform.parent );

            part.RecalculateCachedHierarchy();
            if( parent != null )
            {
                parent.RecalculateCachedHierarchy();
            }
        }
    }
}