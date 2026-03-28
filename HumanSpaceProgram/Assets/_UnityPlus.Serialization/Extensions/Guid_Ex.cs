using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization
{
    public static class Persistent_Guid
    {
        // --- Fluent API: Semantic Shortcuts ---

        public static MemberwiseDescriptor<T> WithAsset<T, TMember>( this MemberwiseDescriptor<T> self, string name, Expression<Func<T, TMember>> accessor )
        {
            return self.WithMember( name, typeof( Ctx.Asset ), accessor );
        }

        public static MemberwiseDescriptor<T> WithReference<T, TMember>( this MemberwiseDescriptor<T> self, string name, Expression<Func<T, TMember>> accessor )
        {
            return self.WithMember( name, typeof( Ctx.Ref ), accessor );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedPrimitive SerializeGuid( this Guid guid )
        {
            // GUIDs should be saved in the '00000000-0000-0000-0000-000000000000' format, with dashes, and without extra anything.
            return (SerializedPrimitive)guid.ToString( "D" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Guid DeserializeGuid( this SerializedData data )
        {
            return Guid.ParseExact( (string)data, "D" );
        }

        // --- Header Helpers ---

        /// <summary>
        /// Writes the $id header to the serialized object.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteIdHeader( SerializedObject data, Guid id )
        {
            if( data == null || id == Guid.Empty )
                return;
            data[KeyNames.ID] = SerializeGuid( id );
        }

        /// <summary>
        /// Tries to read the $id header from the serialized object.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryReadIdHeader( SerializedObject data, out Guid id )
        {
            id = Guid.Empty;
            if( data == null )
                return false;
            if( data.TryGetValue( KeyNames.ID, out SerializedData val ) && val is SerializedPrimitive prim )
            {
                // We use TryParseExact for safety in case of malformed data during deserialization
                return Guid.TryParseExact( (string)prim, "D", out id );
            }
            return false;
        }

        /// <summary>
        /// Tries to read the $ref header from the serialized object.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryReadRefHeader( SerializedObject data, out Guid id )
        {
            id = Guid.Empty;
            if( data == null )
                return false;
            if( data.TryGetValue( KeyNames.REF, out SerializedData val ) && val is SerializedPrimitive prim )
            {
                return Guid.TryParseExact( (string)prim, "D", out id );
            }
            return false;
        }
    }
}