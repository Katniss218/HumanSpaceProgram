using System;
using System.Collections.Generic;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public static partial class SerializationUnit
    {
        // --- Serialize Many ---

        public static IEnumerable<SerializedData> SerializeMany<T>( IEnumerable<T> objects )
            => SerializeMany( ContextKey.Default, objects, null, null );

        public static IEnumerable<SerializedData> SerializeMany<T>( IEnumerable<T> objects, SerializationConfiguration config )
            => SerializeMany( ContextKey.Default, objects, null, config );

        public static IEnumerable<SerializedData> SerializeMany<T>( IEnumerable<T> objects, IReverseReferenceMap s )
            => SerializeMany( ContextKey.Default, objects, s, null );

        public static IEnumerable<SerializedData> SerializeMany<T>( IEnumerable<T> objects, IReverseReferenceMap s, SerializationConfiguration config )
            => SerializeMany( ContextKey.Default, objects, s, config );

        public static IEnumerable<SerializedData> SerializeMany<T>( Type contextType, IEnumerable<T> objects )
            => SerializeMany( ContextRegistry.GetID( contextType ), objects, null, null );

        public static IEnumerable<SerializedData> SerializeMany<T>( Type contextType, IEnumerable<T> objects, SerializationConfiguration config )
            => SerializeMany( ContextRegistry.GetID( contextType ), objects, null, config );

        public static IEnumerable<SerializedData> SerializeMany<T>( Type contextType, IEnumerable<T> objects, IReverseReferenceMap s )
            => SerializeMany( ContextRegistry.GetID( contextType ), objects, s, null );

        public static IEnumerable<SerializedData> SerializeMany<T>( Type contextType, IEnumerable<T> objects, IReverseReferenceMap s, SerializationConfiguration config )
            => SerializeMany( ContextRegistry.GetID( contextType ), objects, s, config );

        public static IEnumerable<SerializedData> SerializeMany<T>( ContextKey context, IEnumerable<T> objects )
            => SerializeMany( context, objects, null, null );

        public static IEnumerable<SerializedData> SerializeMany<T>( ContextKey context, IEnumerable<T> objects, SerializationConfiguration config )
            => SerializeMany( context, objects, null, config );

        public static IEnumerable<SerializedData> SerializeMany<T>( ContextKey context, IEnumerable<T> objects, IReverseReferenceMap s )
            => SerializeMany( context, objects, s, null );

        public static IEnumerable<SerializedData> SerializeMany<T>( ContextKey context, IEnumerable<T> objects, IReverseReferenceMap s, SerializationConfiguration config )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ReverseMap = s ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );

            foreach( var obj in objects )
            {
                driver.Initialize( typeof( T ), context, new SerializationStrategy(), obj, null );

                while( !driver.IsFinished )
                {
                    driver.Tick( float.PositiveInfinity );
                }

                yield return driver.Result as SerializedData;
            }
        }

        // --- Deserialize Many ---

        public static IEnumerable<T> DeserializeMany<T>( IEnumerable<SerializedData> data )
            => DeserializeMany<T>( ContextKey.Default, data, null, null );

        public static IEnumerable<T> DeserializeMany<T>( IEnumerable<SerializedData> data, SerializationConfiguration config )
            => DeserializeMany<T>( ContextKey.Default, data, null, config );

        public static IEnumerable<T> DeserializeMany<T>( IEnumerable<SerializedData> data, IForwardReferenceMap l )
            => DeserializeMany<T>( ContextKey.Default, data, l, null );

        public static IEnumerable<T> DeserializeMany<T>( IEnumerable<SerializedData> data, IForwardReferenceMap l, SerializationConfiguration config )
            => DeserializeMany<T>( ContextKey.Default, data, l, config );

        public static IEnumerable<T> DeserializeMany<T>( Type contextType, IEnumerable<SerializedData> data )
            => DeserializeMany<T>( ContextRegistry.GetID( contextType ), data, null, null );

        public static IEnumerable<T> DeserializeMany<T>( Type contextType, IEnumerable<SerializedData> data, SerializationConfiguration config )
            => DeserializeMany<T>( ContextRegistry.GetID( contextType ), data, null, config );

        public static IEnumerable<T> DeserializeMany<T>( Type contextType, IEnumerable<SerializedData> data, IForwardReferenceMap l )
            => DeserializeMany<T>( ContextRegistry.GetID( contextType ), data, l, null );

        public static IEnumerable<T> DeserializeMany<T>( Type contextType, IEnumerable<SerializedData> data, IForwardReferenceMap l, SerializationConfiguration config )
            => DeserializeMany<T>( ContextRegistry.GetID( contextType ), data, l, config );

        public static IEnumerable<T> DeserializeMany<T>( ContextKey context, IEnumerable<SerializedData> data )
            => DeserializeMany<T>( context, data, null, null );

        public static IEnumerable<T> DeserializeMany<T>( ContextKey context, IEnumerable<SerializedData> data, SerializationConfiguration config )
            => DeserializeMany<T>( context, data, null, config );

        public static IEnumerable<T> DeserializeMany<T>( ContextKey context, IEnumerable<SerializedData> data, IForwardReferenceMap l )
            => DeserializeMany<T>( context, data, l, null );

        public static IEnumerable<T> DeserializeMany<T>( ContextKey context, IEnumerable<SerializedData> data, IForwardReferenceMap l, SerializationConfiguration config )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );

            foreach( var d in data )
            {
                driver.Initialize( typeof( T ), context, new DeserializationStrategy(), null, d );

                while( !driver.IsFinished )
                {
                    driver.Tick( float.PositiveInfinity );
                }

                yield return (T)driver.Result;
            }
        }

        // --- Populate Many ---

        public static IEnumerable<T> PopulateMany<T>( IEnumerable<(T obj, SerializedData data)> objects )
            => PopulateMany( ContextKey.Default, objects, null, null );

        public static IEnumerable<T> PopulateMany<T>( IEnumerable<(T obj, SerializedData data)> objects, SerializationConfiguration config )
            => PopulateMany( ContextKey.Default, objects, null, config );

        public static IEnumerable<T> PopulateMany<T>( IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l )
            => PopulateMany( ContextKey.Default, objects, l, null );

        public static IEnumerable<T> PopulateMany<T>( IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, SerializationConfiguration config )
            => PopulateMany( ContextKey.Default, objects, l, config );

        public static IEnumerable<T> PopulateMany<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects )
            => PopulateMany( ContextRegistry.GetID( contextType ), objects, null, null );

        public static IEnumerable<T> PopulateMany<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, SerializationConfiguration config )
            => PopulateMany( ContextRegistry.GetID( contextType ), objects, null, config );

        public static IEnumerable<T> PopulateMany<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l )
            => PopulateMany( ContextRegistry.GetID( contextType ), objects, l, null );

        public static IEnumerable<T> PopulateMany<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, SerializationConfiguration config )
            => PopulateMany( ContextRegistry.GetID( contextType ), objects, l, config );

        public static IEnumerable<T> PopulateMany<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects )
            => PopulateMany( context, objects, null, null );

        public static IEnumerable<T> PopulateMany<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, SerializationConfiguration config )
            => PopulateMany( context, objects, null, config );

        public static IEnumerable<T> PopulateMany<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l )
            => PopulateMany( context, objects, l, null );

        public static IEnumerable<T> PopulateMany<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, SerializationConfiguration config )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );

            foreach( var (obj, data) in objects )
            {
                driver.Initialize( typeof( T ), context, new DeserializationStrategy(), obj, data );

                while( !driver.IsFinished )
                {
                    driver.Tick( float.PositiveInfinity );
                }

                yield return (T)driver.Result;
            }
        }
    }
}
