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

    public static class FConstructionSite_Transform_Ex
    {
        /// <summary>
        /// Gets the <see cref="FConstructionSite"/> that is constructing this transform.
        /// </summary>
        /// <returns>The construction site. Null if the transform is not under construction/deconstruction.</returns>
        public static FConstructionSite GetConstructionSite( this Transform part )
        {
            FConstructionSite site = part.GetComponent<FConstructionSite>();
            while( site == null )
            {
                part = part.parent;
                if( part == null )
                    break;
                site = part.GetComponent<FConstructionSite>();
            }
            return site;
        }

        /// <summary>
        /// Gets the <see cref="FConstructionSite"/> that is constructing this part.
        /// </summary>
        /// <returns>The construction site. Null if the transform is not under construction/deconstruction.</returns>
        public static FConstructionSite GetConstructionSite( this FConstructible part )
        {
            return GetConstructionSite( part.transform );
        }

        /// <summary>
        /// Checks whether a given transform belongs to a construction site.
        /// </summary>
        public static bool IsUnderConstruction( this Transform part )
        {
            if( part == null )
                return false;

            return part.GetConstructionSite() != null;
        }

        /// <summary>
        /// Checks whether a given part belongs to a construction site.
        /// </summary>
        public static bool IsUnderConstruction( this FConstructible part )
        {
            return IsUnderConstruction( part.transform );
        }

        /// <summary>
        /// Checks whether a given transform belongs to a construction site, and that the construction has started.
        /// </summary>
        public static bool IsUnderActiveConstruction( this Transform part )
        {
            if( part == null )
                return false;

            FConstructionSite site = part.GetConstructionSite();
            if( site == null )
                return false;

            return site.State != ConstructionState.NotStarted;
        }

        /// <summary>
        /// Checks whether a given part belongs to a construction site, and that the construction has started.
        /// </summary>
        public static bool IsUnderActiveConstruction( this FConstructible part )
        {
            return IsUnderActiveConstruction( part.transform );
        }
    }

    [RequireComponent( typeof( RootObjectTransform ) )]
    [DisallowMultipleComponent]
    public class FConstructionSite : MonoBehaviour
    {
        public ConstructionState State { get; private set; } = ConstructionState.Waiting;

        /// <summary>
        /// Cumulative total build speed per second. This is divided by the number of child objects currently under construction.
        /// </summary>
        [field: SerializeField]
        public float BuildSpeedTotal { get; set; }

        /// <summary>
        /// The total number of parts (constructibles) belonging to this construction site.
        /// </summary>
        public int TotalPartCount { get => _constructibles.Count; }

        /// <summary>
        /// The sum of parts (constructibles) constructed (when constructing) or parts deconstructed (when deconstructing).
        /// </summary>
        public int CompletedPartCount { get; private set; } = 0;

        BidirectionalReferenceStore _referenceMap = new BidirectionalReferenceStore();

        List<FConstructible> _constructibles = new List<FConstructible>();

        /// <remarks>
        /// Can be called while deconstructing to cancel, and start constructing instead.
        /// </remarks>
        public void StartConstruction()
        {
            if( State == ConstructionState.Constructing )
                throw new InvalidOperationException( $"Can't start construction when already constructing." );

            this.CompletedPartCount = _constructibles
                .Where( v => v.CurrentState == FConstructible.State.FinishedConstruction )
                .Count();
            this.State = ConstructionState.Constructing;
        }

        /// <remarks>
        /// Can be called while constructing to cancel, and start deconstructing instead.
        /// </remarks>
        public void StartDeconstruction()
        {
            if( State == ConstructionState.Deconstructing )
                throw new InvalidOperationException( $"Can't start deconstruction when already deconstructing." );

            this.CompletedPartCount = _constructibles
                .Where( v => v.CurrentState == FConstructible.State.FinishedDeconstruction )
                .Count();
            this.State = ConstructionState.Deconstructing;
        }

        /// <summary>
        /// Pauses the construction/deconstruction.
        /// </summary>
        public void Pause()
        {
            this.State = this.State switch
            {
                ConstructionState.Constructing => ConstructionState.PausedConstructing,
                ConstructionState.Deconstructing => ConstructionState.PausedDeconstructing,
                _ => throw new InvalidOperationException( $"Can't pause if there is no ongoing construction/deconstruction." ),
            };
        }

        void Update()
        {
            if( UnityEngine.Input.GetKeyDown( KeyCode.G ) )
            {
                this.BuildSpeedTotal = 90f;
                StartConstruction();
            }

            if( UnityEngine.Input.GetKeyDown( KeyCode.H ) )
            {
                this.BuildSpeedTotal = 90f;
                StartDeconstruction();
            }

            if( UnityEngine.Input.GetKeyDown( KeyCode.J ) )
            {
                this.BuildSpeedTotal = 90f;
                Pause();
            }

            float buildSpeedPerPart = BuildSpeedTotal / (TotalPartCount - CompletedPartCount);

            if( CompletedPartCount == TotalPartCount )
            {
                ForceFinish();
                return;
            }

            if( State == ConstructionState.Constructing )
            {
                var inProgressConstructibles = _constructibles.Where( c => c.CurrentState != FConstructible.State.FinishedConstruction );
                foreach( var constructible in inProgressConstructibles )
                {
                    constructible.ChangeBuildPoints( buildSpeedPerPart * TimeManager.DeltaTime );
                    if( constructible.CurrentState == FConstructible.State.FinishedConstruction )
                    {
                        CompletedPartCount++;
                    }
                }
            }
            if( State == ConstructionState.Deconstructing )
            {
                var inProgressConstructibles = _constructibles.Where( c => c.CurrentState != FConstructible.State.FinishedDeconstruction );
                foreach( var constructible in inProgressConstructibles )
                {
                    constructible.ChangeBuildPoints( -buildSpeedPerPart * TimeManager.DeltaTime );
                    if( constructible.CurrentState == FConstructible.State.FinishedDeconstruction )
                    {
                        CompletedPartCount++;
                    }
                }
            }
        }

        public static FConstructionSite TryAddPart( Transform ghostRoot, Transform parent, (FConstructible k, BidirectionalGhostPatch v)[] ghostParts, BidirectionalReferenceStore refMap )
        {
            // step 6. Player places the ghost.
            // assume the position is already set.

            if( ghostRoot.IsUnderActiveConstruction() )
            {
                throw new InvalidOperationException( $"can't add while being constructed." );
            }

            if( parent == null )
            {
                Vessel vessel = VesselFactory.CreatePartless(
                    SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( ghostRoot.position ),
                    SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( ghostRoot.rotation ),
                    Vector3.zero,
                    Vector3.zero );

                vessel.RootPart = ghostRoot;
                parent = vessel.gameObject.transform;
            }
            else
            {
                VesselHierarchyUtils.AttachLoose( ghostRoot, parent );
            }

            FConstructionSite constructionSite = ghostRoot.GetConstructionSite();
            if( constructionSite == null )
            {
                constructionSite = ghostRoot.gameObject.AddComponent<FConstructionSite>();
            }

            foreach( var ghostPart in ghostParts )
            {
                constructionSite._constructibles.Add( ghostPart.k );
            }

            constructionSite._referenceMap.AddAll( refMap.GetAll() );

            ghostRoot.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
            ghostRoot.transform.SetParent( parent );
            return constructionSite;
        }

        /// <summary>
        /// Tries to remove the specified part of the construction site from construction.
        /// </summary>
        /// <returns>True if the specified part was successfully unhooked.</returns>
        public static bool TryRemovePart( Transform ghostRoot )
        {

        }

        /// <summary>
        /// Spawns a ghosted vessel/part.
        /// </summary>
        public static (Transform root, (FConstructible k, BidirectionalGhostPatch v)[], BidirectionalReferenceStore) SpawnGhost( string vesselId )
        {
            // step 1. player clicks, and spawns ghost to place.

            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();
            GameObject rootGo = PartRegistry.Load( new Core.Mods.NamespacedIdentifier( "Vessels", vesselId ), refStore );

            BidirectionalReferenceStore remappedRefStore = refStore.RemapRandomly(); // remapping allows multiple instances of the same objects (the same IDs) to be loaded at any given time.

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( rootGo.transform );

            (FConstructible, BidirectionalGhostPatch)[] ghostParts = new (FConstructible, BidirectionalGhostPatch)[partMap.KeyCount];
            int i = 0;
            foreach( var kvp in partMap )
            {
                BidirectionalGhostPatch patch = BidirectionalGhostPatch.CreateGhostPatch( kvp.Value, remappedRefStore );
                patch.Forward.Run( remappedRefStore );
                ghostParts[i] = (kvp.Key, patch);
                i++;
            }

            return (rootGo.transform, ghostParts, remappedRefStore);
        }
    }
}