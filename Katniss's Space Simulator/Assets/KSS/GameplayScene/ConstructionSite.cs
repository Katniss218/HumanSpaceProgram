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
    public enum ConstructionState : sbyte
    {
        NotStarted = 0,
        Constructing = 1,
        PausedConstructing = 2,
        Deconstructing = -1,
        PausedDeconstructing = -2,
        Waiting = 8
    }

    public static class ConstructionState_Ex
    {
        public static bool IsConstruction( this ConstructionState state )
        {
            return (int)state > 0;
        }

        public static bool IsDeconstruction( this ConstructionState state )
        {
            return (int)state < 0;
        }
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

        public enum ConstructibleState : sbyte
        {
            FinishedDeconstruction = -1,
            InProgress = 0,
            FinishedConstruction = 1
        }

        /// <summary>
        /// Tracks progress of a single constructible.
        /// </summary>
        public class ConstructibleData
        {
            public GhostPatchSet patchSet;
            public float accumulatedBuildPoints;
            public ConstructibleState state = ConstructibleState.InProgress;
        }

        public ConstructionState State { get; private set; } = ConstructionState.Waiting;

        /// <summary>
        /// Cumulative total build speed per second. This is divided by the number of child objects currently under construction.
        /// </summary>
        [field: SerializeField]
        public float BuildSpeedTotal { get; set; }

        /// <summary>
        /// The total number of parts (constructibles) belonging to this construction site.
        /// </summary>
        public int TotalPartCount { get => _constructionData.Count; }

        /// <summary>
        /// The sum of parts (constructibles) constructed (when constructing) or parts deconstructed (when deconstructing).
        /// </summary>
        public int CompletedPartCount { get; private set; } = 0;

        BidirectionalReferenceStore _referenceMap = new BidirectionalReferenceStore();

        Dictionary<FConstructible, ConstructibleData> _constructionData = new Dictionary<FConstructible, ConstructibleData>();

        /// <remarks>
        /// Can be called while deconstructing to cancel, and start constructing instead.
        /// </remarks>
        public void StartConstruction()
        {
            if( State == ConstructionState.Constructing )
                return;

            this.CompletedPartCount = _constructionData.Values
                .Where( v => v.state == ConstructibleState.FinishedConstruction )
                .Count();
            this.State = ConstructionState.Constructing;
        }

        /// <remarks>
        /// Can be called while constructing to cancel, and start deconstructing instead.
        /// </remarks>
        public void StartDeconstruction()
        {
            if( State == ConstructionState.Deconstructing )
                return;

            this.CompletedPartCount = _constructionData.Values
                .Where( v => v.state == ConstructibleState.FinishedDeconstruction )
                .Count();
            this.State = ConstructionState.Deconstructing;
        }

        public void Pause()
        {
            if( this.State == ConstructionState.Constructing )
            {
                this.State = ConstructionState.PausedConstructing;
                return;
            }
            if( this.State == ConstructionState.Deconstructing )
            {
                this.State = ConstructionState.PausedDeconstructing;
                return;
            }
        }

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.G ) )
            {
                StartConstruction();
            }

            float buildSpeedPerPart = BuildSpeedTotal / (TotalPartCount - CompletedPartCount);
            if( State == ConstructionState.Constructing )
            {
                if( CompletedPartCount == TotalPartCount )
                {
                    ForceFinish();
                    return;
                }
                foreach( (var constructible, var data) in _constructionData )
                {
                    if( data.state != ConstructibleState.FinishedConstruction )
                    {
                        data.accumulatedBuildPoints += buildSpeedPerPart * TimeManager.DeltaTime;
                        if( data.accumulatedBuildPoints > constructible.MaxBuildPoints )
                        {
                            data.accumulatedBuildPoints = constructible.MaxBuildPoints;
                            data.patchSet.GhostToOriginalPatch.Run( _referenceMap );
                            data.state = ConstructibleState.FinishedConstruction;
                            CompletedPartCount++;
                        }
                    }
                }
            }
            if( State == ConstructionState.Deconstructing )
            {
                if( CompletedPartCount == TotalPartCount )
                {
                    ForceFinish();
                    return;
                }
                foreach( (_, var data) in _constructionData )
                {
                    if( data.state != ConstructibleState.FinishedDeconstruction )
                    {
                        data.accumulatedBuildPoints -= buildSpeedPerPart * TimeManager.DeltaTime;
                        if( data.accumulatedBuildPoints < 0.0f )
                        {
                            data.accumulatedBuildPoints = 0.0f;
                            data.patchSet.OriginalToGhostPatch.Run( _referenceMap );
                            data.state = ConstructibleState.FinishedDeconstruction;
                            CompletedPartCount++;
                        }
                    }
                }
            }
        }

        public void ForceFinish()
        {
            if( this.State.IsConstruction() )
            {
                // Stopping construction means that the things that are not fully built should be removed, and progress wasted.
                foreach( var kvp in _constructionData )
                {
                    if( kvp.Value.state != ConstructibleState.FinishedConstruction )
                    {
                        Destroy( kvp.Key.gameObject );
                    }
                }
            }
            else if( this.State.IsDeconstruction() )
            {
                // Stopping deconstruction means that the things that were not fully deconstructed should remain (not entirely realistic, but hey gameplay).
                foreach( var kvp in _constructionData )
                {
                    if( kvp.Value.state == ConstructibleState.FinishedDeconstruction )
                    {
                        Destroy( kvp.Key.gameObject );
                    }
                    // 2023/12/02 - Originals are only turned into ghosts only after fully deconstructed, so we can leave them.
                }
            }

            Destroy( this );
            return;
        }

        public static (Transform root, Dictionary<FConstructible, ConstructibleData>, BidirectionalReferenceStore) SpawnGhost( string vesselId )
        {
            // step 1. player clicks, and spawns ghost to place.

            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();
            GameObject rootGo = PartRegistry.Load( new Core.Mods.NamespacedIdentifier( "Vessels", vesselId ), refStore );

            Dictionary<FConstructible, List<Transform>> partMap = MapToAncestralComponent<FConstructible>( rootGo.transform );

            Dictionary<FConstructible, ConstructibleData> ghostParts = new Dictionary<FConstructible, ConstructibleData>();
            foreach( var con in partMap.Keys )
            {
                GhostPatchSet gpart = GhostPatchSet.MakeGhostPatch( con, partMap, refStore );
                gpart.OriginalToGhostPatch.Run( refStore );
                ghostParts.Add( con, new ConstructibleData() { patchSet = gpart, accumulatedBuildPoints = 0.0f } );
            }

            return (rootGo.transform, ghostParts, refStore);
        }

        public static ConstructionSite AddGhostToConstruction( Transform ghostRoot, Dictionary<FConstructible, ConstructibleData> ghostParts, Transform parent, BidirectionalReferenceStore refMap )
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

            cSite._referenceMap.AddAll( refMap.GetAll() );

            ghostRoot.transform.SetParent( parent );
            return cSite;
        }

        public void PickUpGhostFromConstruction( Transform ghostRoot )
        {
            throw new NotImplementedException();

            // reverse of step 6. Player picks up the ghost.

            if( this.State != ConstructionState.NotStarted )
            {
                // can't pickup once construction is started.
                return;
            }

            Dictionary<FConstructible, List<Transform>> partMap = MapToAncestralComponent<FConstructible>( ghostRoot );

            // Check if the hierarchy up from the specified ghost root is fully ghosted. Otherwise the ghost can't be picked up.
            int containedCount = partMap.Count( kvp => _constructionData.ContainsKey( kvp.Key ) );
            if( containedCount != partMap.Count )
            {
                return;
            }

            foreach( var constructible in partMap.Keys )
            {
                _constructionData.Remove( constructible );
            }
            if( _constructionData.Count == 0 )
            {
                Destroy( this );
            }
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