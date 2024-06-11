using System;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class MappingHelper
    {
        // DoSave is the same everywhere.

#warning TODO - these can be moved into the SerializationMapping
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoPopulate<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            object obj2;
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveStruct:
                    obj2 = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    obj2 = obj; // Don't instantiate when populating, object should already be created.
                    mapping.Load( ref obj2, data, l );
                    break;
            }
            obj = (T)obj2;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoLoad<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            object obj2;
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveStruct:
                    obj2 = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    obj2 = mapping.Instantiate( data, l );
                    mapping.Load( ref obj2, data, l );
                    break;
            }
            obj = (T)obj2;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void DoLoadReferences<T>( SerializationMapping mapping, ref T obj, SerializedData data, ILoader l )
        {
            object obj2 = obj;
            switch( mapping.SerializationStyle )
            {
                default:
                    return;
                case SerializationStyle.PrimitiveObject:
                    obj2 = mapping.Instantiate( data, l );
                    break;
                case SerializationStyle.NonPrimitive:
                    mapping.LoadReferences( ref obj2, data, l );
                    break;
            }
            obj = (T)obj2;
        }
    }
}