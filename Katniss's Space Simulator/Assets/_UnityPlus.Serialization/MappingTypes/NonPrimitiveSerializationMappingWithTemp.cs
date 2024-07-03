using System;

namespace UnityPlus.Serialization
{
    public delegate void LoadAction2<TTemp, TSource>( NonPrimitiveSerializationMappingWithTemp<TTemp, TSource> mapping, ref TSource obj, SerializedData data, ILoader l ) where TTemp : class;
    public delegate void LoadReferencesAction2<TTemp, TSource>( NonPrimitiveSerializationMappingWithTemp<TTemp, TSource> mapping, ref TSource obj, SerializedData data, ILoader l ) where TTemp : class;

    /// <summary>
    /// Maps an object that can both be referenced by other objects, and contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class NonPrimitiveSerializationMappingWithTemp<TTemp, TSource> : SerializationMapping, IInstantiableSerializationMapping where TTemp : class
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

        public NonPrimitiveSerializationMappingWithTemp()
        {

        }

        protected override bool Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj != null && !((typeof( TMember ).IsValueType || typeof( TMember ).IsSealed) && !typeof( TMember ).IsGenericType) )
            {
                data = new SerializedObject();
                data[KeyNames.ID] = s.RefMap.GetID( obj ).SerializeGuid(); // doesn't make sense for structs.
                data[KeyNames.TYPE] = obj.GetType().SerializeType();

                data["value"] = OnSave.Invoke( (TSource)(object)obj, s );
            }
            else
            {
                data = OnSave.Invoke( (TSource)(object)obj, s );
            }

            return true;
        }

        protected override bool TryPopulate<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnLoad == null )
                return false;

            if( OnInstantiateTemp != null )
                temp = OnInstantiateTemp.Invoke( data, l );

            // obj can be null here, this is normal.
            TSource obj2 = (TSource)(object)obj;
            OnLoad.Invoke( this, ref obj2, data, l );
            obj = (TMember)(object)obj2;

            return true;
        }

        protected override bool TryLoad<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnInstantiate == null )
                return false;
            if( OnLoad == null )
                return false;

            if( data != null && !((typeof( TMember ).IsValueType || typeof( TMember ).IsSealed) && !typeof( TMember ).IsGenericType) )
            {
                data = data["value"];
            }

            if( OnInstantiateTemp != null )
                temp = OnInstantiateTemp.Invoke( data, l );

            // obj can be null here, this is normal.
            TSource obj2 = (TSource)OnInstantiate.Invoke( data, l );
            OnLoad.Invoke( this, ref obj2, data, l );
            obj = (TMember)(object)obj2;

            return true;
        }

        protected override bool TryLoadReferences<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnLoadReferences == null )
                return false;

            if( data != null && !((typeof( TMember ).IsValueType || typeof( TMember ).IsSealed) && !typeof( TMember ).IsGenericType) )
            {
                data = data["value"];
            }

            // obj can be null here, this is normal.
            var obj2 = (TSource)(object)obj;
            OnLoadReferences.Invoke( this, ref obj2, data, l );
            obj = (TMember)(object)obj2;

            return true;
        }

        public override SerializationMapping GetInstance()
        {
            return new NonPrimitiveSerializationMappingWithTemp<TTemp, TSource>()
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