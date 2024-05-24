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
        [SerializationMappingProvider( typeof( CraneBuildCondition ) )]
        public static SerializationMapping CraneBuildConditionMapping()
        {
            return new CompoundSerializationMapping<CraneBuildCondition>()
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
            foreach( var kvp in _cachedData )
            {
#warning TODO - finish here
               // kvp.Key.SetData( kvp.Value.fwd, _cachedRefStore );
            }
        }

        private void RunGhostToOriginal()
        {
            foreach( var (component, data) in _cachedData )
            {
#warning TODO - finish here
                // component.SetData( data.rev, _cachedRefStore );
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
            }
            _cachedRefStore.Clear();

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( transform );
            if( partMap.TryGetValue( this, out var ourPartsTransforms ) )
            {
                foreach( var transform in ourPartsTransforms ) // this entire thing could be ran once per entire vessel and cached until something is added/removed from it.
                {
                    foreach( var comp in transform.GetComponents() )
                    {
                        SerializedData originalToGhost = comp.GetGhostData( _cachedRefStore );

                        // Only cache things that are ghostable.
                        // This should probably be signified differently than by a null, but it works for now.
                        if( originalToGhost != null )
                        {
                            var mapping = SerializationMappingRegistry.GetMappingOrDefault<Component>( comp );
                            SerializedData ghostToOriginal = mapping.Save( comp, _cachedRefStore );

                            // TODO - remove keys from revObj, that aren't present in forwardObj.
                            /*if( originalToGhost is SerializedObject forwardObj && ghostToOriginal is SerializedObject revObj )
                            {
                                
                            }*/

                            if( wasNull )
                            {
                                _cachedData.Add( comp, (originalToGhost, ghostToOriginal) );
                            }
                        }
                    }
                }
            }
        }

        //
        //
        //
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "build_points", BuildPoints.GetData() },
                { "max_build_points", MaxBuildPoints.GetData() },
                // todo - conditions.
            } );

            SerializedArray arr = new SerializedArray();
            foreach( var kvp in _cachedData )
            {
                arr.Add( new SerializedObject()
                {
                    { "object", s.WriteObjectReference( kvp.Key ) },
                    { "forward", kvp.Value.fwd },
                    { "reverse", kvp.Value.rev }
                } );
            }

            ret.AddAll( new SerializedObject()
            {
                { "cached_data", arr },
                // todo - conditions.
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue<SerializedArray>( "cached_data", out var cachedData ) )
            {
                _cachedData = new();
                foreach( var obj in cachedData.Cast<SerializedObject>() )
                {
                    Component comp = (Component)l.ReadObjectReference( obj["object"] );
                    _cachedData.Add( comp, (obj["forward"], obj["reverse"]) );
                }
            }

            // Set the underlying private fields as to not trigger ghosting prematurely.
            // The ghosting will be called anyway, in `Start()`.
            if( data.TryGetValue( "max_build_points", out var maxBuildPoints ) )
                _maxBuildPoints = maxBuildPoints.AsFloat();

            if( data.TryGetValue( "build_points", out var buildPoints ) )
                _buildPoints = buildPoints.AsFloat();

        }
        */
        [SerializationMappingProvider( typeof( FConstructible ) )]
        public static SerializationMapping FConstructibleMapping()
        {
            return new CompoundSerializationMapping<FConstructible>()
            {
                ("cached_data", new Member<FConstructible, Dictionary<Component, (SerializedData fwd, SerializedData rev)>>( o => o._cachedData )),
                ("max_build_points", new Member<FConstructible, float>( o => o._maxBuildPoints )),
                ("build_points", new Member<FConstructible, float>( o => o._buildPoints ))
                // todo - conditions.
            }
            .UseBaseTypeFactory()
            .IncludeMembers<Behaviour>();
        }
    }
}