using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can both be referenced by other objects, and contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class NonPrimitiveSerializationMapping<TSource> : SerializationMapping, IInstantiableSerializationMapping
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

        public NonPrimitiveSerializationMapping()
        {

        }

        protected override bool Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() )
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

            if( data != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() )
            {
                data = data["value"];
            }

            // obj can be null here, this is normal.
            TSource obj2 = (TSource)(object)obj;
            OnLoad.Invoke( ref obj2, data, l );
            obj = (TMember)(object)obj2;

            return true;
        }

        protected override bool TryLoad<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnInstantiate == null )
                return false;
            if( OnLoad == null )
                return false;

            if( data != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() )
            {
                data = data["value"];
            }

            // obj can be null here, this is normal.
            TSource obj2 = (TSource)OnInstantiate.Invoke( data, l );
            OnLoad.Invoke( ref obj2, data, l );
            obj = (TMember)(object)obj2;

            return true;
        }

        protected override bool TryLoadReferences<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnLoadReferences == null )
                return false;

            if( data != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() )
            {
                data = data["value"];
            }

            // obj can be null here, this is normal.
            var obj2 = (TSource)(object)obj;
            OnLoadReferences.Invoke( ref obj2, data, l );
            obj = (TMember)(object)obj2;

            return true;
        }
    }
}