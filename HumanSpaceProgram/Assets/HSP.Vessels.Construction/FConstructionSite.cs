using HSP.ReferenceFrames;
using HSP.SceneManagement;
using HSP.Time;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vessels.Construction
{
    public static class HSPEvent_AFTER_CONSTRUCTION_SITE_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".construction_site_created";
    }

    public static class HSPEvent_AFTER_CONSTRUCTION_SITE_DESTROYED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".construction_site_destroyed";
    }

    public static class FConstructionSite_Transform_Ex
    {
        /// <summary>
        /// Gets the <see cref="FConstructionSite"/> of the specified vessel.
        /// </summary>
        public static FConstructionSite GetConstructionSite( this Vessel vessel )
        {
            if( vessel == null ) return null;
            return vessel.GetComponent<FConstructionSite>();
        }

        /// <summary>
        /// Gets the <see cref="FConstructionSite"/> for the vessel containing this part.
        /// </summary>
        public static FConstructionSite GetConstructionSite( this VesselPart part )
        {
            if( part == null ) return null;
            Vessel vessel = part.transform.GetVessel();
            return vessel != null ? vessel.GetComponent<FConstructionSite>() : null;
        }

        /// <summary>
        /// Gets the <see cref="FConstructionSite"/> for the vessel containing this transform.
        /// </summary>
        public static FConstructionSite GetConstructionSite( this Transform partTransform )
        {
            if( partTransform == null ) return null;
            Vessel vessel = partTransform.GetComponentInParent<Vessel>();
            return vessel != null ? vessel.GetComponent<FConstructionSite>() : null;
        }

        /// <summary>
        /// Gets the <see cref="FConstructionSite"/> that is constructing this constructible.
        /// </summary>
        public static FConstructionSite GetConstructionSite( this FConstructible constructible )
        {
            if( constructible == null ) return null;
            return GetConstructionSite( constructible.transform );
        }

        /// <summary>
        /// Checks whether a given transform belongs to a construction site's under-construction parts.
        /// </summary>
        public static bool IsUnderConstruction( this Transform part )
        {
            if( part == null ) return false;
            VesselPart vesselPart = part.GetComponentInParent<VesselPart>();
            if( vesselPart != null )
                return vesselPart.IsUnderConstruction();

            FConstructionSite site = part.GetConstructionSite();
            return site != null && site.ContainsTransform( part );
        }

        /// <summary>
        /// Checks whether a given part belongs to a construction site's under-construction parts.
        /// </summary>
        public static bool IsUnderConstruction( this VesselPart part )
        {
            if( part == null ) return false;
            FConstructionSite site = part.GetConstructionSite();
            return site != null && site.ContainsPart( part );
        }

        /// <summary>
        /// Checks whether a given constructible belongs to a construction site's under-construction parts.
        /// </summary>
        public static bool IsUnderConstruction( this FConstructible part )
        {
            return part != null && part.transform.IsUnderConstruction();
        }

        /// <summary>
        /// Checks whether a given transform belongs to a construction site, and that the construction/deconstruction has started.
        /// </summary>
        public static bool IsUnderOngoingConstruction( this Transform part )
        {
            FConstructionSite site = part.GetConstructionSite();
            return site != null && site.State != ConstructionState.NotStarted;
        }

        /// <summary>
        /// Checks whether a given constructible belongs to a construction site, and that the construction/deconstruction has started.
        /// </summary>
        public static bool IsUnderOngoingConstruction( this FConstructible part )
        {
            return part.transform.IsUnderOngoingConstruction();
        }

        /// <summary>
        /// Checks whether a given part belongs to a construction site, and that the construction/deconstruction has started.
        /// </summary>
        public static bool IsUnderOngoingConstruction( this VesselPart part )
        {
            return part.transform.IsUnderOngoingConstruction();
        }
    }

    /// <summary>
    /// Manages the construction of a set of <see cref="VesselPart"/>s and their <see cref="FConstructible"/>s.
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

        [SerializeField] List<VesselPart> _parts = new List<VesselPart>();
        [SerializeField] List<FConstructible> _constructibles = new List<FConstructible>();

        public IReadOnlyList<VesselPart> Parts => _parts;
        public IReadOnlyList<FConstructible> Constructibles => _constructibles;

        public bool ContainsPart( VesselPart part )
        {
            return part != null && _parts.Contains( part );
        }

        public bool ContainsConstructible( FConstructible constructible )
        {
            return constructible != null && _constructibles.Contains( constructible );
        }

        public bool ContainsTransform( Transform t )
        {
            if( t == null ) return false;
            foreach( var part in _parts )
            {
                if( part != null && (part.transform == t || t.IsChildOf( part.transform )) )
                    return true;
            }
            return false;
        }

        public void AddPart( VesselPart part )
        {
            if( part == null || _parts.Contains( part ) )
                return;

            _parts.Add( part );

            var constructibles = part.GetComponentsInChildren<FConstructible>( true );
            foreach( var c in constructibles )
            {
                if( !_constructibles.Contains( c ) )
                {
                    _constructibles.Add( c );
                }
            }
        }

        public void AddParts( IEnumerable<VesselPart> parts )
        {
            if( parts == null ) return;
            foreach( var p in parts )
            {
                AddPart( p );
            }
        }

        public bool RemovePart( VesselPart part )
        {
            if( part == null || !_parts.Contains( part ) )
                return false;

            _parts.Remove( part );

            var constructibles = part.GetComponentsInChildren<FConstructible>( true );
            foreach( var c in constructibles )
            {
                _constructibles.Remove( c );
            }

            if( _parts.Count == 0 )
            {
                State = ConstructionState.NotStarted;
            }

            return true;
        }

        public void RemoveParts( IEnumerable<VesselPart> parts )
        {
            if( parts == null ) return;
            foreach( var p in parts.ToList() )
            {
                RemovePart( p );
            }
        }

        public void SetUnderConstruction( VesselPart part, bool underConstruction = true )
        {
            if( underConstruction )
                AddPart( part );
            else
                RemovePart( part );
        }

        public void SetUnderConstruction( IEnumerable<VesselPart> parts, bool underConstruction = true )
        {
            if( underConstruction )
                AddParts( parts );
            else
                RemoveParts( parts );
        }

        public void MergeWith( FConstructionSite otherSite )
        {
            if( otherSite == null || otherSite == this )
                return;

            AddParts( otherSite._parts );
            if( otherSite.State != ConstructionState.NotStarted && this.State == ConstructionState.NotStarted )
            {
                this.State = otherSite.State;
            }

            otherSite._parts.Clear();
            otherSite._constructibles.Clear();
            otherSite.State = ConstructionState.NotStarted;
        }

        public void InitializeWith( IEnumerable<VesselPart> parts, float buildSpeed, ConstructionState state )
        {
            _parts.Clear();
            _constructibles.Clear();
            this.BuildSpeed = buildSpeed;
            this.State = state;
            AddParts( parts );
        }

        public void CopyFrom( FConstructionSite source )
        {
            if( source == null ) return;
            InitializeWith( source.Parts, source.BuildSpeed, source.State );
        }

        /// <summary>
        /// Calculates the sum of current build points and max build points of all constructibles of this construction site.
        /// </summary>
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
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_CONSTRUCTION_SITE_CREATED.ID, this );
        }

        void OnDisable()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_CONSTRUCTION_SITE_DESTROYED.ID, this );
        }

        void Update()
        {
            FConstructible[] inProgressConstructibles = null;
            float buildPointsDelta = 0.0f;

            if( State == ConstructionState.Constructing )
            {
                inProgressConstructibles = _constructibles.Where( c => c.BuildPercent < 1.0f ).ToArray();
                if( inProgressConstructibles.Length > 0 )
                {
                    buildPointsDelta = (BuildSpeed / inProgressConstructibles.Length) * TimeManager.DeltaTime;
                }
            }
            else if( State == ConstructionState.Deconstructing )
            {
                inProgressConstructibles = _constructibles.Where( c => c.BuildPercent > 0.0f ).ToArray();
                if( inProgressConstructibles.Length > 0 )
                {
                    buildPointsDelta = (-BuildSpeed / inProgressConstructibles.Length) * TimeManager.DeltaTime;
                }
            }

            if( inProgressConstructibles != null )
            {
                if( inProgressConstructibles.Length == 0 )
                {
                    var vessel = this.GetComponentInParent<Vessel>();

                    if( this.State == ConstructionState.Deconstructing )
                    {
                        foreach( var part in _parts.ToArray() )
                        {
                            if( part != null ) Destroy( part.gameObject );
                        }
                    }

                    _parts.Clear();
                    _constructibles.Clear();
                    this.State = ConstructionState.NotStarted;

                    if( vessel != null )
                    {
                        vessel.RecalculatePartCache();
                    }

                    return;
                }

                foreach( var constructible in inProgressConstructibles )
                {
                    float delta = buildPointsDelta * constructible.GetBuildSpeedMultiplier();
                    constructible.BuildPoints += Mathf.Clamp( delta, -constructible.BuildPoints, constructible.MaxBuildPoints - constructible.BuildPoints );
                }
            }
        }

        /// <summary>
        /// Tries to remove the specified part from construction.
        /// </summary>
        public static bool TryRemovePart( Transform ghostRoot )
        {
            if( ghostRoot == null ) return false;
            VesselPart part = ghostRoot.GetComponentInParent<VesselPart>() ?? ghostRoot.GetComponent<VesselPart>();
            if( part == null ) return false;

            FConstructionSite site = part.GetConstructionSite();
            if( site != null )
            {
                return site.RemovePart( part );
            }
            return false;
        }

        [MapsInheritingFrom( typeof( FConstructionSite ) )]
        public static IDescriptor FConstructionSiteMapping()
        {
            return new MemberwiseDescriptor<FConstructionSite>()
                .WithMember( "state", o => o.State )
                .WithMember( "parts", o => o._parts )
                .WithMember( "constructibles", o => o._constructibles )
                .WithMember( "build_speed", o => o.BuildSpeed );
        }
    }
}