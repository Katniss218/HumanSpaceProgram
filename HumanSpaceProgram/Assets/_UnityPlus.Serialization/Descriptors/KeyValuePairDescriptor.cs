using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public static class KeyValuePairDescriptorProvider
    {
        [MapsInheritingFrom( typeof( KeyValuePair<,> ) )]
        public static IDescriptor GetDescriptor<TKey, TValue>( ContextKey context )
        {
            var selector = ContextRegistry.GetSelector( context );
            var keyContext = selector.Select( new ContextSelectionArgs( 0, typeof( TKey ), typeof( TKey ), 2 ) );
            var valueContext = selector.Select( new ContextSelectionArgs( 1, typeof( TValue ), typeof( TValue ), 2 ) );

            return new MemberwiseDescriptor<KeyValuePair<TKey, TValue>>()
                .WithConstructor(
                    args => new KeyValuePair<TKey, TValue>( args[0] != null ? (TKey)args[0] : default, args[1] != null ? (TValue)args[1] : default ),
                    ("key", typeof( TKey )),
                    ("value", typeof( TValue ))
                )
                .WithReadonlyMember( "key", keyContext, kvp => kvp.Key )
                .WithReadonlyMember( "value", valueContext, kvp => kvp.Value );
        }
    }
}