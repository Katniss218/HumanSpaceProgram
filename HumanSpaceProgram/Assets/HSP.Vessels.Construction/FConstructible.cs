using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;
using UnityPlus.Serialization.ReferenceMaps;
using Ctx = UnityPlus.Serialization.Ctx;

namespace HSP.Vessels.Construction
{
    public interface IConstructionCondition
    {
        /// <summary>
        /// Checks what the build speed multiplier for a constructible at a given position would be.
        /// </summary>
        float GetBuildSpeedMultiplier( Vector3 targetScenePosition );
    }

    public struct CraneBuildCondition : IConstructionCondition
    {
        public float minLiftCapacity;

        public float GetBuildSpeedMultiplier( Vector3 targetScenePosition )
        {
            // get all cranes which range and position overlaps with the scene position.
            throw new NotImplementedException();
        }
    }

    public static class CraneBuildCondition_Mapping
    {
        [MapsInheritingFrom( typeof( CraneBuildCondition ) )]
        public static IDescriptor CraneBuildConditionMapping()
        {
            return new MemberwiseDescriptor<CraneBuildCondition>()
                .WithMember( "min_lift_capacity", o => o.minLiftCapacity );
        }
    }


    //
    //
    //


    /// <summary>
    /// Represents a "part" (hierarchy of gameobjects) that can be in various stages of construction.
    /// </summary>
    public class FConstructible : FComponent
    {
        [SerializeField]
        private float _buildPoints;
        /// <summary>
        /// The current total build points the part has accumulated.
        /// </summary>
        public float BuildPoints
        {
            get => _buildPoints;
            set
            {
                _buildPoints = value;
                OnAfterBuildPointsChanged();
            }
        }

        [SerializeField]
        private float _maxBuildPoints;
        /// <summary>
        /// The total number of build points required to complete the colstruction/deconstruction of this part.
        /// </summary>
        /// <remarks>
        /// One build point at 1x build speed takes 1 [s] to build.
        /// </remarks>
        public float MaxBuildPoints
        {
            get => _maxBuildPoints;
            set
            {
                _maxBuildPoints = value;
                OnAfterBuildPointsChanged();
            }
        }

        /// <summary>
        /// The ratio of the current build points to the max build points, in [0..1].
        /// </summary>
        public float BuildPercent => MaxBuildPoints <= 0
            ? 1.0f
            : BuildPoints / MaxBuildPoints;

        public List<IConstructionCondition> Conditions { get; set; }

        bool _isGhost = false; // false by default if omitted.

        /// <summary>
        /// Calculates the current build speed multiplier for this specific part, taking into account the build conditions.
        /// </summary>
        public float GetBuildSpeedMultiplier()
        {
            return Conditions?.Sum( c => c.GetBuildSpeedMultiplier( this.transform.position ) ) ?? 1.0f;
        }

        void OnAfterBuildPointsChanged()
        {
            if( !_isGhost && this.BuildPercent < 1.0f )
            {
                RunOriginalToGhost();
                var vessel = this.transform.GetVessel();
                if( vessel != null )
                    vessel.RecalculatePartCache();
            }
            else if( _isGhost && this.BuildPercent == 1.0f )
            {
                RunGhostToOriginal();
                var vessel = this.transform.GetVessel();
                if( vessel != null )
                    vessel.RecalculatePartCache();
            }
        }

        private void RunOriginalToGhost()
        {
            foreach( var (component, data) in _cachedData.ToArray() )
            {
                SerializationUnit.Populate<Component>( component, data.fwd, _cachedRefStore );
            }
        }

        private void RunGhostToOriginal()
        {
            foreach( var (component, data) in _cachedData.ToArray() )
            {
                SerializationUnit.Populate<Component>( component, data.rev, _cachedRefStore );
            }
        }

        Dictionary<Component, (SerializedData fwd, SerializedData rev)> _cachedData;
        BidirectionalReferenceStore _cachedRefStore = new BidirectionalReferenceStore();

        void Start()
        {
            RecalculateGhostAndUnghostData();

            OnAfterBuildPointsChanged();
        }


        private static void CollectComponentsRecursive<T>( Transform current, List<T> result ) where T : Component
        {
            current.GetComponents( result );

            foreach( Transform child in current )
            {
                CollectComponentsRecursive( child, result );
            }
        }

        private static List<T> GetComponentsInHierarchy<T>( Transform root ) where T : Component
        {
            var result = new List<T>();
            CollectComponentsRecursive( root, result );
            return result;
        }

        private IEnumerable<Component> GetGovernedComponents()
        {
            return GetComponentsInHierarchy<Component>( this.transform );
        }

        /// <summary>
        /// Caches the current state of the vessel.
        /// </summary>
        private void RecalculateGhostAndUnghostData()
        {
            if( _cachedData != null )
                return;

            _cachedData = new Dictionary<Component, (SerializedData fwd, SerializedData rev)>();

            _cachedRefStore.Clear();

            IEnumerable<Component> comps = GetGovernedComponents();

            foreach( var comp in comps )
            {
                SerializedData originalToGhost = SerializationUnit.Serialize<Component>( typeof( Contexts.Ctx.Ghost ), comp, _cachedRefStore );
                if( originalToGhost == null ) // Only cache what can be ghosted - should probably be signified differently than == null, but it works for now.
                    continue;
                SerializedData ghostToOriginal = SerializationUnit.Serialize<Component>( typeof( Ctx.Value ), comp, _cachedRefStore );

                // TODO - remove keys from revObj, that aren't present in forwardObj.
                /*if( originalToGhost is SerializedObject forwardObj && ghostToOriginal is SerializedObject revObj )
                {

                }*/

                _cachedData.Add( comp, (originalToGhost, ghostToOriginal) );
            }
        }

        //
        //
        //

        [MapsInheritingFrom( typeof( FConstructible ) )]
        public static IDescriptor FConstructibleMapping()
        {
            return new MemberwiseDescriptor<FConstructible>()
                .WithMember( "max_build_points", o => o._maxBuildPoints )
                .WithMember( "build_points", o => o._buildPoints )
                .WithMember( "is_ghost", o => o._isGhost )
                .WithMember( "cached_data", typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Value> ), o => o._cachedData );
        }
    }
}