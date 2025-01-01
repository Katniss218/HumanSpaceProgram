using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_References
    {
        [MapsAnyInterface( Context = ObjectContext.Ref )]
        [MapsAnyClass( Context = ObjectContext.Ref )]
        public static SerializationMapping ObjectRefMapping<T>() where T : class
        {
            return new PrimitiveSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteObjectReference<T>( o ),
                OnInstantiate = ( data, l ) =>
                {
                    if( l.TryReadObjectReference<T>( data, out var obj ) )
                        return obj;
#warning TODO - handle this properly after the results are allowed.
                    throw new Exception( $"missing reference, try again" );
                }
            };
        }
    }
}