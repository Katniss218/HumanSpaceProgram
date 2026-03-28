using System;
using System.Collections.Generic;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public static partial class SerializationUnit
    {
        // --- Serialize Many Async ---

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( IEnumerable<T> objects, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextKey.Default, objects, null, null, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( IEnumerable<T> objects, SerializationConfiguration config, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextKey.Default, objects, null, config, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( IEnumerable<T> objects, IReverseReferenceMap s, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextKey.Default, objects, s, null, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( IEnumerable<T> objects, IReverseReferenceMap s, SerializationConfiguration config, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextKey.Default, objects, s, config, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( Type contextType, IEnumerable<T> objects, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextRegistry.GetID( contextType ), objects, null, null, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( Type contextType, IEnumerable<T> objects, SerializationConfiguration config, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextRegistry.GetID( contextType ), objects, null, config, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( Type contextType, IEnumerable<T> objects, IReverseReferenceMap s, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextRegistry.GetID( contextType ), objects, s, null, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( Type contextType, IEnumerable<T> objects, IReverseReferenceMap s, SerializationConfiguration config, float timeBudgetMs = 2f )
            => SerializeManyAsync( ContextRegistry.GetID( contextType ), objects, s, config, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( ContextKey context, IEnumerable<T> objects, float timeBudgetMs = 2f )
            => SerializeManyAsync( context, objects, null, null, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( ContextKey context, IEnumerable<T> objects, SerializationConfiguration config, float timeBudgetMs = 2f )
            => SerializeManyAsync( context, objects, null, config, timeBudgetMs );

        public static IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( ContextKey context, IEnumerable<T> objects, IReverseReferenceMap s, float timeBudgetMs = 2f )
            => SerializeManyAsync( context, objects, s, null, timeBudgetMs );

        public static async IAsyncEnumerable<SerializedData> SerializeManyAsync<T>( ContextKey context, IEnumerable<T> objects, IReverseReferenceMap s, SerializationConfiguration config, float timeBudgetMs = 2f )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ReverseMap = s ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );

            foreach( var obj in objects )
            {
                driver.Initialize( typeof( T ), context, new SerializationStrategy(), obj, null );
                yield return await RunDriverAsync<SerializedData>( driver, timeBudgetMs );
            }
        }

        // --- Deserialize Many Async ---

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( IEnumerable<SerializedData> data, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextKey.Default, data, null, null, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( IEnumerable<SerializedData> data, SerializationConfiguration config, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextKey.Default, data, null, config, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( IEnumerable<SerializedData> data, IForwardReferenceMap l, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextKey.Default, data, l, null, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( IEnumerable<SerializedData> data, IForwardReferenceMap l, SerializationConfiguration config, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextKey.Default, data, l, config, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( Type contextType, IEnumerable<SerializedData> data, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextRegistry.GetID( contextType ), data, null, null, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( Type contextType, IEnumerable<SerializedData> data, SerializationConfiguration config, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextRegistry.GetID( contextType ), data, null, config, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( Type contextType, IEnumerable<SerializedData> data, IForwardReferenceMap l, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextRegistry.GetID( contextType ), data, l, null, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( Type contextType, IEnumerable<SerializedData> data, IForwardReferenceMap l, SerializationConfiguration config, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( ContextRegistry.GetID( contextType ), data, l, config, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( ContextKey context, IEnumerable<SerializedData> data, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( context, data, null, null, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( ContextKey context, IEnumerable<SerializedData> data, SerializationConfiguration config, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( context, data, null, config, timeBudgetMs );

        public static IAsyncEnumerable<T> DeserializeManyAsync<T>( ContextKey context, IEnumerable<SerializedData> data, IForwardReferenceMap l, float timeBudgetMs = 2f )
            => DeserializeManyAsync<T>( context, data, l, null, timeBudgetMs );

        public static async IAsyncEnumerable<T> DeserializeManyAsync<T>( ContextKey context, IEnumerable<SerializedData> data, IForwardReferenceMap l, SerializationConfiguration config, float timeBudgetMs = 2f )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );

            foreach( var d in data )
            {
                driver.Initialize( typeof( T ), context, new DeserializationStrategy(), null, d );
                yield return await RunDriverAsync<T>( driver, timeBudgetMs );
            }
        }

        // --- Populate Many Async ---

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( IEnumerable<(T obj, SerializedData data)> objects, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextKey.Default, objects, null, null, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( IEnumerable<(T obj, SerializedData data)> objects, SerializationConfiguration config, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextKey.Default, objects, null, config, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextKey.Default, objects, l, null, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, SerializationConfiguration config, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextKey.Default, objects, l, config, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextRegistry.GetID( contextType ), objects, null, null, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, SerializationConfiguration config, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextRegistry.GetID( contextType ), objects, null, config, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextRegistry.GetID( contextType ), objects, l, null, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( Type contextType, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, SerializationConfiguration config, float timeBudgetMs = 2f )
            => PopulateManyAsync( ContextRegistry.GetID( contextType ), objects, l, config, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, float timeBudgetMs = 2f )
            => PopulateManyAsync( context, objects, null, null, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, SerializationConfiguration config, float timeBudgetMs = 2f )
            => PopulateManyAsync( context, objects, null, config, timeBudgetMs );

        public static IAsyncEnumerable<T> PopulateManyAsync<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, float timeBudgetMs = 2f )
            => PopulateManyAsync( context, objects, l, null, timeBudgetMs );

        public static async IAsyncEnumerable<T> PopulateManyAsync<T>( ContextKey context, IEnumerable<(T obj, SerializedData data)> objects, IForwardReferenceMap l, SerializationConfiguration config, float timeBudgetMs = 2f )
        {
            var ctx = new SerializationContext( config ?? new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );

            foreach( var (obj, data) in objects )
            {
                driver.Initialize( typeof( T ), context, new DeserializationStrategy(), obj, data );
                yield return await RunDriverAsync<T>( driver, timeBudgetMs );
            }
        }
    }
}
