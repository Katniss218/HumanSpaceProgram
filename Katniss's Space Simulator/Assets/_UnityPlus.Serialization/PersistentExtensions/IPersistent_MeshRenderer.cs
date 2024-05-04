using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization
{
    public static class IPersistent_MeshRenderer
    {
        public static SerializedData GetData( this MeshRenderer mr, IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Renderer.GetData( mr, s );

            ret.AddAll( new SerializedObject()
            {
                { "shared_materials", new SerializedArray( mr.sharedMaterials.Select( mat => s.WriteAssetReference( mat ) ) ) },
                { "shadow_casting_mode", mr.shadowCastingMode.ToString().GetData() },
                { "receive_shadows", mr.receiveShadows.GetData() }
            } );

            return ret;
        }

        public static void SetData( this MeshRenderer mr, SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Renderer.SetData( mr, data, l );

            if( data.TryGetValue( "shared_materials", out var sharedMaterials ) )
            {
                List<Material> mats = new List<Material>();
                foreach( var sharedMatJson in (SerializedArray)sharedMaterials )
                {
                    Material mat = l.ReadAssetReference<Material>( sharedMatJson );
                    mats.Add( mat );
                }

                mr.sharedMaterials = mats.ToArray();
            }

            if( data.TryGetValue( "shadow_casting_mode", out var shadowCastingMode ) )
                mr.shadowCastingMode = Enum.Parse<ShadowCastingMode>( shadowCastingMode.AsString() );

            if( data.TryGetValue( "receive_shadows", out var receiveShadows ) )
                mr.receiveShadows = receiveShadows.AsBoolean();
        }
    }
}