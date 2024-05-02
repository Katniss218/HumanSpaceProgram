using KSS.Core;
using KSS.GameplayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace KSS.Components
{
    public interface IConstructionCondition : IPersistsData
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

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "min_lift_capacity", this.minLiftCapacity }
            };
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "min_lift_capacity", out var minLiftCapacity ) )
                this.minLiftCapacity = (float)minLiftCapacity;
        }
    }


    //
    //
    //


    /// <summary>
    /// Represents a "part" (hierarchy of gameobjects) that can be in various stages of construction.
    /// </summary>
    public class FConstructible : MonoBehaviour, IPersistsData
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
                float oldBuildPerc = BuildPercent;
                _buildPoints = value;
                OnAfterBuildPointPercentChanged( BuildPercent - oldBuildPerc );
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
                float oldBuildPerc = BuildPercent;
                _maxBuildPoints = value;
                OnAfterBuildPointPercentChanged( BuildPercent - oldBuildPerc );
            }
        }

        /// <summary>
        /// The ratio of the current build points to the max build points, in [0..1].
        /// </summary>
        public float BuildPercent => MaxBuildPoints <= 0
            ? 1.0f
            : BuildPoints / MaxBuildPoints;

        public List<IConstructionCondition> Conditions { get; set; }

        /// <summary>
        /// Calculates the current build speed multiplier for this specific part, taking into account the build conditions.
        /// </summary>
        public float GetBuildSpeedMultiplier()
        {
            return Conditions?.Sum( c => c.GetBuildSpeedMultiplier( this.transform.position ) ) ?? 1.0f;
        }

        void OnAfterBuildPointPercentChanged( float delta )
        {
            // This method shouldn't handle the destroying of the part from the vessel after deconstruction is 'finished'.

            if( (this.BuildPercent >= 0.0f && this.BuildPercent - delta <= 0.0f)  // start construction
             || (this.BuildPercent < 1.0f && this.BuildPercent - delta >= 1.0f) ) // start deconstruction
            {
                RunOriginalToGhost();
                var vessel = this.transform.GetPartObject();
                if( vessel != null )
                    vessel.RecalculatePartCache();
            }

            if( this.BuildPercent >= 1.0f && this.BuildPercent - delta <= 1.0f ) // end construction
            {
                RunGhostToOriginal();
                var vessel = this.transform.GetPartObject();
                if( vessel != null )
                    vessel.RecalculatePartCache();
            }
        }

        private void RunOriginalToGhost()
        {
            foreach( var kvp in _cachedData )
            {
                kvp.Key.SetData( kvp.Value.fwd, _cachedRefStore );
            }
        }

        private void RunGhostToOriginal()
        {
            foreach( var (component, data) in _cachedData )
            {
                component.SetData( data.rev, _cachedRefStore );
            }
        }

        Dictionary<Component, (SerializedData fwd, SerializedData rev)> _cachedData = new();
        BidirectionalReferenceStore _cachedRefStore = new BidirectionalReferenceStore();

        void Start()
        {
            CacheGhostAndUnghostData();
            OnAfterBuildPointPercentChanged( this.BuildPercent );
        }

        /// <summary>
        /// Caches the current state of the vessel.
        /// </summary>
        private void CacheGhostAndUnghostData()
        {
            _cachedData.Clear();
            _cachedRefStore.Clear(); // This needs to be recalculated whenever the vessel changes (i..e when the part/component instances become invalidated).
                                     // this method should be recalculated whenever any value of the component changes tbh.

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( transform );
            if( partMap.TryGetValue( this, out var ourPartsTransforms ) )
            {
                foreach( var transform in ourPartsTransforms ) // this entire thing could be ran once per entire vessel and cached until something is added/removed from it.
                {
                    foreach( var comp in transform.GetComponents() )
                    {
                        SerializedData originalToGhost = comp.GetGhostData( _cachedRefStore );
                        // Only cache things that are ghostable. This should probably be something else than null, but it works for now.
                        if( originalToGhost != null )
                        {
                            SerializedData ghostToOriginal = comp.GetData( _cachedRefStore );

                            _cachedData.Add( comp, (originalToGhost, ghostToOriginal) );
                        }
                    }
                }
            }

            // construction site would only manage the distribution of build points, etc.
        }

        //
        //
        //

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "build_points", this.BuildPoints },
                { "max_build_points", this.MaxBuildPoints },
                // todo - conditions.
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "max_build_points", out var maxBuildPoints ) )
                this._maxBuildPoints = (float)maxBuildPoints;

            if( data.TryGetValue( "build_points", out var buildPoints ) )
                this._buildPoints = (float)buildPoints;

            // todo - conditions.
        }
    }
}