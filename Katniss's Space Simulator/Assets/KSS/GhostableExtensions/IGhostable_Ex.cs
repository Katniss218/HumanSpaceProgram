using KSS.Components;
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
    public static class IGhostable_Ex
    {
        public static SerializedData GetGhostData( this Renderer renderer, IReverseReferenceMap s )
        {
            var ghostMat = AssetRegistry.Get<Material>( "builtin::Resources/Materials/ghost_wireframe" );

            SerializedArray ghostMats = new SerializedArray();
            for( int i = 0; i < renderer.sharedMaterials.Length; i++ )
            {
                ghostMats.Add( s.WriteAssetReference( ghostMat ) );
            }

            return new SerializedObject()
            {
                { "shared_materials", ghostMats }
            };
        }

        public static SerializedData GetGhostData( this Collider collider, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "is_trigger", true }
            };
        }

        public static SerializedData GetGhostData( this FPointMass mass, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "mass", 0.0f }
            };
        }
    }
}
