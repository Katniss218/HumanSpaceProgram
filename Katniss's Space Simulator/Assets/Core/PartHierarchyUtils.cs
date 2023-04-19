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
            List<Part> parts = new List<Part>();

            foreach( Transform child in vessel.transform )
            {
                Part part = child.GetComponent<Part>();
                if( part == null )
                {
                    throw new InvalidOperationException( $"Non-part object on vessel '{vessel}'." );
                }

                parts.Add( part );
            }

            return parts;
        }

        public static Vessel GetVessel( this Part part )
        {
            Transform parent = part.transform.parent;

            if( parent == null )
            {
                throw new InvalidOperationException( $"The part '{part}' was missing its vessel." );
            }

            Vessel vessel = parent.GetComponent<Vessel>();
            if( vessel == null )
            {
                throw new InvalidOperationException( $"The part '{part}' was parented to something other than a vessel." );
            }

            return vessel;
        }

        public static void SetVessel( this Part part, Vessel newVessel )
        {
            part.transform.SetParent( newVessel.transform );
            foreach( var cp in part.Children )
            {
                cp.SetVessel( newVessel );
            }
        }
    }
}