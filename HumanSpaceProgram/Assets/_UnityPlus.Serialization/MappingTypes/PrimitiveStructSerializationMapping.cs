using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps an object that can't be referenced, but can contain references to other objects.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class PrimitiveStructSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public PrimitiveStructSerializationMapping()
        {

        }

        // TMember is the type of the variable that holds the mapped object. TSource is the type of the mapping (mapped object can be any type derived from that).

        protected override bool Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
#warning TODO - delegates should be able to be omitted from having the header as well.

            // Omit the header only for member types that are non-generic structs/sealed classes (non-generic and non-inheritable).
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

            // true && true

            return true;
        }

        protected override bool TryPopulate<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            return false;
        }

        protected override bool TryLoad<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            return false;
        }

        protected override bool TryLoadReferences<TMember>( ref TMember obj, SerializedData data, ILoader l )
        {
            if( OnInstantiate == null )
                return false;

            // Instantiating in LoadReferences means that every object that can be referenced should have already been added to the ILoader's RefMap.
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
    }
}