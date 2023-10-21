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
    public static class IPersistent_MeshFilter
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this MeshFilter mf, ISaver s )
        {
            return new SerializedObject()
            {
                { "shared_mesh", s.WriteAssetReference( mf.sharedMesh ) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this MeshFilter mf, ILoader l, SerializedObject data )
        {
            if( data.TryGetValue( "shared_mesh", out var sharedMesh ) )
                mf.sharedMesh = l.ReadAssetReference<Mesh>( sharedMesh );
        }
    }
}