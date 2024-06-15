using System;

namespace UnityPlus.Serialization
{
    public delegate void LoadAction2<TTemp, TSource>( NonPrimitiveSerializationMapping2<TTemp, TSource> mapping, ref TSource obj, SerializedData data, ILoader l ) where TTemp : class;
    public delegate void LoadReferencesAction2<TTemp, TSource>( NonPrimitiveSerializationMapping2<TTemp, TSource> mapping, ref TSource obj, SerializedData data, ILoader l ) where TTemp : class;

    /// <summary>
    /// Maps an object that can both be referenced by other objects, and contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class NonPrimitiveSerializationMapping2<TTemp, TSource> : SerializationMapping, IInstantiableSerializationMapping where TTemp : class
    {
        public TTemp temp;

        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, ILoader, object> OnInstantiate { get; set; }
        
        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, ILoader, TTemp> OnInstantiateTemp { get; set; }
        
        /// <summary>
        /// Loads the members.
        /// </summary>
        public LoadAction2<TTemp, TSource> OnLoad { get; set; }

        /// <summary>
        /// Loads the references.
        /// </summary>
        public LoadReferencesAction2<TTemp, TSource> OnLoadReferences { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.NonPrimitive;

        public NonPrimitiveSerializationMapping2()
        {

        }

        public override SerializedData Save( object obj, ISaver s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, ILoader l )
        {
            if( OnInstantiateTemp != null )
                temp = OnInstantiateTemp( data, l );

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
                OnLoad.Invoke( this, ref obj2, data, l );
                obj = obj2;
            }
        }

        public override void LoadReferences( ref object obj, SerializedData data, ILoader l )
        {
            if( OnLoadReferences != null )
            {
                // obj can be null here, this is normal.
                var obj2 = (TSource)obj;
                OnLoadReferences.Invoke( this, ref obj2, data, l );
                obj = obj2;
            }
        }

        public override SerializationMapping GetWorkingInstance()
        {
            return new NonPrimitiveSerializationMapping2<TTemp, TSource>()
            {
                OnSave = OnSave,
                OnInstantiate = OnInstantiate,
                OnInstantiateTemp = OnInstantiateTemp,
                OnLoad = OnLoad,
                OnLoadReferences = OnLoadReferences,
                context = context,
                temp = null
            };
        }
    }
}