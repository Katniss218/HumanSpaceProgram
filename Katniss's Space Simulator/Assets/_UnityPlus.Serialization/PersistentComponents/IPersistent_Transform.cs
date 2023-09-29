using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistent_Transform
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this Transform t, ISaver s )
        {
            return new SerializedObject()
            {
                { "local_position", s.WriteVector3( t.localPosition ) },
                { "local_rotation", s.WriteQuaternion( t.localRotation ) },
                { "local_scale", s.WriteVector3( t.localScale ) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Transform t, ILoader l, SerializedObject data )
        {
            if( data.TryGetValue( "local_position", out var jsonLocalPosition ) )
                t.localPosition = l.ReadVector3( jsonLocalPosition );

            if( data.TryGetValue( "local_rotation", out var jsonLocalRotation ) )
                t.localRotation = l.ReadQuaternion( jsonLocalRotation );

            if( data.TryGetValue( "local_scale", out var jsonLocalScale ) )
                t.localScale = l.ReadVector3( jsonLocalScale );
        }
    }
}