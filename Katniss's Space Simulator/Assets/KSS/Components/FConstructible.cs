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
    public interface IConstructionCondition : IPersistent
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

        public void SetData( IForwardReferenceMap l, SerializedData data )
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
    public class FConstructible : MonoBehaviour, IPersistent
    {
        public enum State
        {
            FinishedDeconstruction = -1,
            InProgress = 0,
            FinishedConstruction = 1
        }

        // state can be more than just ghost. A ghost would be for things that are not built yet. there can be a separate state for in-progress, etc.
        public State CurrentState { get; private set; }

        /// <summary>
        /// The current total build points the part has accumulated.
        /// </summary>
        public float BuildPoints { get; private set; }

        /// <summary>
        /// The total number of build points required to complete the colstruction/deconstruction of this part.
        /// </summary>
        /// <remarks>
        /// One build point at 1x build speed takes 1 [s] to build.
        /// </remarks>
        [field: SerializeField]
        public float MaxBuildPoints { get; set; }

        /// <summary>
        /// The ratio of the current build points to the max build points, in [0..1].
        /// </summary>
        public float BuildPercent => BuildPoints / MaxBuildPoints;

        /// <summary>
        /// Changes the <see cref="BuildPoints"/> by a specified delta.
        /// </summary>
        public void ChangeBuildPoints( float delta )
        {
            this.BuildPoints += delta;
            OnAfterBuildPointsChanged( delta );
        }

        [field: SerializeField]
        public List<IConstructionCondition> Conditions { get; set; }

        /// <summary>
        /// Calculates the current build speed multiplier for this specific part, taking into account the build conditions.
        /// </summary>
        public float GetBuildSpeedMultiplier()
        {
            return Conditions.Sum( c => c.GetBuildSpeedMultiplier( this.transform.position ) );
        }

        void OnAfterBuildPointsChanged( float delta )
        {
            if( BuildPercent <= 0.0f || BuildPercent >= 1.0f )
                this.transform.GetVessel().RecalculateParts();

            // if previous build perc was below 1, and current is >= 1 -> run ghost to original patch.
            // this doesn't handle the removal of the part from the vessel after deconstruction is finished.
        }

        Dictionary<Component, (SerializedData fwd, SerializedData rev)> _twoWayPatch = new Dictionary<Component, (SerializedData fwd, SerializedData rev)>();

        private void RunOriginalToGhost()
        {
            foreach( var kvp in _twoWayPatch )
            {
                kvp.Key.SetData( null, kvp.Value.fwd );
            }
        }

        private void RunGhostToOriginal()
        {
            foreach( var kvp in _twoWayPatch )
            {
                kvp.Key.SetData( null, kvp.Value.rev );
            }
        }

        void OnEnable()
        {
            if( !this._twoWayPatch.Any() )
            {
                SaveState();
            }
            RunOriginalToGhost();
        }

        void OnDisable()
        {
            RunGhostToOriginal();
        }

        private void SaveState()
        {
            _twoWayPatch.Clear();

            AncestralMap<FConstructible> partMap = AncestralMap<FConstructible>.Create( transform );
            if( partMap.TryGetValue( this, out var ourPartsTransforms ) )
            {
#warning TODO - add custom patch getters.
                List<FDryMass> massComponents = new List<FDryMass>();
                List<Renderer> rendererComponents = new List<Renderer>();
                List<Collider> colliderComponents = new List<Collider>();
                foreach( var transform in ourPartsTransforms ) // this entire thing could be ran once per entire vessel and cached until something is added/removed from it.
                {
                    transform.GetComponents( colliderComponents );
                    foreach( var collider in colliderComponents )
                    {
                        _twoWayPatch.Add( collider, GetColliderPatch( collider ) );
                    }
#error doesn't work somewhy
                    transform.GetComponents( rendererComponents );
                    foreach( var renderer in rendererComponents )
                    {
                        _twoWayPatch.Add( renderer, GetRendererPatch( renderer ) );
                    }

                    transform.GetComponents( massComponents );
                    foreach( var mass in massComponents )
                    {
                        _twoWayPatch.Add( mass, GetFDryMassPatch( mass ) );
                    }
                }
            }

            // patches can be applied on the fly because the objects are already in memory, and we know what we want to do to them.

            // construction site would only manage the distribution of build points, etc.
        }

        //
        //
        //

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject dict = new SerializedObject();
            foreach( var kvp in this._twoWayPatch )
            {
                dict.Add( s.GetID( kvp.Key ).ToString( "D" ), new SerializedObject()
                {
                    { "fwd", kvp.Value.fwd },
                    { "rev", kvp.Value.rev }
                } );
            }
            return new SerializedObject()
            {
                { "max_build_points", this.MaxBuildPoints },
                { "saved_patch", dict }
                // todo - conditions.
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "max_build_points", out var maxbuildPoints ) )
                this.MaxBuildPoints = (float)maxbuildPoints;

            if( data.TryGetValue( "saved_patch", out var savedPatch ) )
            {
                this._twoWayPatch.Clear();
                foreach( var kvp in (SerializedObject)savedPatch )
                {
                    Component c = (Component)l.GetObj( Guid.ParseExact( kvp.Key, "D" ) );
                    this._twoWayPatch.Add( c, (kvp.Value["fwd"], kvp.Value["rev"]) );
                }
            }

            // todo - conditions.
        }


        private static (SerializedData fwd, SerializedData rev) GetColliderPatch( Collider collider )
        {
            SerializedObject fwdObj = new SerializedObject()
            {
                { "is_trigger", true }
            };
            SerializedObject revObj = new SerializedObject()
            {
                { "is_trigger", collider.isTrigger }
            };
            return (fwdObj, revObj);
        }

        const string GhostMaterialAssetID = "builtin::Resources/Materials/ghost";
        static Material ghostMat = null;

        private static (SerializedData fwd, SerializedData rev) GetRendererPatch( Renderer renderer )
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
            return (fwdObj, revObj);
        }

        private static (SerializedData fwd, SerializedData rev) GetFDryMassPatch( FDryMass mass )
        {
            SerializedObject fwdObj = new SerializedObject()
            {
                { "mass", 0.0f }
            };
            SerializedObject revObj = new SerializedObject()
            {
                { "mass", mass.Mass }
            };
            return (fwdObj, revObj);
        }
    }
}