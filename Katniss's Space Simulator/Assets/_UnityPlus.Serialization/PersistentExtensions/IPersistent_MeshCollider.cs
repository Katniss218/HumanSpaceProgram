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
    public static class IPersistent_MeshCollider
    {
        public static SerializedData GetData( this MeshCollider mc, ISaver s )
        {
            return new SerializedObject()
            {
                { "shared_mesh", s.WriteAssetReference( mc.sharedMesh ) },
                { "is_convex", mc.convex },
                { "is_trigger", mc.isTrigger }
            };
        }

        public static void SetData( this MeshCollider mc, ILoader l, SerializedObject data )
        {
            if( data.TryGetValue( "shared_mesh", out var sharedMesh ) )
                mc.sharedMesh = l.ReadAssetReference<Mesh>( sharedMesh );

            if( data.TryGetValue( "is_convex", out var isConvex ) )
                mc.convex = (bool)isConvex;

            if( data.TryGetValue( "is_trigger", out var isTrigger ) )
                mc.isTrigger = (bool)isTrigger;
        }
    }
}