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
        public static SerializedData GetData( this Transform t, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "local_position", s.WriteVector3( t.localPosition ) },
                { "local_rotation", s.WriteQuaternion( t.localRotation ) },
                { "local_scale", s.WriteVector3( t.localScale ) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Transform t, IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "local_position", out var localPosition ) )
                t.localPosition = l.ReadVector3( localPosition );

            if( data.TryGetValue( "local_rotation", out var localRotation ) )
                t.localRotation = l.ReadQuaternion( localRotation );

            if( data.TryGetValue( "local_scale", out var localScale ) )
                t.localScale = l.ReadVector3( localScale );
        }
    }
}