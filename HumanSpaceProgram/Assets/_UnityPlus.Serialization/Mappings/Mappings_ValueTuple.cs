using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_ValueTuple
    {
        [MapsInheritingFrom( typeof( ValueTuple<,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2>()
        {
            return new MemberwiseSerializationMapping<(T1, T2)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 )
                .WithMember( "3", o => o.Item3 );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 )
                .WithMember( "3", o => o.Item3 )
                .WithMember( "4", o => o.Item4 );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 )
                .WithMember( "3", o => o.Item3 )
                .WithMember( "4", o => o.Item4 )
                .WithMember( "5", o => o.Item5 );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5, T6>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5, T6)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 )
                .WithMember( "3", o => o.Item3 )
                .WithMember( "4", o => o.Item4 )
                .WithMember( "5", o => o.Item5 )
                .WithMember( "6", o => o.Item6 );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5, T6, T7)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 )
                .WithMember( "3", o => o.Item3 )
                .WithMember( "4", o => o.Item4 )
                .WithMember( "5", o => o.Item5 )
                .WithMember( "6", o => o.Item6 )
                .WithMember( "7", o => o.Item7 );
        }

        // This doesn't work, for some reason.
        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5, T6, T7, TRest>() where TRest : struct
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5, T6, T7, TRest)>()
                .WithMember( "1", o => o.Item1 )
                .WithMember( "2", o => o.Item2 )
                .WithMember( "3", o => o.Item3 )
                .WithMember( "4", o => o.Item4 )
                .WithMember( "5", o => o.Item5 )
                .WithMember( "6", o => o.Item6 )
                .WithMember( "7", o => o.Item7 )
                .WithMember( "rest", o => o.Item8 );
        }
    }
}