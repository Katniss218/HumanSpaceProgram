using HSP.Core;
using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vessels;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Construction
{
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
        /// Gets the <see cref="FConstructionSite"/> that is constructing this constructible.
        /// </summary>
        /// <returns>The construction site corresponding to this constructible. Null if the transform is not under construction/deconstruction.</returns>
        public static FConstructionSite GetConstructionSite( this FConstructible part )
        {
            return GetConstructionSite( part.transform );
        }

        /// <summary>
        /// Checks whether a given transform is a descendant of a construction site.
        /// </summary>
        public static bool IsUnderConstruction( this Transform part )
        {
            if( part == null )
                return false;

            return part.GetConstructionSite() != null;
        }

        /// <summary>
        /// Checks whether a given constructible belongs to a construction site.
        /// </summary>
        public static bool IsUnderConstruction( this FConstructible part )
        {
            return IsUnderConstruction( part.transform );
        }

        /// <summary>
        /// Checks whether a given transform belongs to a construction site, and that the construction/deconstruction has started.
        /// </summary>
        public static bool IsUnderOngoingConstruction( this Transform part )
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
        public static bool IsUnderOngoingConstruction( this FConstructible part )
        {
            return IsUnderOngoingConstruction( part.transform );
        }
    }

    /// <summary>
    /// Manages the construction of its descendant <see cref="FConstructible"/>s.
    /// </summary>
    [DisallowMultipleComponent]
    public class FConstructionSite : MonoBehaviour
    {
        /// <summary>
        /// The current state of (de)construction at this construction site.
        /// </summary>
        public ConstructionState State { get; private set; } = ConstructionState.NotStarted;

        [SerializeField] float _buildSpeed;
        /// <summary>
        /// Cumulative total build speed in [build points per second]. <br/>
        /// This is then divided by the number of in-progress constructibles to obtain the delta for each constructible.
        /// </summary>
        public float BuildSpeed
        {
            get => _buildSpeed;
            set
            {
                if( value < 0 )
                    throw new ArgumentOutOfRangeException( $"Build speed can't be negative." );
                _buildSpeed = value;
            }
        }

        FConstructible[] _constructibles = new FConstructible[] { };

        /// <summary>
        /// Calculates the sum of current build points and max build points of all constructibles of this construction site.
        /// </summary>
        /// <returns>The calculated sum.</returns>
        public (float current, float total) GetBuildPoints()
        {
            return (_constructibles.Sum( c => c.BuildPoints ), _constructibles.Sum( c => c.MaxBuildPoints ));
        }

        /// <summary>
        /// Gets the number of <see cref="FConstructible"/>s that are currently being built (build speed != 0).
        /// </summary>
        public int GetCountOfProgressing()
        {
            return _constructibles.Select( c => c.GetBuildSpeedMultiplier() == 0.0f ? 0 : 1 ).Sum();
        }

        /// <summary>
        /// Gets the number of <see cref="FConstructible"/>s that are currently not being built (build speed = 0).
        /// </summary>
        public int GetCountOfNotProgressing()
        {
            return _constructibles.Select( c => c.GetBuildSpeedMultiplier() != 0.0f ? 0 : 1 ).Sum();
        }

        /// <summary>
        /// Starts the process of construction.
        /// </summary>
        /// <remarks>
        /// If called while deconstructing, it will start constructing again from the current point.
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
        /// If called while constructing, it will start deconstructing from the current point.
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

        void OnEnable()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_ConstructionSite.GAMEPLAY_AFTER_CONSTRUCTION_SITE_CREATED, this );
        }

        void OnDisable()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_ConstructionSite.GAMEPLAY_AFTER_CONSTRUCTION_SITE_DESTROYED, this );
        }

        void Update()
        {
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
                    if( this.State == ConstructionState.Deconstructing )
                    {
                        foreach( var constructible in _constructibles.ToArray() )
                        {
                            Destroy( constructible.gameObject );
                        }
                    }

                    var vessel = this.transform.GetVessel();

                    if( this.State == ConstructionState.Deconstructing )
                    {
                        this.transform.SetParent( null ); // This stops the part object from including the deconstructed children when recalculating.
                    }

                    Destroy( this );

                    if( vessel != null )
                    {
                        vessel.RecalculatePartCache();
                    }

                    return;
                }

                foreach( var constructible in inProgressConstructibles )
                {
                    buildPointsDelta *= constructible.GetBuildSpeedMultiplier();
                    constructible.BuildPoints += Mathf.Clamp( buildPointsDelta, -constructible.BuildPoints, constructible.MaxBuildPoints - constructible.BuildPoints );
                }
            }
        }

        /// <summary>
        /// Creates a new construction site with the specified vessel, or appends it to the specified parent
        /// </summary>
        /// <param name="ghostRoot"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static FConstructionSite CreateOrAppend( Transform ghostRoot, Transform parent )
        {
            // step 6. Player places the ghost.
            // assume the position is already set.

            if( ghostRoot.IsUnderOngoingConstruction() )
            {
                throw new InvalidOperationException( $"Can't add something that is under ongoing construction - it should be/have been already added." );
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
            constructionSite._constructibles = AncestralMap<FConstructible>.Create( constructionSite.transform ).Keys.ToArray();

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

        [MapsInheritingFrom( typeof( FConstructionSite ) )]
        public static SerializationMapping FConstructionSiteMapping()
        {
            return new MemberwiseSerializationMapping<FConstructionSite>()
            {
                ("state", new Member<FConstructionSite, ConstructionState>( o => o.State )),

                ("constructibles", new Member<FConstructionSite, object>( o => null, (o, value) => o._constructibles = AncestralMap<FConstructible>.Create( o.transform ).Keys.ToArray() )),

                ("build_speed", new Member<FConstructionSite, float>( o => o.BuildSpeed ))
            };
        }
    }
}