using System;
using System.Collections.Generic;
using System.Globalization;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    /// <summary>
    /// Provides built-in descriptors for standard .NET types via the Provider system.
    /// This allows these types to be overridden by user providers if registered in a specific context.
    /// </summary>
    internal static class BuiltInDescriptorProviders
    {
        [MapsInheritingFrom( typeof( char ) )]
        private static IDescriptor ProvideChar() => new PrimitiveConfigurableDescriptor<char>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)(v.ToString()),
            ( d, c ) => { string strData = (string)d; return string.IsNullOrEmpty( strData ) ? '\0' : strData[0]; }
        );

        [MapsInheritingFrom( typeof( string ) )]
        private static IDescriptor ProvideString() => new PrimitiveConfigurableDescriptor<string>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (string)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( bool ) )]
        private static IDescriptor ProvideBool() => new PrimitiveConfigurableDescriptor<bool>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (bool)(SerializedPrimitive)d );

        // --- Numeric Types (Explicit) ---

        [MapsInheritingFrom( typeof( byte ) )]
        private static IDescriptor ProvideByte() => new PrimitiveConfigurableDescriptor<byte>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (byte)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( sbyte ) )]
        private static IDescriptor ProvideSByte() => new PrimitiveConfigurableDescriptor<sbyte>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (sbyte)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( short ) )]
        private static IDescriptor ProvideInt16() => new PrimitiveConfigurableDescriptor<short>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (short)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( ushort ) )]
        private static IDescriptor ProvideUInt16() => new PrimitiveConfigurableDescriptor<ushort>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (ushort)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( int ) )]
        private static IDescriptor ProvideInt32() => new PrimitiveConfigurableDescriptor<int>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (int)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( uint ) )]
        private static IDescriptor ProvideUInt32() => new PrimitiveConfigurableDescriptor<uint>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (uint)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( long ) )]
        private static IDescriptor ProvideInt64() => new PrimitiveConfigurableDescriptor<long>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (long)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( ulong ) )]
        private static IDescriptor ProvideUInt64() => new PrimitiveConfigurableDescriptor<ulong>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (ulong)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( float ) )]
        private static IDescriptor ProvideSingle() => new PrimitiveConfigurableDescriptor<float>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (float)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( double ) )]
        private static IDescriptor ProvideDouble() => new PrimitiveConfigurableDescriptor<double>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (double)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( decimal ) )]
        private static IDescriptor ProvideDecimal() => new PrimitiveConfigurableDescriptor<decimal>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (decimal)(SerializedPrimitive)d );

        // --- Extended System Types ---

        [MapsInheritingFrom( typeof( Guid ) )]
        private static IDescriptor ProvideGuid() => new PrimitiveConfigurableDescriptor<Guid>(
            ( v, w, c ) => w.Data = v.SerializeGuid(),
            ( d, c ) => d.DeserializeGuid()
        );

        [MapsInheritingFrom( typeof( Type ) )]
        private static IDescriptor ProvideType() => new PrimitiveConfigurableDescriptor<Type>(
            ( v, w, c ) => w.Data = v.SerializeType(),
            ( d, c ) => d.DeserializeType()
        );

        [MapsInheritingFrom( typeof( DateTime ) )]
        private static IDescriptor ProvideDateTime() => new PrimitiveConfigurableDescriptor<DateTime>(
            // DateTime is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602Z`
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "o", CultureInfo.InvariantCulture ),
            ( d, c ) => DateTime.Parse( (string)d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
        );

        [MapsInheritingFrom( typeof( DateTimeOffset ) )]
        private static IDescriptor ProvideDateTimeOffset() => new PrimitiveConfigurableDescriptor<DateTimeOffset>(
            // DateTimeOffset is saved as an ISO-8601 string.
            // `2024-06-08T11:57:10.1564602+00:00`

            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "o", CultureInfo.InvariantCulture ),
            ( d, c ) => DateTimeOffset.Parse( (string)d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
        );

        [MapsInheritingFrom( typeof( TimeSpan ) )]
        private static IDescriptor ProvideTimeSpan() => new PrimitiveConfigurableDescriptor<TimeSpan>(
            // TimeSpan is saved as `[-][d'.']hh':'mm':'ss['.'fffffff]`.
            // `-3962086.01:03:44.2452523`

            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "c", CultureInfo.InvariantCulture ),
            ( d, c ) => TimeSpan.ParseExact( (string)d, "c", CultureInfo.InvariantCulture )
        );

        [MapsInheritingFrom( typeof( Index ) )]
        private static IDescriptor ProvideIndex() => new PrimitiveConfigurableDescriptor<Index>(
            ( v, w, c ) =>
            {
                int encoded = v.IsFromEnd ? -(v.Value + 1) : v.Value;
                w.Data = (SerializedPrimitive)encoded;
            },
            ( d, c ) =>
            {
                int encoded = (int)(SerializedPrimitive)d;
                return encoded < 0
                    ? new Index( -encoded - 1, fromEnd: true )
                    : new Index( encoded, fromEnd: false );
            }
        );

        [MapsInheritingFrom( typeof( Range ) )]
        private static IDescriptor ProvideRange() => new PrimitiveConfigurableDescriptor<Range>(
            ( v, w, c ) =>
            {
                int start = v.Start.IsFromEnd ? -(v.Start.Value + 1) : v.Start.Value;
                int end = v.End.IsFromEnd ? -(v.End.Value + 1) : v.End.Value;

                w.Data = new SerializedArray( (SerializedPrimitive)start, (SerializedPrimitive)end );
            },
            ( d, c ) =>
            {
                SerializedArray arr = (SerializedArray)d;

                int start = (int)(SerializedPrimitive)arr[0];
                int end = (int)(SerializedPrimitive)arr[1];

                Index s = start < 0 ? new Index( -start - 1, true ) : new Index( start, false );
                Index e = end < 0 ? new Index( -end - 1, true ) : new Index( end, false );

                return new Range( s, e );
            }
        );

        [MapsInheritingFrom( typeof( KeyValuePair<,> ) )]
        private static IDescriptor ProvideKeyValuePair<TKey, TValue>( ContextKey context )
        {
            IContextSelector selector = ContextRegistry.GetSelector( context );
            ContextKey keyContext = selector.Select( new ContextSelectionArgs( 0, typeof( TKey ), typeof( TKey ), 2 ) );
            ContextKey valueContext = selector.Select( new ContextSelectionArgs( 1, typeof( TValue ), typeof( TValue ), 2 ) );

            return new MemberwiseDescriptor<KeyValuePair<TKey, TValue>>()
                .WithConstructor(
                    args => new KeyValuePair<TKey, TValue>( args[0] != null ? (TKey)args[0] : default, args[1] != null ? (TValue)args[1] : default ),
                    ("key", typeof( TKey )),
                    ("value", typeof( TValue ))
                )
                .WithReadonlyMember( "key", keyContext, kvp => kvp.Key )
                .WithReadonlyMember( "value", valueContext, kvp => kvp.Value );
        }


        // --- Inner Types ---

        [MapsInheritingFrom( typeof( Delegate ) )]
        private static IDescriptor ProvideDelegate() => new PrimitiveConfigurableDescriptor<Delegate>(
            ( v, w, c ) =>
            {
                var data = Persistent_Delegate.GetData( v, c.ReverseMap );
                w.Data = data;
            },
            ( d, c ) =>
            {
                if( d is SerializedObject obj && obj.TryGetValue( KeyNames.VALUE, out var val ) )
                    d = val;
                return Persistent_Delegate.ToDelegate( d, c.ForwardMap );
            }
        );

        [MapsInheritingFrom( typeof( Enum ) )]
        private static IDescriptor ProvideEnum<T>() where T : struct, Enum
        {
            return new EnumDescriptor<T>();
        }

        [MapsAnyInterface( ContextType = typeof( Ctx.Ref ) )]
        [MapsAnyClass( ContextType = typeof( Ctx.Ref ) )]
        private static IDescriptor ProvideReference<T>( ContextKey _, Type targetType ) where T : class
        {
            // if the targetType is itself generic (and has only 1 generic type param that matches our params), then the type T that is passed is only going to have that 1 type parameter.
            // example: targetType = MyClass<int>
            // then T = int. and this breaks, we provide the reference to an Int.
            return (IDescriptor)Activator.CreateInstance( typeof( ReferenceDescriptor<> ).MakeGenericType( targetType ) );
            // return new ReferenceDescriptor<T>();
        }
    }
}