using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_ValueTuple
    {
        [MapsInheritingFrom( typeof( ValueTuple<,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2>()
        {
            return new MemberwiseSerializationMapping<(T1, T2)>()
            {
                ("1", new Member<(T1, T2), T1>( o => o.Item1 )),
                ("2", new Member<(T1, T2), T2>( o => o.Item2 ))
            };
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3)>()
            {
                ("1", new Member<(T1, T2, T3), T1>( o => o.Item1 )),
                ("2", new Member<(T1, T2, T3), T2>( o => o.Item2 )),
                ("3", new Member<(T1, T2, T3), T3>( o => o.Item3 ))
            };
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4)>()
            {
                ("1", new Member<(T1, T2, T3, T4), T1>(o => o.Item1)),
                ("2", new Member<(T1, T2, T3, T4), T2>(o => o.Item2)),
                ("3", new Member<(T1, T2, T3, T4), T3>(o => o.Item3)),
                ("4", new Member<(T1, T2, T3, T4), T4>(o => o.Item4))
            };
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5)>()
            {
                ("1", new Member<(T1, T2, T3, T4, T5), T1>(o => o.Item1)),
                ("2", new Member<(T1, T2, T3, T4, T5), T2>(o => o.Item2)),
                ("3", new Member<(T1, T2, T3, T4, T5), T3>(o => o.Item3)),
                ("4", new Member<(T1, T2, T3, T4, T5), T4>(o => o.Item4)),
                ("5", new Member<(T1, T2, T3, T4, T5), T5>(o => o.Item5))
            };
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5, T6>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5, T6)>()
            {
                ("1", new Member<(T1, T2, T3, T4, T5, T6), T1>(o => o.Item1)),
                ("2", new Member<(T1, T2, T3, T4, T5, T6), T2>(o => o.Item2)),
                ("3", new Member<(T1, T2, T3, T4, T5, T6), T3>(o => o.Item3)),
                ("4", new Member<(T1, T2, T3, T4, T5, T6), T4>(o => o.Item4)),
                ("5", new Member<(T1, T2, T3, T4, T5, T6), T5>(o => o.Item5)),
                ("6", new Member<(T1, T2, T3, T4, T5, T6), T6>(o => o.Item6))
            };
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5, T6, T7)>()
            {
                ("1", new Member<(T1, T2, T3, T4, T5, T6, T7), T1>(o => o.Item1)),
                ("2", new Member<(T1, T2, T3, T4, T5, T6, T7), T2>(o => o.Item2)),
                ("3", new Member<(T1, T2, T3, T4, T5, T6, T7), T3>(o => o.Item3)),
                ("4", new Member<(T1, T2, T3, T4, T5, T6, T7), T4>(o => o.Item4)),
                ("5", new Member<(T1, T2, T3, T4, T5, T6, T7), T5>(o => o.Item5)),
                ("6", new Member<(T1, T2, T3, T4, T5, T6, T7), T6>(o => o.Item6)),
                ("7", new Member<(T1, T2, T3, T4, T5, T6, T7), T7>(o => o.Item7))
            };
        }

        // This doesn't work, for some reason.
        /*[SerializationMappingProvider( typeof( ValueTuple<,,,,,,,> ) )]
        public static SerializationMapping ValueTupleMapping<T1, T2, T3, T4, T5, T6, T7, TRest>() where TRest : struct
        {
            return new MemberwiseSerializationMapping<(T1, T2, T3, T4, T5, T6, T7, TRest)>()
            {
                ("1", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T1>(o => o.Item1)),
                ("2", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T2>(o => o.Item2)),
                ("3", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T3>(o => o.Item3)),
                ("4", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T4>(o => o.Item4)),
                ("5", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T5>(o => o.Item5)),
                ("6", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T6>(o => o.Item6)),
                ("7", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), T7>(o => o.Item7)),
                ("rest", new Member<(T1, T2, T3, T4, T5, T6, T7, TRest), TRest>(o => o.Item8))
            };
        }*/
    }
}