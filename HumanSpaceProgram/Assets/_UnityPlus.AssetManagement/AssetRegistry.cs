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
    /// Handles retrieval via cache, lazy-loading, or the async Resolver-Loader pipeline.
    /// </summary>
    public static class AssetRegistry
    {
        private struct LazyAsset
        {
            public Func<object> loader;
            public bool isCacheable;

            public LazyAsset( Func<object> loader, bool isCacheable )
            {
                this.loader = loader;
                this.isCacheable = isCacheable;
            }
        }

        // Cache
        private static readonly Dictionary<string, object> _loaded = new Dictionary<string, object>();
        private static readonly Dictionary<object, string> _inverseLoaded = new Dictionary<object, string>();

        // Lazy Loaders (Legacy/Synchronous)
        private static readonly Dictionary<string, LazyAsset> _lazyRegistry = new Dictionary<string, LazyAsset>();

        // Resolver-Loader Pipeline
        private static List<IAssetResolver> _resolvers = new List<IAssetResolver>();
        private static List<IAssetLoader> _loaders = new List<IAssetLoader>();

        // Async deduplication
        private static readonly Dictionary<string, Task<object>> _loadingTasks = new Dictionary<string, Task<object>>();

        // Thread synchronization for the dictionaries
        private static readonly object _lock = new object();

        /// <summary>
        /// The number of cached (loaded) assets in the registry.
        /// </summary>
        public static int LoadedCount
        {
            get { lock( _lock ) return _loaded.Count; }
        }

        /// <summary>
        /// The number of lazy loaders in the registry.
        /// </summary>
        public static int LazyCount
        {
            get { lock( _lock ) return _lazyRegistry.Count; }
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

        public static void RegisterLazy( string assetID, Func<object> loader, bool isCacheable )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );
            if( loader == null )
                throw new ArgumentNullException( nameof( loader ) );

            lock( _lock )
            {
                _lazyRegistry[assetID] = new LazyAsset( loader, isCacheable );
            }
        }

        public static bool UnregisterLazy( string assetID )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );

            lock( _lock )
                return _lazyRegistry.Remove( assetID );
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
                return _loaded.ContainsKey( assetID ) || _lazyRegistry.ContainsKey( assetID );
            }
        }

        public static bool IsLoaded( string assetID )
        {
            lock( _lock )
                return _loaded.ContainsKey( assetID );
        }

        public static bool IsRegisteredLazy( string assetID )
        {
            lock( _lock )
                return _lazyRegistry.ContainsKey( assetID );
        }

        public static IEnumerable<string> GetRegisteredIDs()
        {
            lock( _lock )
            {
                return _loaded.Keys.Concat( _lazyRegistry.Keys ).Distinct().ToList();
            }
        }

        /// <summary>
        /// Retrieves a registered asset synchronously.
        /// NOTE: This only checks Cache and Lazy Loaders. It DOES NOT trigger the async Resolver pipeline.
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

            // 2. Check Lazy
            LazyAsset lazy;
            bool hasLazy;
            lock( _lock )
            {
                hasLazy = _lazyRegistry.TryGetValue( assetID, out lazy );
            }

            if( hasLazy )
            {
                object asset = null;
                try
                {
                    asset = lazy.loader.Invoke();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load asset '{assetID}' via lazy loader: {ex}" );
                }

                if( asset != null )
                {
                    if( lazy.isCacheable )
                    {
                        Register( assetID, asset );
                    }
                    return asset as T;
                }
            }

            return default;
        }

        /// <summary>
        /// Asynchronously retrieves an asset. 
        /// Checks Cache, Lazy Loaders (wrapped), and the Resolver-Loader pipeline.
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

            // 2. Deduplication / Join existing task
            Task<object> task;
            lock( _lock )
            {
                if( _loadingTasks.TryGetValue( assetID, out task ) )
                {
                    // Task exists, wait for it
                }
                else
                {
                    // Create new task
                    task = GetAsyncInternal<T>( assetID, ct );
                    _loadingTasks[assetID] = task;
                }
            }

            try
            {
                object result = await task;
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

        private static async Task<object> GetAsyncInternal<T>( string assetID, CancellationToken ct ) where T : class
        {
            // Re-check cache inside task in case it populated while waiting for lock
            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out object val ) )
                    return val;
            }

            // Check Lazy (Legacy Sync Fallback)
            if( IsRegisteredLazy( assetID ) )
            {
                // Warning: Running sync lazy loader on thread pool might be dangerous if it uses Unity API.
                // Ideally lazy loaders should be migrated. For now, we yield to main thread if possible or just run it.
                // Since this is GetAsync, we assume the caller can handle async.
                // To be safe, we wrap Get<T> logic.
                // Note: Get<T> is sync. We should probably just call it.
                T legacyResult = Get<T>( assetID );
                if( legacyResult != null ) return legacyResult;
            }

            // Resolver Pipeline
            if( !AssetUri.TryParse( assetID, out AssetUri uri ) )
            {
                return null;
            }

            // Snapshot resolvers to avoid locking during async ops
            List<IAssetResolver> activeResolvers;
            lock( _lock )
            {
                activeResolvers = new List<IAssetResolver>( _resolvers );
            }

            AssetDataHandle handle = null;

            // 1. Resolve
            Type targetType = typeof( T );
            foreach( var resolver in activeResolvers )
            {
                ct.ThrowIfCancellationRequested();
                if( resolver.CanResolve( uri, targetType ) )
                {
                    try
                    {
                        handle = await resolver.ResolveAsync( uri, targetType, ct );
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
                // Snapshot loaders
                List<IAssetLoader> activeLoaders;
                lock( _lock )
                {
                    activeLoaders = new List<IAssetLoader>( _loaders );
                }

                // 2. Load
                foreach( var loader in activeLoaders )
                {
                    ct.ThrowIfCancellationRequested();
                    // Filter by requested type compatibility if possible, or leave it to loader?
                    // The loader should check if it can produce the OutputType requested or compatible.
                    // Simple check: if T is Texture2D, loader.OutputType should be Texture2D or subclass.
                    if( !typeof( T ).IsAssignableFrom( loader.OutputType ) )
                        continue;

                    if( loader.CanLoad( handle ) )
                    {
                        object result = await loader.LoadAsync( handle, ct );
                        if( result != null )
                        {
                            // Cache result
                            Register( assetID, result );
                            return result;
                        }
                    }
                }
            }
            finally
            {
                // 3. Cleanup
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
    }
}