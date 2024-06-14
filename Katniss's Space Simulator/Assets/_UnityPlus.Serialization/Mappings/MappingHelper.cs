using System;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class MappingHelper
    {
        // DoSave is the same everywhere.

#warning TODO - these can be moved into the SerializationMapping
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool DoPopulate<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            switch( mapping.SerializationStyle )
            {
                default:
                    return false;
                case SerializationStyle.PrimitiveStruct:
                    obj = (T)mapping.Instantiate( data, l );
                    return true;
                case SerializationStyle.NonPrimitive:
                    object obj2 = obj; // Don't instantiate when populating, object should already be created.
                    mapping.Load( ref obj2, data, l );
                    obj = (T)obj2;
                    return true;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool DoLoad<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            switch( mapping.SerializationStyle )
            {
                default:
                    return false;
                case SerializationStyle.PrimitiveStruct:
                    obj = (T)mapping.Instantiate( data, l );
                    return true;
                case SerializationStyle.NonPrimitive:
                    object obj2 = mapping.Instantiate( data, l );
                    mapping.Load( ref obj2, data, l );
                    obj = (T)obj2;
                    return true;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool DoLoadReferences<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            switch( mapping.SerializationStyle )
            {
                default:
                    return false;
                case SerializationStyle.PrimitiveObject:
                    obj =  (T)mapping.Instantiate( data, l );
                    return true;
                case SerializationStyle.NonPrimitive:
                    object obj2 = obj;
                    mapping.LoadReferences( ref obj2, data, l );
                    obj = (T)obj2;
                    return true;
            }
        }
    }
}