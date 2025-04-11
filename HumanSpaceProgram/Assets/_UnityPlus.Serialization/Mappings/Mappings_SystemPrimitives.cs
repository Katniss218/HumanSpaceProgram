using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_SystemPrimitives
    {
        [MapsInheritingFrom( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new PrimitiveSerializationMapping<bool>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (bool)data
            };
        }

        [MapsInheritingFrom( typeof( byte ) )]
        public static SerializationMapping ByteMapping()
        {
            return new PrimitiveSerializationMapping<byte>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (byte)data
            };
        }

        [MapsInheritingFrom( typeof( sbyte ) )]
        public static SerializationMapping SByteMapping()
        {
            return new PrimitiveSerializationMapping<sbyte>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (sbyte)data
            };
        }

        [MapsInheritingFrom( typeof( short ) )]
        public static SerializationMapping Int16Mapping()
        {
            return new PrimitiveSerializationMapping<short>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (short)data
            };
        }

        [MapsInheritingFrom( typeof( ushort ) )]
        public static SerializationMapping UInt16Mapping()
        {
            return new PrimitiveSerializationMapping<ushort>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (ushort)data
            };
        }

        [MapsInheritingFrom( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new PrimitiveSerializationMapping<int>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (int)data
            };
        }

        [MapsInheritingFrom( typeof( uint ) )]
        public static SerializationMapping UInt32Mapping()
        {
            return new PrimitiveSerializationMapping<uint>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (uint)data
            };
        }

        [MapsInheritingFrom( typeof( long ) )]
        public static SerializationMapping Int64Mapping()
        {
            return new PrimitiveSerializationMapping<long>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (long)data
            };
        }

        [MapsInheritingFrom( typeof( ulong ) )]
        public static SerializationMapping UInt64Mapping()
        {
            return new PrimitiveSerializationMapping<ulong>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (ulong)data
            };
        }

        [MapsInheritingFrom( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new PrimitiveSerializationMapping<float>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (float)data
            };
        }

        [MapsInheritingFrom( typeof( double ) )]
        public static SerializationMapping DoubleMapping()
        {
            return new PrimitiveSerializationMapping<double>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (double)data
            };
        }

        [MapsInheritingFrom( typeof( decimal ) )]
        public static SerializationMapping DecimalMapping()
        {
            return new PrimitiveSerializationMapping<decimal>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (decimal)data
            };
        }

        [MapsInheritingFrom( typeof( char ) )]
        public static SerializationMapping CharMapping()
        {
            return new PrimitiveSerializationMapping<char>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)(o.ToString()),
                OnLoad = ( data, l ) => ((string)data)[0]
            };
        }

        [MapsInheritingFrom( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new PrimitiveSerializationMapping<string>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnLoad = ( data, l ) => (string)data
            };
        }

        [MapsInheritingFrom( typeof( DateTime ) )]
        public static SerializationMapping DateTimeMapping()
        {
            // DateTime is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602Z`

            return new PrimitiveSerializationMapping<DateTime>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnLoad = ( data, l ) => DateTime.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [MapsInheritingFrom( typeof( DateTimeOffset ) )]
        public static SerializationMapping DateTimeOffsetMapping()
        {
            // DateTimeOffset is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602+00:00`

            return new PrimitiveSerializationMapping<DateTimeOffset>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnLoad = ( data, l ) => DateTimeOffset.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [MapsInheritingFrom( typeof( TimeSpan ) )]
        public static SerializationMapping TimeSpanMapping()
        {
            // TimeSpan is saved as `[-][dd'.']hh':'mm':'ss['.'fffffff]`.
            // `-3962086.01:03:44.2452523`

            return new PrimitiveSerializationMapping<TimeSpan>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "c", CultureInfo.InvariantCulture ),
                OnLoad = ( data, l ) => TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture )
            };
        }

        [MapsInheritingFrom( typeof( Enum ) )]
        public static SerializationMapping EnumMapping<T>() where T : struct, Enum
        {
            return new PrimitiveSerializationMapping<T>( skipHeader: ObjectHeaderSkipMode.WhenTypesMatch )
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "G" ),
                OnLoad = ( data, l ) => Enum.Parse<T>( (string)data )
            };
        }

        [MapsInheritingFrom( typeof( Nullable<> ) )]
        public static SerializationMapping NullableMapping<T>() where T : struct
        {
            return new NullableSerializationMapping<T>();
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
                OnLoad = ( SerializedData data, IForwardReferenceMap l ) =>
                {
                    // This is kinda non-standard, but since we need the reference to the `target` to even create the delegate, we can only create it here.
                    return Persistent_Delegate.ToDelegate( data, l );
                }
            };
        }
    }
}