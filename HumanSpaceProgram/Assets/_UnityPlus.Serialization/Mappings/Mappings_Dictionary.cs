using System.Collections.Generic;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Dictionary
    {
        [MapsInheritingFrom( typeof( KeyValuePair<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping KeyValuePair_ValueToValue_Mapping<TKey, TValue>()
        {
            return new MemberwiseSerializationMapping<KeyValuePair<TKey, TValue>>()
                .WithReadonlyMember( "key", o => o.Key )
                .WithReadonlyMember( "value", o => o.Value )
                .WithFactory<TKey, TValue>( ( key, value ) => new KeyValuePair<TKey, TValue>( key, value ) );
        }

        [MapsInheritingFrom( typeof( KeyValuePair<,> ), Context = KeyValueContext.RefToValue )]
        public static SerializationMapping KeyValuePair_RefToValue_Mapping<TKey, TValue>()
        {
            return new MemberwiseSerializationMapping<KeyValuePair<TKey, TValue>>()
                .WithReadonlyMember( "key", ObjectContext.Ref, o => o.Key )
                .WithReadonlyMember( "value", o => o.Value )
                .WithFactory<TKey, TValue>( ( key, value ) => new KeyValuePair<TKey, TValue>( key, value ) );
        }

        [MapsInheritingFrom( typeof( KeyValuePair<,> ), Context = KeyValueContext.ValueToRef )]
        public static SerializationMapping KeyValuePair_ValueToRef_Mapping<TKey, TValue>()
        {
            return new MemberwiseSerializationMapping<KeyValuePair<TKey, TValue>>()
                .WithReadonlyMember( "key", o => o.Key )
                .WithReadonlyMember( "value", ObjectContext.Ref, o => o.Value )
                .WithFactory<TKey, TValue>( ( key, value ) => new KeyValuePair<TKey, TValue>( key, value ) );
        }

        [MapsInheritingFrom( typeof( KeyValuePair<,> ), Context = KeyValueContext.RefToRef )]
        public static SerializationMapping KeyValuePair_RefToRef_Mapping<TKey, TValue>()
        {
            return new MemberwiseSerializationMapping<KeyValuePair<TKey, TValue>>()
                .WithReadonlyMember( "key", ObjectContext.Ref, o => o.Key )
                .WithReadonlyMember( "value", ObjectContext.Ref, o => o.Value )
                .WithFactory<TKey, TValue>( ( key, value ) => new KeyValuePair<TKey, TValue>( key, value ) );
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ), Context = KeyValueContext.ValueToValue )]
        public static SerializationMapping Dictionary_ValueToValue_Mapping<TKey, TValue>()
        {
            return new EnumeratedSerializationMapping<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(
                KeyValueContext.ValueToValue,
                ( o, i, oElem ) =>
                {
                    o[oElem.Key] = oElem.Value;
                } )
                .WithFactory( ( int count ) => new Dictionary<TKey, TValue>( count ) );
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ), Context = KeyValueContext.RefToValue )]
        public static SerializationMapping Dictionary_RefToValue_Mapping<TKey, TValue>()
        {
            return new EnumeratedSerializationMapping<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(
                KeyValueContext.RefToValue,
                ( o, i, oElem ) =>
                {
                    o[oElem.Key] = oElem.Value;
                } )
                .WithFactory( ( int count ) => new Dictionary<TKey, TValue>( count ) );
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ), Context = KeyValueContext.ValueToRef )]
        public static SerializationMapping Dictionary_ValueToRef_Mapping<TKey, TValue>()
        {
            return new EnumeratedSerializationMapping<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(
                KeyValueContext.ValueToRef,
                ( o, i, oElem ) =>
                {
                    o[oElem.Key] = oElem.Value;
                } )
                .WithFactory( ( int count ) => new Dictionary<TKey, TValue>( count ) );
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ), Context = KeyValueContext.RefToRef )]
        public static SerializationMapping Dictionary_RefToRef_Mapping<TKey, TValue>()
        {
#warning TODO - would be nice if the element context could be specified as 'pass through' from the mapping's main context.
            return new EnumeratedSerializationMapping<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(
                KeyValueContext.RefToRef,
                ( o, i, oElem ) =>
                {
                    o[oElem.Key] = oElem.Value;
                } )
                .WithFactory( ( int count ) => new Dictionary<TKey, TValue>( count ) );
        }
    }
}