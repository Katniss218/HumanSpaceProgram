using KSS.Components;
using KSS.Core;
using KSS.Core.Components;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

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


        deconstruction:

        click to deconstruct
        deconstruction starts progressing, making parts ghostly.
        on finish, ghost parts are destroyed.

        */

        public class DataEntry
        {
            public GhostedPart gPart;
            public float buildPoints;
            public sbyte state = 0; // -1 = finished deconstructing, 0 = in-progress or not started yet, 1 = finished constructing.
        }

        [field: SerializeField]
        public ConstructionMode CurrentMode { get; private set; } = ConstructionMode.Paused;

        BidirectionalReferenceStore _refMap;

        Dictionary<FConstructible, DataEntry> _constructionData = new Dictionary<FConstructible, DataEntry>();

        /// <summary>
        /// Cumulative total build speed per second. This is divided by the number of child objects currently under construction.
        /// </summary>
        [field: SerializeField]
        public float BuildSpeedTotal { get; set; }

        public int TotalCount { get => _constructionData.Count; }

        public int CompletedCount { get; private set; } = 0;

        void Update()
        {
            float buildSpeedPerPart = BuildSpeedTotal / (TotalCount - CompletedCount);
            if( CurrentMode == ConstructionMode.Construction )
            {
                if( CompletedCount == TotalCount )
                {
                    Destroy( this );
                    return;
                }
                foreach( var kvp in _constructionData )
                {
                    var dataEntry = kvp.Value;
                    if( dataEntry.state != 1 )
                    {
                        dataEntry.buildPoints += buildSpeedPerPart * TimeManager.DeltaTime;
                        if( dataEntry.buildPoints > kvp.Key.MaxBuildPoints )
                        {
                            dataEntry.buildPoints = kvp.Key.MaxBuildPoints;
                            dataEntry.gPart.GhostToOriginalPatch.Run( _refMap );
                            dataEntry.state = 1;
                            CompletedCount++;
                        }
                    }
                }
            }
            if( CurrentMode == ConstructionMode.Deconstruction )
            {
                if( CompletedCount == TotalCount )
                {
                    foreach( var kvp in _constructionData )
                    {
                        Destroy( kvp.Key.gameObject );
                    }
                    Destroy( this );
                    return;
                }
                foreach( var kvp in _constructionData )
                {
                    var dataEntry = kvp.Value;
                    if( dataEntry.state != -1 )
                    {
                        dataEntry.buildPoints -= buildSpeedPerPart * TimeManager.DeltaTime;
                        if( dataEntry.buildPoints < 0.0f )
                        {
                            dataEntry.buildPoints = 0.0f;
                            dataEntry.gPart.OriginalToGhostPatch.Run( _refMap );
                            dataEntry.state = -1;
                            CompletedCount++;
                        }
                    }
                }
            }
        }

        public static (Transform root, Dictionary<FConstructible, DataEntry>, BidirectionalReferenceStore) SpawnGhost( string vesselId )
        {
            // step 1. player clicks, and spawns ghost to place.

            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();
            GameObject rootGo = PartRegistry.Load( new Core.Mods.NamespacedIdentifier( "Vessels", vesselId ), refStore );

            Dictionary<FConstructible, List<Transform>> partMap = MapToAncestralComponent<FConstructible>( rootGo.transform );
            FConstructible[] constructibles = rootGo.GetComponentsInChildren<FConstructible>();
            Dictionary<FConstructible, DataEntry> ghostParts = new Dictionary<FConstructible, DataEntry>();
            foreach( var con in constructibles )
            {
                GhostedPart gpart = GhostedPart.MakeGhostPatch( con, partMap, refStore );
                gpart.OriginalToGhostPatch.Run( refStore );
                ghostParts.Add( con, new DataEntry() { gPart = gpart, buildPoints = 0.0f } );
            }

            return (rootGo.transform, ghostParts, refStore);
        }

        public static ConstructionSite PlaceGhost( Transform ghostRoot, Dictionary<FConstructible, DataEntry> ghostParts, Transform parent, BidirectionalReferenceStore refMap )
        {
            // step 6. Player places the ghost.

            // assume the position is already set.

            if( parent == null )
            {
                Vessel v = VesselFactory.CreatePartless(
                    SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( ghostRoot.position ),
                    SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( ghostRoot.rotation ),
                    Vector3.zero,
                    Vector3.zero );

                v.RootPart = ghostRoot;
                parent = v.gameObject.transform;
            }
            else
            {
                VesselHierarchyUtils.AttachLoose( ghostRoot, parent );
            }

            ConstructionSite cSite = parent.GetComponentInParent<ConstructionSite>();
            if( cSite == null )
            {
                cSite = parent.gameObject.AddComponent<ConstructionSite>();
            }
            foreach( var kvp in ghostParts )
            {
                cSite._constructionData.Add( kvp.Key, kvp.Value );
            }

            cSite._refMap = cSite._refMap == null
                ? refMap
                : BidirectionalReferenceStore.Combine( cSite._refMap, refMap );

            ghostRoot.transform.SetParent( parent );
            return cSite;
        }

        public void PickupGhost( Transform ghostRoot )
        {
            // reverse of step 6. Player picks up the ghost.

            // if parent's ancestral chain has a c-site - add to that c-site, otherwise - make new c-site.
        }

        /// <summary>
        /// This returns a map that maps each T component in the tree, starting at root, to the descendants that belong to it. <br />
        /// Each descendant belongs to its closest ancestor that has the T component. <br />
        /// Descendants that have the T component are mapped to their own component.
        /// </summary>
        public static Dictionary<T, List<Transform>> MapToAncestralComponent<T>( Transform root ) where T : Component
        {
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