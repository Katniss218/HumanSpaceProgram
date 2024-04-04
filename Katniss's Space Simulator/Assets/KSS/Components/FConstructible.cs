using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

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
        public float BuildPercent => BuildPoints / MaxBuildPoints;

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
            foreach( var kvp in _twoWayPatch )
            {
                kvp.Key.SetData( kvp.Value.fwd, null );
            }
        }

        private void RunGhostToOriginal()
        {
            foreach( var kvp in _twoWayPatch )
            {
                kvp.Key.SetData( kvp.Value.rev, null );
            }
        }

        Dictionary<Component, (SerializedData fwd, SerializedData rev)> _twoWayPatch = new Dictionary<Component, (SerializedData fwd, SerializedData rev)>();

        void Start()
        {
            SaveState();
            OnAfterBuildPointPercentChanged( this.BuildPercent );
        }

        public static List<Func<Transform, IEnumerable<KeyValuePair<Component, (SerializedData fwd, SerializedData rev)>>>> PatchGetters { get; private set; } = new()
        {
            GetColliderPatches,
            GetRendererPatches,
            GetFDryMassPatches
        };

        private void SaveState()
        {
            _twoWayPatch.Clear();

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( transform );
            if( partMap.TryGetValue( this, out var ourPartsTransforms ) )
            {
                foreach( var transform in ourPartsTransforms ) // this entire thing could be ran once per entire vessel and cached until something is added/removed from it.
                {
                    foreach( var getter in PatchGetters )
                    {
                        foreach( var kvp in getter.Invoke( transform ) )
                        {
                            _twoWayPatch.Add( kvp.Key, kvp.Value );
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
            return new SerializedObject()
            {
                { "build_points", this.BuildPoints },
                { "max_build_points", this.MaxBuildPoints },
                // todo - conditions.
            };
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "max_build_points", out var maxBuildPoints ) )
                this._maxBuildPoints = (float)maxBuildPoints;

            if( data.TryGetValue( "build_points", out var buildPoints ) )
                this._buildPoints = (float)buildPoints;

            // todo - conditions.
        }


        private static IEnumerable<KeyValuePair<Component, (SerializedData fwd, SerializedData rev)>> GetColliderPatches( Transform transform )
        {
            foreach( var collider in transform.GetComponents<Collider>() )
            {
                SerializedObject fwdObj = new SerializedObject()
                {
                    { "is_trigger", true }
                };
                SerializedObject revObj = new SerializedObject()
                {
                    { "is_trigger", collider.isTrigger }
                };

                yield return new( collider, (fwdObj, revObj) );
            }
        }

        const string GhostMaterialAssetID = "builtin::Resources/Materials/ghost";
        static Material ghostMat = null;

        private static IEnumerable<KeyValuePair<Component, (SerializedData fwd, SerializedData rev)>> GetRendererPatches( Transform transform )
        {
            foreach( var renderer in transform.GetComponents<Renderer>() )
            {
                if( ghostMat == null )
                {
                    ghostMat = AssetRegistry.Get<Material>( GhostMaterialAssetID );
                }

                var sharedMaterials = renderer.sharedMaterials;

                var mats = sharedMaterials.Select( mat => ((IReverseReferenceMap)null).WriteAssetReference( mat ) );
                SerializedArray origMats = new SerializedArray( mats );

                SerializedArray ghostMats = new SerializedArray();
                for( int i = 0; i < sharedMaterials.Length; i++ )
                    ghostMats.Add( ((IReverseReferenceMap)null).WriteAssetReference( ghostMat ) );

                SerializedObject fwdObj = new SerializedObject()
                {
                    { "shared_materials", ghostMats }
                };
                SerializedObject revObj = new SerializedObject()
                {
                    { "shared_materials", origMats }
                };

                yield return new( renderer, (fwdObj, revObj) );
            }
        }

        private static IEnumerable<KeyValuePair<Component, (SerializedData fwd, SerializedData rev)>> GetFDryMassPatches( Transform transform )
        {
            foreach( var mass in transform.GetComponents<FPointMass>() )
            {
                SerializedObject fwdObj = new SerializedObject()
                {
                    { "mass", 0.0f }
                };
                SerializedObject revObj = new SerializedObject()
                {
                    { "mass", mass.Mass }
                };

                yield return new( mass, (fwdObj, revObj) );
            }
        }
    }
}