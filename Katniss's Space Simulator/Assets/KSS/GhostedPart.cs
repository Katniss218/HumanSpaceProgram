using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

namespace KSS
{
    /// <summary>
    /// Represents a part/vessel that appears ghosted out.
    /// </summary>
    public class GhostedPart
    {
        private BidirectionalReferenceStore refMap;

        private EditObjectsPatch forwardPatch; // patch to make ghosted.
        private EditObjectsPatch reversePatch; // patch to stop ghosted.

        /// <summary>
        /// Makes the loader load a ghosted hierarchy.
        /// </summary>
        public static void ApplyTo( ILoader loader )
        {
            // 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="partMap"></param>
        /// <param name="refMap">The reference map saved from when the objects were loaded.</param>
        /// <returns></returns>
        public static GhostedPart MakeGhostPatch( FPart root, Dictionary<FPart, List<Transform>> partMap, BidirectionalReferenceStore refMap )
        {
            if( partMap.TryGetValue( root, out List<Transform> list ) )
            {
                List<(Guid id, SerializedData dataToApply)> forwardPatch = new List<(Guid id, SerializedData dataToApply)>();
                List<(Guid id, SerializedData dataToApply)> reversePatch = new List<(Guid id, SerializedData dataToApply)>();

                Material ghostMat = AssetRegistry.Get<Material>( "builtin::Resources/Materials/ghost" );
                List<Collider> colliders = new List<Collider>();
                List<Renderer> renderers = new List<Renderer>();
                foreach( var transform in list )
                {
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
                        }
                    }
                }

                return new GhostedPart()
                {
                    refMap = refMap,
                    forwardPatch = new EditObjectsPatch( forwardPatch ),
                    reversePatch = new EditObjectsPatch( reversePatch )
                };
            }
            throw new ArgumentException( $"the specified {nameof( root )} object must be contained in the {nameof( partMap )}.", nameof( root ) );
        }
    }
}