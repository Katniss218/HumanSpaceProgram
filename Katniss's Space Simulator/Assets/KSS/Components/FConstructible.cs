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
        public static SerializationMapping CraneBuildConditionMapping()
        {
            return new MemberwiseSerializationMapping<CraneBuildCondition>()
            {
                ("min_lift_capacity", new Member<CraneBuildCondition, float>( o => o.minLiftCapacity ))
            };
        }
    }


    //
    //
    //


    /// <summary>
    /// Represents a "part" (hierarchy of gameobjects) that can be in various stages of construction.
    /// </summary>
    public class FConstructible : MonoBehaviour
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
            foreach( var (component, data) in _cachedData )
            {
                SerializationUnit.Populate<Component>( component, data.fwd );
            }
        }

        private void RunGhostToOriginal()
        {
            foreach( var (component, data) in _cachedData )
            {
                SerializationUnit.Populate<Component>( component, data.rev );
            }
        }

        Dictionary<Component, (SerializedData fwd, SerializedData rev)> _cachedData;
        BidirectionalReferenceStore _cachedRefStore = new BidirectionalReferenceStore();

        void Start()
        {
            RecalculateGhostAndUnghostData();

            OnAfterBuildPointPercentChanged( this.BuildPercent );
        }

        /// <summary>
        /// Caches the current state of the vessel.
        /// </summary>
        private void RecalculateGhostAndUnghostData()
        {
#warning TODO - this runs in the VAB scene too, and saves the entire data twice, even if the current state is equal.
            bool wasNull = _cachedData == null;
            if( wasNull )
            {
                _cachedData = new();

                _cachedRefStore.Clear();

                // this entire thing could be ran once per entire vessel and cached until something is added/removed from it.
                AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( transform );
                if( partMap.TryGetValue( this, out var ourPartsTransforms ) )
                {
                    IEnumerable<Component> comps = ourPartsTransforms.SelectMany( t => t.GetComponents() );

                    var su = SerializationUnit.FromObjects<Component>( GhostableContext.Ghost, comps );

                    su.Serialize( _cachedRefStore );

                    foreach( var originalToGhost in su.GetData().Where( d => d != null ) )
                    {
                        // Only cache things that are ghostable.
                        // This should probably be signified differently than by a null, but it works for now.

                        Component comp = (Component)_cachedRefStore.GetObj( originalToGhost[KeyNames.ID].DeserializeGuid() );
                        su = SerializationUnit.FromObjects<Component>( ObjectContext.Value, comp );

                        su.Serialize( _cachedRefStore );

                        var ghostToOriginal = su.GetData().First();

                        // TODO - remove keys from revObj, that aren't present in forwardObj.
                        /*if( originalToGhost is SerializedObject forwardObj && ghostToOriginal is SerializedObject revObj )
                        {

                        }*/

                        _cachedData.Add( comp, (originalToGhost, ghostToOriginal) );
                    }
                }
            }
        }

        //
        //
        //

        [MapsInheritingFrom( typeof( FConstructible ) )]
        public static SerializationMapping FConstructibleMapping()
        {
            return new MemberwiseSerializationMapping<FConstructible>()
            {
                ("max_build_points", new Member<FConstructible, float>( o => o._maxBuildPoints )),
                ("build_points", new Member<FConstructible, float>( o => o._buildPoints )),
                // todo - conditions.
#warning TODO - the dict RefToValue doesn't load properly.
                ("cached_data", new Member<FConstructible, Dictionary<Component, (SerializedData fwd, SerializedData rev)>>( KeyValueContext.RefToValue, o => o._cachedData ))
            };
        }
    }
}