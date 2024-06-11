using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class PrimitiveObjectSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.PrimitiveObject;

        public PrimitiveObjectSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, ISaver s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, ILoader l )
        {
            if( OnInstantiate != null )
                return OnInstantiate.Invoke( data, l.RefMap );
            return default( TSource );
        }

        public override void Load( ref object obj, SerializedData data, ILoader l )
        {
            throw new InvalidOperationException( $"Load is not supported on `{nameof( PrimitiveObjectSerializationMapping<TSource> )}`." );
        }

        public override void LoadReferences( ref object obj, SerializedData data, ILoader l )
        {
            throw new InvalidOperationException( $"LoadReferences is not supported on `{nameof( PrimitiveObjectSerializationMapping<TSource> )}`." );
        }
    }
}