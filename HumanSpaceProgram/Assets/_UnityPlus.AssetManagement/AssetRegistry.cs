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
    /// </summary>
    public static class AssetRegistry
    {
        // Cache
        private static readonly Dictionary<string, object> _loaded = new Dictionary<string, object>();
        private static readonly Dictionary<object, string> _inverseLoaded = new Dictionary<object, string>();

        // Resolver-Loader Pipeline
        private static List<IAssetResolver> _resolvers = new List<IAssetResolver>();
        private static List<IAssetLoader> _loaders = new List<IAssetLoader>();

        // Async deduplication
        private static readonly Dictionary<string, Task<object>> _loadingTasks = new Dictionary<string, Task<object>>();

        // Thread synchronization for the dictionaries
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
            get { lock( _lock ) return _loaded.Count; }
        }

        /// <summary>
        /// Registers the given (loaded) object as an asset.
        /// </summary>
        public static void Register( string assetID, object asset )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );
            if( asset == null )
                throw new ArgumentNullException( nameof( asset ) );

            lock( _lock )
            {
                _loaded[assetID] = asset;
                _inverseLoaded[asset] = assetID;
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
                if( _loaded.TryGetValue( assetID, out object asset ) )
                {
                    _inverseLoaded.Remove( asset );
                    _loaded.Remove( assetID );
                    return true;
                }
            }
            return false;
        }

        public static bool Unregister( object assetRef )
        {
            if( assetRef == null )
                throw new ArgumentNullException( nameof( assetRef ) );

            lock( _lock )
            {
                if( _inverseLoaded.TryGetValue( assetRef, out string assetID ) )
                {
                    _inverseLoaded.Remove( assetRef );
                    _loaded.Remove( assetID );
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves the ID of a registered asset object.
        /// </summary>
        /// <param name="assetRef">The asset object instance.</param>
        /// <returns>The Asset ID, or null if not found.</returns>
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
            lock( _lock )
                _loaders.Remove( loader );
        }

        public static bool IsRegistered( string assetID )
        {
            lock( _lock )
            {
                return _loaded.ContainsKey( assetID );
            }
        }

        public static bool IsLoaded( string assetID )
        {
            lock( _lock )
                return _loaded.ContainsKey( assetID );
        }

        public static IEnumerable<string> GetRegisteredIDs()
        {
            lock( _lock )
            {
                return _loaded.Keys.ToList();
            }
        }

        /// <summary>
        /// Retrieves a registered asset synchronously.
        /// <para>
        /// If the asset is not in the cache, this method will attempt to load it via the Resolver pipeline synchronously.
        /// <b>WARNING:</b> This uses the <see cref="MainThreadDispatcher"/> to cooperatively wait for the loading task.
        /// This prevents deadlocks but stalls the main thread until the asset is loaded.
        /// </para>
        /// </summary>
        public static T Get<T>( string assetID ) where T : class
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );

            // 1. Check Cache
            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out object val ) )
                    return val as T;
            }

            // 2. Trigger Async Pipeline Synchronously with Pump Guard
            try
            {
                using( var cts = new CancellationTokenSource( SynchronousLoadTimeoutMs ) )
                {
                    Task<T> task = GetAsync<T>( assetID, cts.Token );

                    // Cooperative Wait Loop
                    // We must keep the MainThreadDispatcher pumping, otherwise any loader waiting 
                    // for the main thread (via RunAsync) will never finish, causing a deadlock.
                    while( !task.IsCompleted )
                    {
                        MainThreadDispatcher.Pump();

                        // Avoid burning 100% CPU if possible, though in a single-threaded game loop, 
                        // blocking usually means freezing. Thread.Yield() helps OS scheduling.
                        // However, we want to return as soon as possible.
                        if( !task.IsCompleted )
                        {
                            // Optional: Small sleep/spin. 
                            // Thread.Sleep(0) yields the rest of time slice.
                            Thread.Sleep( 0 );
                        }
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
        /// Asynchronously retrieves an asset. 
        /// Checks Cache and the Resolver-Loader pipeline.
        /// </summary>
        public static async Task<T> GetAsync<T>( string assetID, CancellationToken ct = default ) where T : class
        {
            if( string.IsNullOrEmpty( assetID ) )
                return null;

            // 1. Fast Cache Check
            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out object val ) )
                    return val as T;
            }

            // 2. Cycle Detection
            bool isReentrant = IsLoadingRecursive( assetID );

            // 3. Deduplication / Join existing task
            Task<object> task;
            lock( _lock )
            {
                if( !isReentrant && _loadingTasks.TryGetValue( assetID, out task ) )
                {
                    // Task exists, wait for it
                }
                else
                {
                    // Create new task
                    task = GetAsyncInternal<T>( assetID, ct );

                    if( !isReentrant )
                    {
                        _loadingTasks[assetID] = task;
                    }
                }
            }

            try
            {
                object result = await task.ConfigureAwait( false ); // Stay on background thread if possible
                return result as T;
            }
            finally
            {
                // Cleanup task reference if it was ours
                lock( _lock )
                {
                    if( _loadingTasks.TryGetValue( assetID, out var t ) && t == task )
                    {
                        _loadingTasks.Remove( assetID );
                    }
                }
            }
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

        private static async Task<object> GetAsyncInternal<T>( string assetID, CancellationToken ct ) where T : class
        {
            // Push Context
            LoadNode parentNode = _reentrancyStack.Value;
            _reentrancyStack.Value = new LoadNode { AssetID = assetID, Parent = parentNode };

            try
            {
                // Re-check cache inside task
                lock( _lock )
                {
                    if( _loaded.TryGetValue( assetID, out object val ) )
                        return val;
                }

                // Resolver Pipeline
                if( !AssetUri.TryParse( assetID, out AssetUri uri ) )
                {
                    return null;
                }

                List<IAssetResolver> activeResolvers;
                lock( _lock )
                {
                    activeResolvers = new List<IAssetResolver>( _resolvers );
                }

                AssetDataHandle handle = null;

                Type targetType = typeof( T );
                foreach( var resolver in activeResolvers )
                {
                    ct.ThrowIfCancellationRequested();
                    // Resolvers run on thread pool naturally, but we ensure await doesn't capture context unnecessarily
                    if( resolver.CanResolve( uri, targetType ) )
                    {
                        try
                        {
                            handle = await resolver.ResolveAsync( uri, targetType, ct ).ConfigureAwait( false );
                            if( handle != null )
                                break;
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Resolver {resolver.ID} failed for {assetID}: {ex}" );
                        }
                    }
                }

                if( handle == null )
                    return null;

                try
                {
                    List<IAssetLoader> activeLoaders;
                    lock( _lock )
                    {
                        activeLoaders = new List<IAssetLoader>( _loaders );
                    }

                    foreach( var loader in activeLoaders )
                    {
                        ct.ThrowIfCancellationRequested();
                        if( !typeof( T ).IsAssignableFrom( loader.OutputType ) )
                            continue;

                        if( loader.CanLoad( handle ) )
                        {
                            // CRITICAL: We explicitly wrap the loader execution in Task.Run.
                            // This ensures the loader starts on the ThreadPool, breaking any implicit 
                            // dependence on the Unity Main Thread SynchronizationContext.
                            // The Loader must use MainThreadDispatcher to get back to the main thread.

                            object result = await Task.Run( async () =>
                            {
                                return await loader.LoadAsync( handle, ct ).ConfigureAwait( false );
                            }, ct ).ConfigureAwait( false );

                            if( result != null )
                            {
                                Register( assetID, result );
                                return result;
                            }
                        }
                    }
                }
                finally
                {
                    try
                    {
                        handle.Dispose();
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Error disposing handle for {assetID}: {ex}" );
                    }
                }

                return null;
            }
            finally
            {
                _reentrancyStack.Value = parentNode;
            }
        }
    }
}