using System;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// v4 Implementation of the main Serialization API.
    /// </summary>
    public static partial class SerializationUnit
    {
        // --- Serialize ---

        public static SerializedData Serialize<T>( T obj )
            => Serialize( ContextKey.Default, obj, null, null );

        public static SerializedData Serialize<T>( T obj, SerializationConfiguration config )
            => Serialize( ContextKey.Default, obj, null, config );

        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s )
            => Serialize( ContextKey.Default, obj, s, null );

        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s, SerializationConfiguration config )
            => Serialize( ContextKey.Default, obj, s, config );

        public static SerializedData Serialize<T>( Type contextType, T obj )
            => Serialize( ContextRegistry.GetID( contextType ), obj, null, null );

        public static SerializedData Serialize<T>( Type contextType, T obj, SerializationConfiguration config )
            => Serialize( ContextRegistry.GetID( contextType ), obj, null, config );

        public static SerializedData Serialize<T>( Type contextType, T obj, IReverseReferenceMap s )
            => Serialize( ContextRegistry.GetID( contextType ), obj, s, null );

        public static SerializedData Serialize<T>( Type contextType, T obj, IReverseReferenceMap s, SerializationConfiguration config )
            => Serialize( ContextRegistry.GetID( contextType ), obj, s, config );

        public static SerializedData Serialize<T>( ContextKey context, T obj )
            => Serialize( context, obj, null, null );

        public static SerializedData Serialize<T>( ContextKey context, T obj, SerializationConfiguration config )
            => Serialize( context, obj, null, config );

        public static SerializedData Serialize<T>( ContextKey context, T obj, IReverseReferenceMap s )
            => Serialize( context, obj, s, null );

        public static SerializedData Serialize<T>( ContextKey context, T obj, IReverseReferenceMap s, SerializationConfiguration config )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ReverseMap = s ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new SerializationStrategy() );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            return driver.Result as SerializedData;
        }

        // --- Deserialize ---

        public static T Deserialize<T>( SerializedData data )
            => Deserialize<T>( ContextKey.Default, data, null, null );

        public static T Deserialize<T>( SerializedData data, SerializationConfiguration config )
            => Deserialize<T>( ContextKey.Default, data, null, config );

        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l )
            => Deserialize<T>( ContextKey.Default, data, l, null );

        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l, SerializationConfiguration config )
            => Deserialize<T>( ContextKey.Default, data, l, config );

        public static T Deserialize<T>( Type contextType, SerializedData data )
            => Deserialize<T>( ContextRegistry.GetID( contextType ), data, null, null );

        public static T Deserialize<T>( Type contextType, SerializedData data, SerializationConfiguration config )
            => Deserialize<T>( ContextRegistry.GetID( contextType ), data, null, config );

        public static T Deserialize<T>( Type contextType, SerializedData data, IForwardReferenceMap l )
            => Deserialize<T>( ContextRegistry.GetID( contextType ), data, l, null );

        public static T Deserialize<T>( Type contextType, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config )
            => Deserialize<T>( ContextRegistry.GetID( contextType ), data, l, config );

        public static T Deserialize<T>( ContextKey context, SerializedData data )
            => Deserialize<T>( context, data, null, null );

        public static T Deserialize<T>( ContextKey context, SerializedData data, SerializationConfiguration config )
            => Deserialize<T>( context, data, null, config );

        public static T Deserialize<T>( ContextKey context, SerializedData data, IForwardReferenceMap l )
            => Deserialize<T>( context, data, l, null );

        public static T Deserialize<T>( ContextKey context, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( null, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            return (T)driver.Result;
        }

        // --- Populate (Class) ---

        public static void Populate<T>( T obj, SerializedData data ) where T : class
            => Populate( ContextKey.Default, obj, data, null, null );

        public static void Populate<T>( T obj, SerializedData data, SerializationConfiguration config ) where T : class
            => Populate( ContextKey.Default, obj, data, null, config );

        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l ) where T : class
            => Populate( ContextKey.Default, obj, data, l, null );

        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config ) where T : class
            => Populate( ContextKey.Default, obj, data, l, config );

        public static void Populate<T>( Type contextType, T obj, SerializedData data ) where T : class
            => Populate( ContextRegistry.GetID( contextType ), obj, data, null, null );

        public static void Populate<T>( Type contextType, T obj, SerializedData data, SerializationConfiguration config ) where T : class
            => Populate( ContextRegistry.GetID( contextType ), obj, data, null, config );

        public static void Populate<T>( Type contextType, T obj, SerializedData data, IForwardReferenceMap l ) where T : class
            => Populate( ContextRegistry.GetID( contextType ), obj, data, l, null );

        public static void Populate<T>( Type contextType, T obj, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config ) where T : class
            => Populate( ContextRegistry.GetID( contextType ), obj, data, l, config );

        public static void Populate<T>( ContextKey context, T obj, SerializedData data ) where T : class
            => Populate( context, obj, data, null, null );

        public static void Populate<T>( ContextKey context, T obj, SerializedData data, SerializationConfiguration config ) where T : class
            => Populate( context, obj, data, null, config );

        public static void Populate<T>( ContextKey context, T obj, SerializedData data, IForwardReferenceMap l ) where T : class
            => Populate( context, obj, data, l, null );

        public static void Populate<T>( ContextKey context, T obj, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config ) where T : class
        {
            if( obj == null ) throw new ArgumentNullException( nameof( obj ) );

            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }
        }

        // --- Populate (Struct) ---

        public static void Populate<T>( ref T obj, SerializedData data ) where T : struct
            => Populate( ContextKey.Default, ref obj, data, null, null );

        public static void Populate<T>( ref T obj, SerializedData data, SerializationConfiguration config ) where T : struct
            => Populate( ContextKey.Default, ref obj, data, null, config );

        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
            => Populate( ContextKey.Default, ref obj, data, l, null );

        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config ) where T : struct
            => Populate( ContextKey.Default, ref obj, data, l, config );

        public static void Populate<T>( Type contextType, ref T obj, SerializedData data ) where T : struct
            => Populate( ContextRegistry.GetID( contextType ), ref obj, data, null, null );

        public static void Populate<T>( Type contextType, ref T obj, SerializedData data, SerializationConfiguration config ) where T : struct
            => Populate( ContextRegistry.GetID( contextType ), ref obj, data, null, config );

        public static void Populate<T>( Type contextType, ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
            => Populate( ContextRegistry.GetID( contextType ), ref obj, data, l, null );

        public static void Populate<T>( Type contextType, ref T obj, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config ) where T : struct
            => Populate( ContextRegistry.GetID( contextType ), ref obj, data, l, config );

        public static void Populate<T>( ContextKey context, ref T obj, SerializedData data ) where T : struct
            => Populate( context, ref obj, data, null, null );

        public static void Populate<T>( ContextKey context, ref T obj, SerializedData data, SerializationConfiguration config ) where T : struct
            => Populate( context, ref obj, data, null, config );

        public static void Populate<T>( ContextKey context, ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
            => Populate( context, ref obj, data, l, null );

        public static void Populate<T>( ContextKey context, ref T obj, SerializedData data, IForwardReferenceMap l, SerializationConfiguration config ) where T : struct
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            obj = (T)driver.Result;
        }
    }
}