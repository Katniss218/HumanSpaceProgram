using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can be referenced by other objects, but can't contain references.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class PrimitiveObjectSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public PrimitiveObjectSerializationMapping()
        {

        }

        protected override bool Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() ) // This doesn't appear to slow the system down much at all when benchbarked.
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
            if( OnInstantiate == null )
                return false;

            // Instantiating in Load/Populate means that this object can be added to the ILoader's RefMap
            //   (and later referenced by other objects).
            if( data != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() ) // This doesn't appear to slow the system down much at all when benchbarked.
            {
                TSource obj2 = OnInstantiate.Invoke( data["value"], l.RefMap );
                obj = (TMember)(object)obj2;
            }
            else
            {
                TSource obj2 = OnInstantiate.Invoke( data, l.RefMap );
                obj = (TMember)(object)obj2;
            }
            return true;
        }

        protected override bool TryLoad<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnInstantiate == null )
                return false;

            // Instantiating in Load/Populate means that this object can be added to the ILoader's RefMap
            //   (and later referenced by other objects).
            if( data != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() ) // This doesn't appear to slow the system down much at all when benchbarked.
            {
                TSource obj2 = OnInstantiate.Invoke( data["value"], l.RefMap );
                obj = (TMember)(object)obj2;
            }
            else
            {
                TSource obj2 = OnInstantiate.Invoke( data, l.RefMap );
                obj = (TMember)(object)obj2;
            }
            return true;
        }

        protected override bool TryLoadReferences<T>( ref T obj, SerializedData data, ILoader l )
        {
            return false;
        }
    }
}