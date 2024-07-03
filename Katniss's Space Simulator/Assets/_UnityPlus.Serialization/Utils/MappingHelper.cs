using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class MappingHelper
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type GetSerializedType<T>( SerializedData data )
        {
            if( data == null )
                return typeof( T );

            if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                return type.DeserializeType();

            return typeof( T );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializationMapping GetMapping_Load<T>( int context, Type memberType, SerializedData data, ILoader l )
        {
            if( data == null )
                return SerializationMappingRegistry.GetMapping<T>( context, memberType );

            if( l.MappingCache.TryGetValue( data, out var mapping ) )
                return mapping;

            mapping = SerializationMappingRegistry.GetMapping<T>( context, memberType );

            l.MappingCache[data] = mapping;

            return mapping;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializationMapping GetMapping_LoadReferences<T>( int context, T member, SerializedData data, ILoader l )
        {
            if( data == null )
                return SerializationMappingRegistry.GetMapping<T>( context, member );

            if( l.MappingCache.TryGetValue( data, out var mapping ) )
                return mapping;

            return SerializationMappingRegistry.GetMapping<T>( context, member );
        }
    }
}