using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class SerializationMapping_Ex
    {
        /// <summary>
        /// Use this method to invoke a mapping.
        /// </summary>
        /// <remarks>
        /// Doesn't require doing a null check on the mapping.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializationResult SafeSave<TMember>( this SerializationMapping mapping, TMember obj, ref SerializedData data, ISaver s )
        {
            if( mapping == null )
                return SerializationResult.PrimitiveFinishedFailed;

            return mapping.Save<TMember>( obj, ref data, s );
        }

        /// <summary>
        /// Use this method to invoke a mapping.
        /// </summary>
        /// <remarks>
        /// Doesn't require doing a null check on the mapping.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializationResult SafeLoad<TMember>( this SerializationMapping mapping, ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( mapping == null )
                return SerializationResult.PrimitiveFinishedFailed;

            return mapping.Load<TMember>( ref obj, data, l, populate );
        }
    }
}