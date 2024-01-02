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
        /// Cumulative total build speed in [build points per second]. <br/>
        /// This is divided by the number of in-progress constructibles to obtain the delta.
        /// </summary>
        [field: SerializeField]
        public float BuildSpeed { get; set; }

        List<FConstructible> _constructibles = new List<FConstructible>();

        /// <summary>
        /// Starts the process of construction.
        /// </summary>
        /// <remarks>
        /// If called while deconstructing, it will start constructing from where it got to.
        /// </remarks>
        public void StartConstructing()
        {
            if( State == ConstructionState.Constructing )
                throw new InvalidOperationException( $"Can't start construction when already constructing." );

            this.State = ConstructionState.Constructing;
        }

        /// <summary>
        /// Starts the process of deconstruction.
        /// </summary>
        /// <remarks>
        /// If called while constructing, it will start deconstructing from where it got to.
        /// </remarks>
        public void StartDeconstructing()
        {
            if( State == ConstructionState.Deconstructing )
                throw new InvalidOperationException( $"Can't start deconstruction when already deconstructing." );

            this.State = ConstructionState.Deconstructing;
        }

        /// <summary>
        /// Pauses the process of construction/deconstruction.
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
        
        public void Unpause()
        {
            this.State = this.State switch
            {
                ConstructionState.PausedConstructing => ConstructionState.Constructing,
                ConstructionState.PausedDeconstructing => ConstructionState.Deconstructing,
                _ => throw new InvalidOperationException( $"Can't unpause if nothing is paused." ),
            };
        }

        void Update()
        {
            if( UnityEngine.Input.GetKeyDown( KeyCode.G ) )
            {
                this.BuildSpeed = 90f;
                StartConstructing();
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.H ) )
            {
                this.BuildSpeed = 90f;
                StartDeconstructing();
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.J ) )
            {
                this.BuildSpeed = 90f;
                Pause();
            }

            FConstructible[] inProgressConstructibles = null;
            float buildPointsDelta = 0.0f;

            if( State == ConstructionState.Constructing )
            {
                inProgressConstructibles = _constructibles.Where( c => c.BuildPercent < 1.0f ).ToArray();
                buildPointsDelta = (BuildSpeed / inProgressConstructibles.Length) * TimeManager.DeltaTime;
            }
            else if( State == ConstructionState.Deconstructing )
            {
                inProgressConstructibles = _constructibles.Where( c => c.BuildPercent > 0.0f ).ToArray();
                buildPointsDelta = (-BuildSpeed / inProgressConstructibles.Length) * TimeManager.DeltaTime;
            }

            if( inProgressConstructibles != null )
            {
                if( !inProgressConstructibles.Any() )
                {
                    Destroy( this );
                    return;
                }
                foreach( var constructible in inProgressConstructibles )
                {
                    constructible.BuildPoints += buildPointsDelta;
                }
            }
        }

        public static FConstructionSite TryAddPart( Transform ghostRoot, Transform parent )
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
            return false;
        }
    }
}