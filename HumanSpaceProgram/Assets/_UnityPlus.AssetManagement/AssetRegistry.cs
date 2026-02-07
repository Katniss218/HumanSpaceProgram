using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// A static registry for assets.
    /// Handles retrieval via cache or the async Resolver-Loader pipeline.
    /// Supports multiple assets sharing the same ID, differentiated by their System.Type.
    /// </summary>
    public static class AssetRegistry
    {
        // Cache: Maps ID -> List of loaded objects (to support collision/overloading by type)
        private static readonly Dictionary<string, List<object>> _loaded = new Dictionary<string, List<object>>();
        private static readonly Dictionary<object, string> _inverseLoaded = new Dictionary<object, string>();

        private static List<IAssetResolver> _resolvers = new List<IAssetResolver>();
        private static List<IAssetLoader> _loaders = new List<IAssetLoader>();

        // Async deduplication
        private static readonly Dictionary<(string id, Type reqType), Task<object>> _loadingTasks = new Dictionary<(string, Type), Task<object>>();

        private static readonly object _lock = new object();

        // Cycle Detection / Re-entrancy Context
        private class LoadNode
        {
            public string AssetID;
            public LoadNode Parent;
        }
        private static readonly AsyncLocal<LoadNode> _reentrancyStack = new AsyncLocal<LoadNode>();

        /// <summary>
        /// The maximum time (in milliseconds) a synchronous Get<T> call will wait before giving up.
        /// Default: 30 seconds.
        /// </summary>
        public static int SynchronousLoadTimeoutMs = 30000;

        /// <summary>
        /// The number of cached (loaded) assets in the registry.
        /// </summary>
        public static int LoadedCount
        {
            get { lock( _lock ) return _inverseLoaded.Count; }
        }

        public static void RegisterResolver( IAssetResolver resolver )
        {
            if( resolver == null )
                throw new ArgumentNullException( nameof( resolver ) );

            lock( _lock )
            {
                if( !_resolvers.Contains( resolver ) )
                {
                    _resolvers.Add( resolver );
                    _resolvers = _resolvers.SortDependencies<IAssetResolver, string>()
                        .ToList();
                }
            }
        }

        public static void UnregisterResolver( IAssetResolver resolver )
        {
            if( resolver == null )
                throw new ArgumentNullException( nameof( resolver ) );

            lock( _lock )
            {
                _resolvers.Remove( resolver );
                _resolvers = _resolvers.SortDependencies<IAssetResolver, string>()
                    .ToList();
            }
        }

        public static void RegisterLoader( IAssetLoader loader )
        {
            if( loader == null )
                throw new ArgumentNullException( nameof( loader ) );

            lock( _lock )
            {
                if( !_loaders.Contains( loader ) )
                    _loaders.Add( loader );
            }
        }

        public static void UnregisterLoader( IAssetLoader loader )
        {
            if( loader == null )
                throw new ArgumentNullException( nameof( loader ) );

            lock( _lock )
                _loaders.Remove( loader );
        }

        /// <summary>
        /// Registers the given (loaded) object as an asset.
        /// If an asset with the same ID and Type exists, it will NOT be overwritten unless it is the exact same reference.
        /// </summary>
        public static void Register( string assetID, object asset )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );
            if( asset == null )
                throw new ArgumentNullException( nameof( asset ) );

            lock( _lock )
            {
                if( !_loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    assets = new List<object>();
                    _loaded[assetID] = assets;
                }

                // Check for duplicate reference
                if( !assets.Contains( asset ) )
                {
                    assets.Add( asset );
                    _inverseLoaded[asset] = assetID;
                }
            }
        }

        /// <summary>
        /// Unregisters and unloads an asset.
        /// </summary>
        public static bool Unregister( string assetID )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );

            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    foreach( var asset in assets )
                    {
                        _inverseLoaded.Remove( asset );
                    }
                    _loaded.Remove( assetID );
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Unregisters and unloads an asset by Reference.
        /// </summary>
        public static bool Unregister( object assetRef )
        {
            if( assetRef == null )
                throw new ArgumentNullException( nameof( assetRef ) );

            lock( _lock )
            {
                if( _inverseLoaded.TryGetValue( assetRef, out string assetID ) )
                {
                    _inverseLoaded.Remove( assetRef );

                    if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                    {
                        assets.Remove( assetRef );
                        if( assets.Count == 0 )
                            _loaded.Remove( assetID );
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clears all registered assets, resolvers, loaders, and pending tasks.
        /// </summary>
        public static void Clear()
        {
            lock( _lock )
            {
                _loaded.Clear();
                _inverseLoaded.Clear();
                _resolvers.Clear();
                _loaders.Clear();
                _loadingTasks.Clear();
            }
        }

        /// <summary>
        /// Scans the registry for UnityEngine.Objects that have been destroyed (== null) and unregisters them.
        /// </summary>
        public static void PruneDestroyedAssets()
        {
            lock( _lock )
            {
                var toRemove = new List<object>();

                foreach( var kvp in _inverseLoaded )
                {
                    // Check if it is a Unity Object and if it is destroyed
                    if( kvp.Key is UnityEngine.Object uObj && uObj == null )
                    {
                        toRemove.Add( kvp.Key );
                    }
                }

                foreach( var obj in toRemove )
                {
                    // Reuse internal unregister logic manually to avoid deadlocks (we already hold lock)
                    if( _inverseLoaded.TryGetValue( obj, out string assetID ) )
                    {
                        _inverseLoaded.Remove( obj );
                        if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                        {
                            assets.Remove( obj );
                            if( assets.Count == 0 )
                                _loaded.Remove( assetID );
                        }
                    }
                }

                if( toRemove.Count > 0 )
                {
                    Debug.Log( $"[AssetRegistry] Pruned {toRemove.Count} destroyed assets." );
                }
            }
        }

        /// <summary>
        /// Retrieves the ID of a registered asset object.
        /// </summary>
        public static string GetAssetID( object assetRef )
        {
            if( assetRef == null )
                return null;

            lock( _lock )
            {
                if( _inverseLoaded.TryGetValue( assetRef, out string id ) )
                    return id;
            }

            return null;
        }

        public static List<string> GetLoadingAssets()
        {
            lock( _lock )
            {
                return _loadingTasks.Keys
                    .Select( k => $"{k.id} ({k.reqType.Name})" )
                    .ToList();
            }
        }

        public static bool IsLoaded( string assetID )
        {
            lock( _lock )
                return _loaded.ContainsKey( assetID );
        }

        /// <summary>
        /// Retrieves a registered asset synchronously.
        /// </summary>
        public static T Get<T>( string assetID ) where T : class
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );

            // 1. Check Cache
            if( TryGetFromCache( assetID, out T cached ) )
                return cached;

            // 2. Trigger Async Pipeline Synchronously
            try
            {
                using( var cts = new CancellationTokenSource( SynchronousLoadTimeoutMs ) )
                using( cts.Token.Register( () => Debug.LogWarning( $"[AssetRegistry] Synchronous load of '{assetID}' timed out after {SynchronousLoadTimeoutMs}ms." ) ) )
                {
                    Task<T> task = GetAsync<T>( assetID, cts.Token );

                    // Busy-wait pump to prevent deadlocks if the loading task requires the main thread
                    while( !task.IsCompleted )
                    {
                        MainThreadDispatcher.Pump();
                        if( !task.IsCompleted )
                            Thread.Sleep( 0 );
                    }

                    return task.GetAwaiter().GetResult();
                }
            }
            catch( AggregateException ae )
            {
                Exception inner = ae.Flatten().InnerException;
                Debug.LogError( $"AssetRegistry.Get<{typeof( T ).Name}>('{assetID}') failed: {inner}" );
                return null;
            }
            catch( Exception ex )
            {
                Debug.LogError( $"AssetRegistry.Get<{typeof( T ).Name}>('{assetID}') failed: {ex}" );
                return null;
            }
        }

        /// <summary>
        /// Retrieves an asset asynchronously. 
        /// </summary>
        public static async Task<T> GetAsync<T>( string assetID, CancellationToken ct = default ) where T : class
        {
            if( string.IsNullOrEmpty( assetID ) )
                return null;

            // 1. Fast Cache Check
            if( TryGetFromCache( assetID, out T cached ) )
                return cached;

            // 2. Cycle Detection
            bool isReentrant = IsLoadingRecursive( assetID );

            // 3. Deduplication
            Task<object> task;
            var loadKey = (assetID, typeof( T ));

            lock( _lock )
            {
                // We use a composite key (ID, Type) to prevent race conditions 
                // where loading 'Foo' as Texture would otherwise block loading 'Foo' as JSON.
                if( _loadingTasks.TryGetValue( loadKey, out task ) )
                {
                    // Task exists, simply await it.
                }
                else
                {
                    task = GetAsyncInternal<T>( assetID, ct );
                    if( !isReentrant )
                        _loadingTasks[loadKey] = task;
                }
            }

            try
            {
                object result = await task.ConfigureAwait( false );

                if( result is T typedResult )
                    return typedResult;

                // Fallback: If deduplication logic failed us or another thread finished 
                // just before we started, check cache one last time.
                if( TryGetFromCache( assetID, out T finalCheck ) )
                    return finalCheck;

                return null;
            }
            finally
            {
                lock( _lock )
                {
                    // Only remove if it's the exact task we added/found
                    if( _loadingTasks.TryGetValue( loadKey, out var t ) && t == task )
                    {
                        _loadingTasks.Remove( loadKey );
                    }
                }
            }
        }

        private static async Task<object> GetAsyncInternal<T>( string assetID, CancellationToken ct ) where T : class
        {
            LoadNode parentNode = _reentrancyStack.Value;
            _reentrancyStack.Value = new LoadNode()
            {
                AssetID = assetID,
                Parent = parentNode
            };

            try
            {
                // Double-check cache inside the reentrancy context
                if( TryGetFromCache( assetID, out T cached ) )
                    return cached;

                if( !AssetUri.TryParse( assetID, out AssetUri uri ) )
                    return null;

                // 1. RESOLUTION PHASE
                List<AssetDataHandle> candidates = await ResolveHandlesAsync<T>( uri, assetID, ct ).ConfigureAwait( false );

                if( candidates == null || candidates.Count == 0 )
                    return null;

                // 2. LOADING PHASE
                try
                {
                    return await TryLoadFromHandlesAsync<T>( assetID, candidates, ct ).ConfigureAwait( false );
                }
                finally
                {
                    DisposeHandles( candidates, assetID );
                }
            }
            finally
            {
                _reentrancyStack.Value = parentNode;
            }
        }

        private static async Task<List<AssetDataHandle>> ResolveHandlesAsync<T>( AssetUri uri, string assetID, CancellationToken ct )
        {
            List<IAssetResolver> activeResolvers;
            lock( _lock )
            {
                activeResolvers = new List<IAssetResolver>( _resolvers );
            }

            List<AssetDataHandle> candidates = new List<AssetDataHandle>();

            foreach( var resolver in activeResolvers )
            {
                if( resolver.CanResolve( uri, typeof( T ) ) )
                {
                    try
                    {
                        IEnumerable<AssetDataHandle> handles = await resolver.ResolveAsync( uri, ct ).ConfigureAwait( false );
                        if( handles != null )
                        {
                            candidates.AddRange( handles );
                        }
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Resolver {((IOverridable<string>)resolver).ID} failed for {assetID}: {ex}" );
                    }
                }
            }

            return candidates;
        }

        private static async Task<object> TryLoadFromHandlesAsync<T>( string assetID, List<AssetDataHandle> handles, CancellationToken ct )
        {
            List<IAssetLoader> activeLoaders;
            lock( _lock )
            {
                activeLoaders = new List<IAssetLoader>( _loaders );
            }

            foreach( var handle in handles )
            {
                if( handle == null )
                    continue;

                foreach( var loader in activeLoaders )
                {
                    // 1. Type Check: Does this loader produce the type we want?
                    if( !typeof( T ).IsAssignableFrom( loader.OutputType ) )
                        continue;

                    // 2. Format Check: Can the loader handle this data?
                    if( loader.CanLoad( handle, typeof( T ) ) )
                    {
                        // MATCH FOUND
                        // Run loader on ThreadPool to avoid blocking main thread with heavy parsing logic.
                        object result = await Task.Run( async () =>
                        {
                            return await loader.LoadAsync( handle, typeof( T ), ct ).ConfigureAwait( false );
                        }, ct ).ConfigureAwait( false );

                        if( result != null )
                        {
                            Register( assetID, result );
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        private static bool TryGetFromCache<T>( string assetID, out T result ) where T : class
        {
            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    // Helper to find an asset of type T in a list of mixed-type objects
                    for( int i = 0; i < assets.Count; i++ )
                    {
                        if( assets[i] is T typedAsset )
                        {
                            result = typedAsset;
                            return true;
                        }
                    }
                }
            }
            result = null;
            return false;
        }

        private static bool IsLoadingRecursive( string assetID )
        {
            LoadNode current = _reentrancyStack.Value;
            while( current != null )
            {
                if( current.AssetID == assetID )
                    return true;
                current = current.Parent;
            }
            return false;
        }

        private static void DisposeHandles( List<AssetDataHandle> handles, string assetID )
        {
            if( handles == null ) 
                return;

            foreach( var handle in handles )
            {
                try
                {
                    handle?.Dispose();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Error disposing handle for {assetID}: {ex}" );
                }
            }
        }
    }
}