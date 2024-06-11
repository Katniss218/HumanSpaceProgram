using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can both be referenced by other objects, and contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class NonPrimitiveSerializationMapping<TSource> : SerializationMapping, IInstantiableSerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, ILoader, object> OnInstantiate { get; set; }

        /// <summary>
        /// Loads the members.
        /// </summary>
        public LoadAction<TSource> OnLoad { get; set; }

        /// <summary>
        /// Loads the references.
        /// </summary>
        public LoadReferencesAction<TSource> OnLoadReferences { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.NonPrimitive;

        public NonPrimitiveSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, ISaver s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, ILoader l )
        {
            if( OnInstantiate != null )
                return OnInstantiate.Invoke( data, l );
            return default( TSource );
        }

        public override void Load( ref object obj, SerializedData data, ILoader l )
        {
            if( OnLoad != null )
            {
                // obj can be null here, this is normal.
                var obj2 = (TSource)obj;
                OnLoad.Invoke( ref obj2, data, l );
                obj = obj2;
            }
        }

        public override void LoadReferences( ref object obj, SerializedData data, ILoader l )
        {
            if( OnLoadReferences != null )
            {
                // obj can be null here, this is normal.
                var obj2 = (TSource)obj;
                OnLoadReferences.Invoke( ref obj2, data, l );
                obj = obj2;
            }
        }
    }
}