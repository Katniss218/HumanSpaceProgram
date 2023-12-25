using KSS.Components;
using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Patching;

namespace KSS
{
    /// <summary>
    /// Represents a single constructible object that can be transitioned between its normal, and ghosted-out states.
    /// </summary>
    public struct BidirectionalGhostPatch
    {
        public static string GhostMaterialAssetID = "builtin::Resources/Materials/ghost";

        /// <summary>
        /// The original-to-ghost (forward) patch.
        /// </summary>
        public SetDataPatch Forward { get; private set; }

        /// <summary>
        /// The ghost-to-original (reverse) patch.
        /// </summary>
        public SetDataPatch Reverse { get; private set; }

        /// <param name="refMap">The reference map saved from when the objects were loaded.</param>
        public static BidirectionalGhostPatch CreateGhostPatch( IEnumerable<Transform> transforms, IReverseReferenceMap refMap )
        {
            Material ghostMat = AssetRegistry.Get<Material>( GhostMaterialAssetID );

            List<(Guid id, SerializedData dataToApply)> forwardPatch = new List<(Guid id, SerializedData dataToApply)>();
            List<(Guid id, SerializedData dataToApply)> reversePatch = new List<(Guid id, SerializedData dataToApply)>();

            List<FDryMass> masses = new List<FDryMass>();
            List<Collider> colliders = new List<Collider>();
            List<Renderer> renderers = new List<Renderer>();
            foreach( var transform in transforms )
            {
                // TODO - this should be procedural, mods might define their own stuff that should be deactivated by a patch.
                transform.GetComponents<Collider>( colliders );
                foreach( var collider in colliders )
                {
                    if( refMap.TryGetID( collider, out Guid id ) )
                    {
                        SerializedObject fwdObj = new SerializedObject()
                            {
                                { "is_trigger", true }
                            };
                        SerializedObject revObj = new SerializedObject()
                            {
                                { "is_trigger", collider.isTrigger }
                            };
                        forwardPatch.Add( (id, fwdObj) );
                        reversePatch.Add( (id, revObj) );
                    }
                }
                transform.GetComponents<Renderer>( renderers );
                foreach( var renderer in renderers )
                {
                    if( refMap.TryGetID( renderer, out Guid id ) )
                    {
                        var sharedMaterials = renderer.sharedMaterials;

                        var mats = sharedMaterials.Select( mat => refMap.WriteAssetReference( mat ) );
                        SerializedArray origMats = new SerializedArray( mats );

                        SerializedArray ghostMats = new SerializedArray();
                        for( int i = 0; i < sharedMaterials.Length; i++ )
                            ghostMats.Add( refMap.WriteAssetReference( ghostMat ) );

                        SerializedObject fwdObj = new SerializedObject()
                            {
                                { "shared_materials", ghostMats }
                            };
                        SerializedObject revObj = new SerializedObject()
                            {
                                { "shared_materials", origMats }
                            };
                        forwardPatch.Add( (id, fwdObj) );
                        reversePatch.Add( (id, revObj) );
                    }
                }
                transform.GetComponents<FDryMass>( masses );
                foreach( var mass in masses )
                {
                    if( refMap.TryGetID( mass, out Guid id ) )
                    {
                        SerializedObject fwdObj = new SerializedObject()
                            {
                                { "mass", 0.0f }
                            };
                        SerializedObject revObj = new SerializedObject()
                            {
                                { "mass", mass.Mass }
                            };
                        forwardPatch.Add( (id, fwdObj) );
                        reversePatch.Add( (id, revObj) );
                    }
                }
            }

            return new BidirectionalGhostPatch()
            {
                Forward = new SetDataPatch( forwardPatch ),
                Reverse = new SetDataPatch( reversePatch )
            };
        }
    }
}