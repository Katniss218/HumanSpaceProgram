
using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// The default mapping, returned when no mapping was found.
    /// </summary>
    public class EmptySerializationMapping<TSource> : SerializationMapping
    {
        public override SerializationStyle SerializationStyle => SerializationStyle.None;

        internal EmptySerializationMapping() 
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            throw new InvalidOperationException( $"Save is not supported on `{nameof( EmptySerializationMapping<TSource> )}`." );
        }

        public override object Instantiate( SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"Instantiate is not supported on `{nameof( EmptySerializationMapping<TSource> )}`." );
        }

        public override void Load( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"Load is not supported on `{nameof( EmptySerializationMapping<TSource> )}`." );
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"LoadReferences is not supported on `{nameof( EmptySerializationMapping<TSource> )}`." );
        }
    }
}