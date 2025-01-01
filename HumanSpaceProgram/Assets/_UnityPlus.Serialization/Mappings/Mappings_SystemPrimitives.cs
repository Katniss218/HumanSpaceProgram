using System;
using System.Globalization;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_SystemPrimitives
    {
        [MapsInheritingFrom( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new PrimitiveSerializationMapping<bool>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (bool)data
            };
        }

        [MapsInheritingFrom( typeof( byte ) )]
        public static SerializationMapping ByteMapping()
        {
            return new PrimitiveSerializationMapping<byte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (byte)data
            };
        }

        [MapsInheritingFrom( typeof( sbyte ) )]
        public static SerializationMapping SByteMapping()
        {
            return new PrimitiveSerializationMapping<sbyte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (sbyte)data
            };
        }

        [MapsInheritingFrom( typeof( short ) )]
        public static SerializationMapping Int16Mapping()
        {
            return new PrimitiveSerializationMapping<short>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (short)data
            };
        }

        [MapsInheritingFrom( typeof( ushort ) )]
        public static SerializationMapping UInt16Mapping()
        {
            return new PrimitiveSerializationMapping<ushort>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ushort)data
            };
        }

        [MapsInheritingFrom( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new PrimitiveSerializationMapping<int>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (int)data
            };
        }

        [MapsInheritingFrom( typeof( uint ) )]
        public static SerializationMapping UInt32Mapping()
        {
            return new PrimitiveSerializationMapping<uint>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (uint)data
            };
        }

        [MapsInheritingFrom( typeof( long ) )]
        public static SerializationMapping Int64Mapping()
        {
            return new PrimitiveSerializationMapping<long>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (long)data
            };
        }

        [MapsInheritingFrom( typeof( ulong ) )]
        public static SerializationMapping UInt64Mapping()
        {
            return new PrimitiveSerializationMapping<ulong>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ulong)data
            };
        }

        [MapsInheritingFrom( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new PrimitiveSerializationMapping<float>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (float)data
            };
        }

        [MapsInheritingFrom( typeof( double ) )]
        public static SerializationMapping DoubleMapping()
        {
            return new PrimitiveSerializationMapping<double>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (double)data
            };
        }

        [MapsInheritingFrom( typeof( decimal ) )]
        public static SerializationMapping DecimalMapping()
        {
            return new PrimitiveSerializationMapping<decimal>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (decimal)data
            };
        }

        [MapsInheritingFrom( typeof( char ) )]
        public static SerializationMapping CharMapping()
        {
            return new PrimitiveSerializationMapping<char>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)(o.ToString()),
                OnInstantiate = ( data, l ) => ((string)data)[0]
            };
        }

        [MapsInheritingFrom( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new PrimitiveSerializationMapping<string>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (string)data
            };
        }

        [MapsInheritingFrom( typeof( DateTime ) )]
        public static SerializationMapping DateTimeMapping()
        {
            // DateTime is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602Z`

            return new PrimitiveSerializationMapping<DateTime>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTime.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [MapsInheritingFrom( typeof( DateTimeOffset ) )]
        public static SerializationMapping DateTimeOffsetMapping()
        {
            // DateTimeOffset is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602+00:00`

            return new PrimitiveSerializationMapping<DateTimeOffset>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTimeOffset.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [MapsInheritingFrom( typeof( TimeSpan ) )]
        public static SerializationMapping TimeSpanMapping()
        {
            // TimeSpan is saved as `[-][dd'.']hh':'mm':'ss['.'fffffff]`.
            // `-3962086.01:03:44.2452523`

            return new PrimitiveSerializationMapping<TimeSpan>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "c", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture )
            };
        }

        [MapsInheritingFrom( typeof( Enum ) )]
        public static SerializationMapping EnumMapping<T>() where T : struct, Enum
        {
            return new PrimitiveSerializationMapping<T>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "G" ),
                OnInstantiate = ( data, l ) => Enum.Parse<T>( (string)data )
            };
        }

        [MapsInheritingFrom( typeof( Delegate ) )]
        public static SerializationMapping DelegateMapping()
        {
            return new PrimitiveSerializationMapping<Delegate>()
            {
                OnSave = ( o, s ) =>
                {
                    return Persistent_Delegate.GetData( o, s.RefMap );
                },
                OnInstantiate = ( SerializedData data, IForwardReferenceMap l ) =>
                {
                    // This is kinda non-standard, but since we need the reference to the `target` to even create the delegate, we can only create it here.
                    return Persistent_Delegate.ToDelegate( data, l );
                }
            };
        }
    }
}