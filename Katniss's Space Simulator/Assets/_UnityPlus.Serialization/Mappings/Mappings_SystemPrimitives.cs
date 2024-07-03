using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_SystemPrimitives
    {
        [SerializationMappingProvider( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new PrimitiveStructSerializationMapping<bool>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( byte ) )]
        public static SerializationMapping ByteMapping()
        {
            return new PrimitiveStructSerializationMapping<byte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (byte)data
            };
        }

        [SerializationMappingProvider( typeof( sbyte ) )]
        public static SerializationMapping SByteMapping()
        {
            return new PrimitiveStructSerializationMapping<sbyte>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (sbyte)data
            };
        }

        [SerializationMappingProvider( typeof( short ) )]
        public static SerializationMapping Int16Mapping()
        {
            return new PrimitiveStructSerializationMapping<short>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (short)data
            };
        }

        [SerializationMappingProvider( typeof( ushort ) )]
        public static SerializationMapping UInt16Mapping()
        {
            return new PrimitiveStructSerializationMapping<ushort>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ushort)data
            };
        }

        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new PrimitiveStructSerializationMapping<int>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (int)data
            };
        }

        [SerializationMappingProvider( typeof( uint ) )]
        public static SerializationMapping UInt32Mapping()
        {
            return new PrimitiveStructSerializationMapping<uint>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (uint)data
            };
        }

        [SerializationMappingProvider( typeof( long ) )]
        public static SerializationMapping Int64Mapping()
        {
            return new PrimitiveStructSerializationMapping<long>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (long)data
            };
        }

        [SerializationMappingProvider( typeof( ulong ) )]
        public static SerializationMapping UInt64Mapping()
        {
            return new PrimitiveStructSerializationMapping<ulong>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (ulong)data
            };
        }

        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new PrimitiveStructSerializationMapping<float>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (float)data
            };
        }

        [SerializationMappingProvider( typeof( double ) )]
        public static SerializationMapping DoubleMapping()
        {
            return new PrimitiveStructSerializationMapping<double>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (double)data
            };
        }

        [SerializationMappingProvider( typeof( decimal ) )]
        public static SerializationMapping DecimalMapping()
        {
            return new PrimitiveStructSerializationMapping<decimal>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (decimal)data
            };
        }

        [SerializationMappingProvider( typeof( char ) )]
        public static SerializationMapping CharMapping()
        {
            return new PrimitiveStructSerializationMapping<char>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)(o.ToString()),
                OnInstantiate = ( data, l ) => ((string)data)[0]
            };
        }

        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new PrimitiveStructSerializationMapping<string>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o,
                OnInstantiate = ( data, l ) => (string)data
            };
        }

        [SerializationMappingProvider( typeof( DateTime ) )]
        public static SerializationMapping DateTimeMapping()
        {
            // DateTime is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602Z`

            return new PrimitiveStructSerializationMapping<DateTime>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTime.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [SerializationMappingProvider( typeof( DateTimeOffset ) )]
        public static SerializationMapping DateTimeOffsetMapping()
        {
            // DateTimeOffset is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602+00:00`

            return new PrimitiveStructSerializationMapping<DateTimeOffset>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "o", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => DateTimeOffset.Parse( (string)data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
            };
        }

        [SerializationMappingProvider( typeof( TimeSpan ) )]
        public static SerializationMapping TimeSpanMapping()
        {
            // TimeSpan is saved as `[-][dd'.']hh':'mm':'ss['.'fffffff]`.
            // `-3962086.01:03:44.2452523`

            return new PrimitiveStructSerializationMapping<TimeSpan>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "c", CultureInfo.InvariantCulture ),
                OnInstantiate = ( data, l ) => TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture )
            };
        }

        [SerializationMappingProvider( typeof( Enum ) )]
        public static SerializationMapping EnumMapping<T>() where T : struct, Enum
        {
            return new PrimitiveStructSerializationMapping<T>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString( "G" ),
                OnInstantiate = ( data, l ) => Enum.Parse<T>( (string)data )
            };
        }

        [SerializationMappingProvider( typeof( Delegate ) )]
        public static SerializationMapping DelegateMapping()
        {
            return new PrimitiveStructSerializationMapping<Delegate>()
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