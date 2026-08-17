using System;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    internal static class TupleProviders
    {
        [MapsInheritingFrom( typeof( ValueTuple<,> ) )]
        private static IDescriptor ProvideValueTuple2<T1, T2>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey c1 = selector.Select( new ContextSelectionArgs( 0, typeof( T1 ), typeof( T1 ), 2 ) );
            ContextKey c2 = selector.Select( new ContextSelectionArgs( 1, typeof( T2 ), typeof( T2 ), 2 ) );

            return new MemberwiseDescriptor<ValueTuple<T1, T2>>()
                .WithMember( "1", c1, t => t.Item1, ( ref ValueTuple<T1, T2> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", c2, t => t.Item2, ( ref ValueTuple<T1, T2> t, T2 v ) => t.Item2 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,> ) )]
        private static IDescriptor ProvideValueTuple3<T1, T2, T3>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey c1 = selector.Select( new ContextSelectionArgs( 0, typeof( T1 ), typeof( T1 ), 3 ) );
            ContextKey c2 = selector.Select( new ContextSelectionArgs( 1, typeof( T2 ), typeof( T2 ), 3 ) );
            ContextKey c3 = selector.Select( new ContextSelectionArgs( 2, typeof( T3 ), typeof( T3 ), 3 ) );

            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3>>()
                .WithMember( "1", c1, t => t.Item1, ( ref ValueTuple<T1, T2, T3> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", c2, t => t.Item2, ( ref ValueTuple<T1, T2, T3> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", c3, t => t.Item3, ( ref ValueTuple<T1, T2, T3> t, T3 v ) => t.Item3 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,> ) )]
        private static IDescriptor ProvideValueTuple4<T1, T2, T3, T4>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey c1 = selector.Select( new ContextSelectionArgs( 0, typeof( T1 ), typeof( T1 ), 4 ) );
            ContextKey c2 = selector.Select( new ContextSelectionArgs( 1, typeof( T2 ), typeof( T2 ), 4 ) );
            ContextKey c3 = selector.Select( new ContextSelectionArgs( 2, typeof( T3 ), typeof( T3 ), 4 ) );
            ContextKey c4 = selector.Select( new ContextSelectionArgs( 3, typeof( T4 ), typeof( T4 ), 4 ) );

            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4>>()
                .WithMember( "1", c1, t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", c2, t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", c3, t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", c4, t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4> t, T4 v ) => t.Item4 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,> ) )]
        private static IDescriptor ProvideValueTuple5<T1, T2, T3, T4, T5>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey c1 = selector.Select( new ContextSelectionArgs( 0, typeof( T1 ), typeof( T1 ), 5 ) );
            ContextKey c2 = selector.Select( new ContextSelectionArgs( 1, typeof( T2 ), typeof( T2 ), 5 ) );
            ContextKey c3 = selector.Select( new ContextSelectionArgs( 2, typeof( T3 ), typeof( T3 ), 5 ) );
            ContextKey c4 = selector.Select( new ContextSelectionArgs( 3, typeof( T4 ), typeof( T4 ), 5 ) );
            ContextKey c5 = selector.Select( new ContextSelectionArgs( 4, typeof( T5 ), typeof( T5 ), 5 ) );

            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5>>()
                .WithMember( "1", c1, t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", c2, t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", c3, t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", c4, t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", c5, t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T5 v ) => t.Item5 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,> ) )]
        private static IDescriptor ProvideValueTuple6<T1, T2, T3, T4, T5, T6>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey c1 = selector.Select( new ContextSelectionArgs( 0, typeof( T1 ), typeof( T1 ), 6 ) );
            ContextKey c2 = selector.Select( new ContextSelectionArgs( 1, typeof( T2 ), typeof( T2 ), 6 ) );
            ContextKey c3 = selector.Select( new ContextSelectionArgs( 2, typeof( T3 ), typeof( T3 ), 6 ) );
            ContextKey c4 = selector.Select( new ContextSelectionArgs( 3, typeof( T4 ), typeof( T4 ), 6 ) );
            ContextKey c5 = selector.Select( new ContextSelectionArgs( 4, typeof( T5 ), typeof( T5 ), 6 ) );
            ContextKey c6 = selector.Select( new ContextSelectionArgs( 5, typeof( T6 ), typeof( T6 ), 6 ) );

            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6>>()
                .WithMember( "1", c1, t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", c2, t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", c3, t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", c4, t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", c5, t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", c6, t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T6 v ) => t.Item6 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,> ) )]
        private static IDescriptor ProvideValueTuple7<T1, T2, T3, T4, T5, T6, T7>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey c1 = selector.Select( new ContextSelectionArgs( 0, typeof( T1 ), typeof( T1 ), 7 ) );
            ContextKey c2 = selector.Select( new ContextSelectionArgs( 1, typeof( T2 ), typeof( T2 ), 7 ) );
            ContextKey c3 = selector.Select( new ContextSelectionArgs( 2, typeof( T3 ), typeof( T3 ), 7 ) );
            ContextKey c4 = selector.Select( new ContextSelectionArgs( 3, typeof( T4 ), typeof( T4 ), 7 ) );
            ContextKey c5 = selector.Select( new ContextSelectionArgs( 4, typeof( T5 ), typeof( T5 ), 7 ) );
            ContextKey c6 = selector.Select( new ContextSelectionArgs( 5, typeof( T6 ), typeof( T6 ), 7 ) );
            ContextKey c7 = selector.Select( new ContextSelectionArgs( 6, typeof( T7 ), typeof( T7 ), 7 ) );

            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>()
                .WithMember( "1", c1, t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", c2, t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", c3, t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", c4, t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", c5, t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", c6, t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T6 v ) => t.Item6 = v )
                .WithMember( "7", c7, t => t.Item7, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T7 v ) => t.Item7 = v );
        }

#warning TODO - add system.tuple (immutable, needs readonly members + constructor)
    }
}