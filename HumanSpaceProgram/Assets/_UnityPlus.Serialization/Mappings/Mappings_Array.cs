using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Array
    {
        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Values )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new IndexedSerializationMapping<T[], T>( o => o.Length,
                ObjectContext.Value,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
                    o[i] = oElem;
                } )
                .WithFactory( ( int count ) => new T[count] );
        }

        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Assets )]
        public static SerializationMapping ArrayAssetMapping<T>() where T : class
        {
            return new IndexedSerializationMapping<T[], T>( o => o.Length,
                ObjectContext.Asset,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
                    o[i] = oElem;
                } )
                .WithFactory( length => new T[length] );
        }

        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Refs )]
        public static SerializationMapping ArrayReferenceMapping<T>() where T : class
        {
            return new IndexedSerializationMapping<T[], T>( o => o.Length,
                ObjectContext.Ref,
                ( o, i ) => // writes to data
                {
                    return o[i];
                },
                ( o, i, oElem ) => // loads from data
                {
                    o[i] = oElem;
                } )
                .WithFactory( length => new T[length] );
        }
    }
}