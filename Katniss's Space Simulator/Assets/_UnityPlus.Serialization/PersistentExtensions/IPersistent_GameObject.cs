using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistent_GameObject
    {
        /// <summary>
        /// Writes the 'data' part of a gameobject (only gameobject, not components).
        /// </summary>
        /// <param name="objects">The serialized array to add the serialized data object to.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this GameObject gameObject, ISaver s )
        {
            return new SerializedObject()
            {
                { $"{SerializerUtils.REF}", s.WriteGuid( s.GetReferenceID( gameObject ) ) },
                { "name", gameObject.name },
                { "layer", gameObject.layer },
                { "is_active", gameObject.activeSelf },
                { "is_static", gameObject.isStatic },
                { "tag", gameObject.tag }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this GameObject gameObject, ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "name", out var name ) )
                gameObject.name = (string)name;

            if( data.TryGetValue( "layer", out var layer ) )
                gameObject.layer = (int)layer;

            if( data.TryGetValue( "is_active", out var isActive ) )
                gameObject.SetActive( (bool)isActive );

            if( data.TryGetValue( "is_static", out var isStatic ) )
                gameObject.isStatic = (bool)isStatic;

            if( data.TryGetValue( "tag", out var tag ) )
                gameObject.tag = (string)tag;
        }
    }
}
