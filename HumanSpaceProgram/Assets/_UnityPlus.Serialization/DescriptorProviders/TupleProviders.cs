using System;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    internal static class TupleProviders
    {
        [MapsInheritingFrom( typeof( ValueTuple<,> ) )]
        private static IDescriptor ProvideValueTuple2<T1, T2>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2> t, T2 v ) => t.Item2 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,> ) )]
        private static IDescriptor ProvideValueTuple3<T1, T2, T3>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3> t, T3 v ) => t.Item3 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,> ) )]
        private static IDescriptor ProvideValueTuple4<T1, T2, T3, T4>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4> t, T4 v ) => t.Item4 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,> ) )]
        private static IDescriptor ProvideValueTuple5<T1, T2, T3, T4, T5>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T5 v ) => t.Item5 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,> ) )]
        private static IDescriptor ProvideValueTuple6<T1, T2, T3, T4, T5, T6>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T6 v ) => t.Item6 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,> ) )]
        private static IDescriptor ProvideValueTuple7<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new MemberwiseDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T6 v ) => t.Item6 = v )
                .WithMember( "7", t => t.Item7, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T7 v ) => t.Item7 = v );
        }

#warning TODO - add system.tuple (immutable, needs readonly members + constructor)
    }
}