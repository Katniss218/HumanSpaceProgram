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

    public static class ConstructionSite_Transform_Ex
    {
        /// <summary>
        /// Gets the <see cref="ConstructionSite"/> that is constructing this transform.
        /// </summary>
        /// <returns>The construction site. Null if the transform is not under construction/deconstruction.</returns>
        public static ConstructionSite GetConstructionSite( this Transform part )
        {
            ConstructionSite site = part.GetComponent<ConstructionSite>();
            while( site == null )
            {
                part = part.parent;
                if( part == null )
                    break;
                site = part.GetComponent<ConstructionSite>();
            }
            return site;
        }

        /// <summary>
        /// Gets the <see cref="ConstructionSite"/> that is constructing this part.
        /// </summary>
        /// <returns>The construction site. Null if the transform is not under construction/deconstruction.</returns>
        public static ConstructionSite GetConstructionSite( this FConstructible part )
        {
            return GetConstructionSite( part.transform );
        }

        /// <summary>
        /// Checks whether a given transform belongs to a construction site.
        /// </summary>
        public static bool IsUnderConstruction( this Transform part )
        {
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
            ConstructionSite site = part.GetConstructionSite();
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
    public class ConstructionSite : MonoBehaviour
    {
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
            public BidirectionalGhostPatch patchSet;
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
                throw new InvalidOperationException( $"Can't start construction when already constructing." );

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
                throw new InvalidOperationException( $"Can't start deconstruction when already deconstructing." );

            this.CompletedPartCount = _constructionData.Values
                .Where( v => v.state == ConstructibleState.FinishedDeconstruction )
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
                foreach( (var constructible, var data) in _constructionData )
                {
                    if( data.state != ConstructibleState.FinishedConstruction )
                    {
                        data.accumulatedBuildPoints += buildSpeedPerPart * TimeManager.DeltaTime;
                        if( data.accumulatedBuildPoints >= constructible.MaxBuildPoints )
                        {
                            data.accumulatedBuildPoints = constructible.MaxBuildPoints;
                            data.patchSet.Reverse.Run( _referenceMap );
                            data.state = ConstructibleState.FinishedConstruction;
                            CompletedPartCount++;
                        }
                    }
                }
            }
            if( State == ConstructionState.Deconstructing )
            {
                foreach( (_, var data) in _constructionData )
                {
                    if( data.state != ConstructibleState.FinishedDeconstruction )
                    {
                        data.accumulatedBuildPoints -= buildSpeedPerPart * TimeManager.DeltaTime;
                        if( data.accumulatedBuildPoints <= 0.0f )
                        {
                            data.accumulatedBuildPoints = 0.0f;
                            data.patchSet.Forward.Run( _referenceMap );
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
                foreach( var kvp in _constructionData )
                {
                    if( kvp.Value.state != ConstructibleState.FinishedConstruction )
                    {
                        kvp.Key.transform.SetParent( null ); // needed because getcomponentsinchildren checks destroyed children (facepalm unity).
                        Destroy( kvp.Key.gameObject );
                    }
                }
            }
            else if( this.State.IsDeconstruction() )
            {
                foreach( var kvp in _constructionData )
                {
                    if( kvp.Value.state == ConstructibleState.FinishedDeconstruction )
                    {
                        kvp.Key.transform.SetParent( null ); // needed because getcomponentsinchildren checks destroyed children (facepalm unity).
                        Destroy( kvp.Key.gameObject );
                    }
                }
            }

            this.transform.GetVessel().RecalculateParts();
            Destroy( this );
        }

        public static ConstructionSite TryAddPart( Transform ghostRoot, Transform parent, (FConstructible k, BidirectionalGhostPatch v)[] ghostParts, BidirectionalReferenceStore refMap )
        {
            // step 6. Player places the ghost.
            // assume the position is already set.

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

#warning  TODO - there should be only one construction site in the parent of the constructed objects. If you add a part to construct

            //
            //    0
            //   /|\
            //  1 2 3
            //
            // if you add something with `1` as parent, and then add `2` itself, then the only c-site should appear at `0`.
            // c-sites under construction don't split as the parents are constructed though.

            // another consideration - whether or not the player should be able to add parts to sites already constructing, or only to sites that are not started.


            ConstructionSite constructionSite = parent.GetComponentInParent<ConstructionSite>();
            if( constructionSite == null )
            {
                constructionSite = parent.gameObject.AddComponent<ConstructionSite>();
            }
            foreach( var ghostPart in ghostParts )
            {
#warning TODO - key may be null
                constructionSite._constructionData.Add( ghostPart.k, new ConstructibleData() { patchSet = ghostPart.v, accumulatedBuildPoints = 0.0f } );
            }

            constructionSite._referenceMap.AddAll( refMap.GetAll() );

            ghostRoot.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
            ghostRoot.transform.SetParent( parent );
            return constructionSite;
        }

        public bool TryRemovePart( Transform ghostRoot, out Dictionary<FConstructible, BidirectionalGhostPatch> ghostParts, out BidirectionalReferenceStore refMap )
        {
            // reverse of step 6. Player picks up the ghost.

            if( this.State != ConstructionState.NotStarted )
            {
                // can't pickup once construction is started.
                ghostParts = null;
                refMap = null;
                return false;
            }

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( ghostRoot );

            // Check if the hierarchy up from the specified ghost root is fully ghosted. Otherwise the ghost can't be picked up.
           /* int containedCount = partMap.Count( kvp => _constructionData.ContainsKey( kvp.Key ) );
            if( containedCount != partMap.Count )
            {
                ghostParts = null;
                refMap = null;
                return false;
            }

            ghostParts = new Dictionary<FConstructible, BidirectionalGhostPatch>();
            foreach( var constructible in partMap.Keys )
            {
                if( _constructionData.TryGetValue( constructible, out var data ) )
                {
                    ghostParts.Add( constructible, data.patchSet );
                    _constructionData.Remove( constructible );
                }
            }
            if( _constructionData.Count == 0 )
            {
                Destroy( this );
            }*/
            ghostParts = null;
            refMap = null;
            return false;
        }

        /// <summary>
        /// Spawns a ghosted vessel/part.
        /// </summary>
        public static (Transform root, (FConstructible k, BidirectionalGhostPatch v)[], BidirectionalReferenceStore) SpawnGhost( string vesselId )
        {
            // step 1. player clicks, and spawns ghost to place.

            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();
            GameObject rootGo = PartRegistry.Load( new Core.Mods.NamespacedIdentifier( "Vessels", vesselId ), refStore );

            BidirectionalReferenceStore remappedRefStore = refStore.RemapRandomly(); // remapping allows object with the same guids (the same object) to be loaded again.

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( rootGo.transform );

            (FConstructible, BidirectionalGhostPatch)[] ghostParts = new (FConstructible, BidirectionalGhostPatch)[partMap.KeyCount];
            int i = 0;
            foreach( var kvp in partMap.AsEnumerable() )
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