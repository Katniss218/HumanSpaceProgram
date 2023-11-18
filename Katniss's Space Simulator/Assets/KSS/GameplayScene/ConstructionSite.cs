using KSS.Core.Components;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.GameplayScene
{
    public enum ConstructionMode
    {
        Paused = 0,
        Construction,
        Deconstruction
    }

    [RequireComponent( typeof( RootObjectTransform ) )]
    [DisallowMultipleComponent]
    public class ConstructionSite : MonoBehaviour
    {
        /*
        
    1. The player clicks button, selects what to construct.
       - Original is spawned.
       - Patches that can transform the original into the ghost and vice versa are created.
       - The forward (into-ghost) patches are ran.
    6. The player places the ghost hierarchy.
       - The ghost hierarchy is attached and a construction site is created.
       - The parent part to where the construction is attached, and all its children become nonfunctional.
    8. The player adjusts the ghost's position/rotation.
       - Construction site updates the ghost's color to indicate if construction will be able to proceeed (nothing overlaps basically).
    9. The player accepts the position/rotation of the construction site. 
       - Construction starts progressing.
    10. As parts of the ghost are constructed, construction site runs the reverse (into-original) patches for the specified part.
    11. When construction finishes completely, the construction site is removed and everything becomes functional again.

        */

        public ConstructionMode CurrentMode { get; set; } = ConstructionMode.Paused;

        Patcher _patcher;

        List<GhostedPart> _ghostedParts;

        Dictionary<FPart, float> _progress;

        void Update()
        {
            if( CurrentMode == ConstructionMode.Construction )
            {
                
            }
            if( CurrentMode == ConstructionMode.Deconstruction )
            {
                
            }
        }

        void LateUpdate()
        {
            
        }

        public static Transform SpawnGhost( string vesselId )
        {
            throw new NotImplementedException();
            // step 1. player clicks, and spawns ghost to place.
        }

        public static ConstructionSite PlaceGhost( Transform ghostRoot, Transform parent )
        {
            throw new NotImplementedException();
            // step 6. Player places the ghost.

            ghostRoot.transform.SetParent( parent );

            // if parent's ancestral chain has a c-site - add to that c-site, otherwise - make new c-site.
        }

        public void PickupGhost( Transform ghostRoot )
        {
            // reverse of step 6. Player picks up the ghost.

            // if parent's ancestral chain has a c-site - add to that c-site, otherwise - make new c-site.
        }

        public static Dictionary<T, List<Transform>> MapToAncestralComponent<T>( Transform root ) where T : Component
        {
            // This returns a map that maps each T component in the tree, starting at root, to the descendants that belong to it.
            // Each descendant belongs to its closest ancestor that has the T component.
            // Descendants that have the T component are mapped to their own component.
#warning TODO - this map should probably be part of the vessel. It's useful.

            T rootsPart = root.GetComponent<T>();
            if( rootsPart == null )
            {
                throw new ArgumentException( $"Root must contain {typeof( T ).FullName}." );
            }

            Dictionary<T, List<Transform>> map = new Dictionary<T, List<Transform>>();
            Stack<(Transform parent, T parentPart)> stack = new Stack<(Transform, T)>();

            stack.Push( (root, rootsPart) ); // Initial entry with null parentPart

            while( stack.Count > 0 )
            {
                (Transform current, T parentPart) = stack.Pop();

                T currentPart = current.GetComponent<T>();
                if( currentPart == null )
                    currentPart = parentPart; // Inherit parent's part if the current doesn't have one

                if( map.TryGetValue( currentPart, out var list ) )
                {
                    list.Add( current );
                }
                else
                {
                    map.Add( currentPart, new List<Transform>() { current } );
                }

                foreach( Transform child in current )
                {
                    stack.Push( (child, currentPart) );
                }
            }

            return map;
        }
    }
}