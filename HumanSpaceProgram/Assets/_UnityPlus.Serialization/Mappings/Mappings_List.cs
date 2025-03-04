using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_List
    {
        /*[MapsInheritingFrom( typeof( List<> ), Context = ArrayContext.Values )]
        public static SerializationMapping ListMapping<T>()
        {
            return new IndexedSerializationMapping<List<T>, T>( o => o.Count,
                ObjectContext.Value,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
#warning TODO - using Add doesn't guarantee same order if some elements fail and are added later.
        // could work if the list is filled with default elements up to capacity first.
                    if( o.Count <= i )
                        o.Add( oElem );
                    else
                        o[i] = oElem;
                } )
                .WithFactory( ( int count ) => new List<T>( count ) );
        }*/

        [MapsInheritingFrom( typeof( List<> ), Context = ArrayContext.Values )]
        public static SerializationMapping ListMapping<T>()
        {
            return new IndexedSerializationMapping<List<T>, T>( o => o.Count,
                ObjectContext.Value,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => { } )
                .WithFactory( ( IEnumerable<T> elements ) => elements == null ? new List<T>() : new List<T>( elements ) );
        }

        [MapsInheritingFrom( typeof( List<> ), Context = ArrayContext.Assets )]
        public static SerializationMapping ListAssetMapping<T>()
        {
            return new IndexedSerializationMapping<List<T>, T>( o => o.Count,
                ObjectContext.Asset,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => { } )
                .WithFactory( ( IEnumerable<T> elements ) => elements == null ? new List<T>() : new List<T>( elements ) );
        }

        [MapsInheritingFrom( typeof( List<> ), Context = ArrayContext.Refs )]
        public static SerializationMapping ListReferenceMapping<T>()
        {
            return new IndexedSerializationMapping<List<T>, T>( o => o.Count,
                ObjectContext.Ref,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => { } )
                .WithFactory( ( IEnumerable<T> elements ) => elements == null ? new List<T>() : new List<T>( elements ) );
        }
    }
}