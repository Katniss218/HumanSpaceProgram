using System;

namespace UnityPlus.Serialization
{
    public enum ObjectStructure : byte
    {
        /// <summary>
        /// primitive without a wrapper.
        /// </summary>
        Unwrapped,
        /// <summary>
        /// primitive or collection wrapped in an object with metadata headers and a "value" field.
        /// </summary>
        Wrapped,
        /// <summary>
        /// object with metadata headers, but the members are written directly on the root object instead of a "value" field.
        /// </summary>
        InlineMetadata
    }

    public static class SerializationHelpers
    {
        public static void DetermineObjectStructure( Type declaredType, Type actualType, out bool needsId, out bool needsType )
        {
            needsId = (!actualType.IsValueType
                && actualType != typeof( string ))
                || declaredType == typeof( object );

            needsType = declaredType != actualType
                && !declaredType.IsValueType
                && !declaredType.IsSealed
                && !typeof( Delegate ).IsAssignableFrom( declaredType );
        }

        public static void WriteMetadata( object value, ref SerializedData data, ObjectStructure structure, SerializationContext context, Type actualType, bool needsId, bool needsType )
        {
            if( structure == ObjectStructure.Unwrapped )
                return;

            if( structure == ObjectStructure.InlineMetadata )
            {
                if( data is SerializedObject objRaw )
                {
                    if( needsId )
                        Persistent_Guid.WriteIdHeader( objRaw, context.ReverseMap.GetID( value ) );
                    if( needsType )
                        Persistent_Type.WriteTypeHeader( objRaw, actualType );
                }
                return;
            }

            if( structure == ObjectStructure.Wrapped )
            {
                var wrapper = new SerializedObject();
                if( needsId ) 
                    Persistent_Guid.WriteIdHeader( wrapper, context.ReverseMap.GetID( value ) );
                if( needsType ) 
                    Persistent_Type.WriteTypeHeader( wrapper, actualType );
                wrapper[KeyNames.VALUE] = data;
                data = wrapper;
            }
        }
        /// <summary>
        /// Extracts the underlying SerializedArray from a data node.
        /// </summary>
        public static SerializedArray GetValueNode( SerializedData data )
        {
            if( data is SerializedArray arr )
            {
                return arr;
            }

            if( data is SerializedObject obj && obj.TryGetValue( KeyNames.VALUE, out SerializedData inner ) && inner is SerializedArray innerArr )
            {
                return innerArr;
            }

            return null;
        }
    }
}