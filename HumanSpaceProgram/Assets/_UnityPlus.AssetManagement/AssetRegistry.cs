using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// A static registry for assets. <br/>
    /// Register assets at startup. Request them after registering.
    /// </summary>
    /// <remarks>
    /// The assets are associated 1-to-1 with string IDs.
    /// </remarks>
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

        // Registry is a class used to manage 2 types of `assets`:
        // - cacheable      - shared (singleton), often immutable, owned by the registry.
        // - non-cacheable  - unique (transient), often mutable, owned by the caller.

        // An `asset` in this context is more broad than just Unity asset, since it can be an arbitrary class (reference type).

        private static Dictionary<string, object> _loaded = new Dictionary<string, object>();
        private static Dictionary<object, string> _inverseLoaded = new Dictionary<object, string>();

        // Assets don't have to all be loaded and cached immediately at startup.
        // An asset can be `lazy-loaded` if a loader method is provided.
        // With this an asset loading and caching can be deferred to when the asset is requested.

        private static Dictionary<string, LazyAsset> _lazyRegistry = new Dictionary<string, LazyAsset>();

        /// <summary>
        /// The number of cached (loaded) assets in the registry.
        /// </summary>
        public static int LoadedCount => _loaded.Count;

        /// <summary>
        /// The number of lazy loaders in the registry.
        /// </summary>
        public static int LazyCount => _lazyRegistry.Count;

        /// <summary>
        /// Checks if any asset is registered under the specified asset ID.
        /// </summary>
        public static bool IsRegistered( string assetID )
        {
            return _loaded.ContainsKey( assetID ) || _lazyRegistry.ContainsKey( assetID );
        }

        /// <summary>
        /// Checks if a lazy loader is registered under the specified asset ID.
        /// </summary>
        public static bool IsRegisteredLazy( string assetID )
        {
            return _lazyRegistry.ContainsKey( assetID );
        }

        /// <summary>
        /// Checks if an asset is cached (has been loaded) under the specified asset ID.
        /// </summary>
        public static bool IsLoaded( string assetID )
        {
            return _loaded.ContainsKey( assetID );
        }

        /// <summary>
        /// Retrieves a registered asset, performs graceful type conversion on the asset to the requested type.
        /// </summary>
        /// <remarks>
        /// If no asset has been registered (loaded) for the specified <paramref name="assetID"/>, and there exists a lazy loader for the asset, it will be invoked to load the asset. <br/>
        /// Lazy-loaded assets that are marked as 'cacheable' will be cached after loading, and a cached instance will be returned.
        /// </remarks>
        /// <typeparam name="T">The requested type.</typeparam>
        /// <param name="assetID">The asset ID under which the asset is registered.</param>
        /// <returns>The registered asset, converted to the specified type <typeparamref name="T"/>. <br/>
        /// <see cref="default"/>, if no asset can be found. <br/>
        /// <see cref="default"/>, if the type of the registered asset doesn't match the requested type.</returns>
        public static T Get<T>( string assetID ) where T : class
        {
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID of an asset to get can't be null." );
            }

            // Try to get an already loaded asset.
            if( _loaded.TryGetValue( assetID, out object val ) )
            {
                return val as T;
            }

            // Try to load a lazy asset.
            if( _lazyRegistry.TryGetValue( assetID, out LazyAsset lazy ) )
            {
                object asset = null;
                try
                {
                    asset = lazy.loader.Invoke();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load an asset with ID '{assetID}' using its lazy loader." );
                    Debug.LogException( ex );
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
        /// Returns the asset ID of a registered asset.
        /// </summary>
        /// <param name="assetRef">A reference to an asset retrieved from this registry.</param>
        /// <returns>The asset ID of the specified asset, <see cref="null"/> if it doesn't exist in the registry.</returns>
        public static string GetAssetID( object assetRef )
        {
            if( assetRef == null )
            {
                throw new ArgumentNullException( nameof( assetRef ), $"Asset to get the asset ID of can't be null." );
            }

            if( _inverseLoaded.TryGetValue( assetRef, out string assetID ) )
            {
                return assetID;
            }

            return null;
        }

        /// <summary>
        /// Returns all loaded assets, optionally filtered to those whose asset IDs start with the given prefix.
        /// </summary>
        /// <remarks>
        /// This method doesn't include lazy-loaded assets, unless already cached.
        /// </remarks>
        /// <param name="path">The optional prefix that all the returned asset IDs must start with.</param>
        public static (string assetID, T asset)[] GetAll<T>( string path = null ) where T : class
        {
            List<(string, T)> assets = new List<(string, T)>();

            foreach( var kvp in _loaded )
            {
                if( kvp.Value is not T )
                    continue;

                if( path == null || kvp.Key.StartsWith( path ) )
                {
                    assets.Add( (kvp.Key, (T)kvp.Value) );
                }
            }

            return assets.ToArray();
        }

        /// <summary>
        /// Registers the given (loaded) object as an asset.
        /// </summary>
        /// <remarks>
        /// Replaces the previous entry, if any is registered.
        /// </remarks>
        /// <param name="assetID">The asset ID to register the object under.</param>
        /// <param name="asset">The asset object to register.</param>
        public static void Register( string assetID, object asset )
        {
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID of an asset to register can't be null." );
            }
            if( asset == null )
            {
                throw new ArgumentNullException( nameof( asset ), $"Asset to register can't be null." );
            }

            // Dispose previous when the support for that is added.
            _loaded[assetID] = asset;
            _inverseLoaded[asset] = assetID;
        }

        /// <summary>
        /// Unregisters and unloads an asset.
        /// </summary>
        /// <param name="assetID">The asset ID that the asset is registered under.</param>
        /// <returns>True if the asset existed in the registry, otherwise false.</returns>
        public static bool Unregister( string assetID )
        {
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID of an asset to unregister can't be null." );
            }

            if( _loaded.TryGetValue( assetID, out object asset ) )
            {
                _inverseLoaded.Remove( asset );
                _loaded.Remove( assetID );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregisters and unloads an asset.
        /// </summary>
        /// <param name="assetRef">The asset to unregister.</param>
        /// <returns>True if the asset existed in the registry, otherwise false.</returns>
        public static bool Unregister( object assetRef )
        {
            if( assetRef == null )
            {
                throw new ArgumentNullException( nameof( assetRef ), $"Asset to unregister can't be null." );
            }

            if( _inverseLoaded.TryGetValue( assetRef, out string assetID ) )
            {
                _inverseLoaded.Remove( assetRef );
                _loaded.Remove( assetID );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers a lazy-loaded asset. <br/>
        /// The lazy loader will be invoked when the asset is requested. <br/>
        /// </summary>
        /// <remarks>
        /// Replaces the previous entry, if any is registered. <br/>
        /// The asset loaded by the lazy loader will remain cached after the lazy loader itself has been unregistered, and needs to be unloaded separately. <br />
        /// The lazy-loader itself is never unregistered unless the <see cref="UnregisterLazy"/> function is called.
        /// </remarks>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="loader">The function that will load the asset when requested.</param>
        /// <param name="isCacheable">Whether or not the loaded value can be cached. <br/> True if the asset is persistent and singleton, otherwise should be false.</param>
        public static void RegisterLazy( string assetID, Func<object> loader, bool isCacheable )
        {
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID of an asset to register can't be null." );
            }
            if( loader == null )
            {
                throw new ArgumentNullException( nameof( loader ), $"Lazy-loader function can't be null." );
            }

            // loader example: `() => PNGLoad(imagePathVariable);`
            _lazyRegistry[assetID] = new LazyAsset( loader, isCacheable );
        }

        /// <summary>
        /// Unregisters a lazy loader function for the given asset ID.
        /// </summary>
        public static bool UnregisterLazy( string assetID )
        {
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID to unregister from can't be null." );
            }

            return _lazyRegistry.Remove( assetID );
        }

        /// <summary>
        /// Unloads the specified lazy-loaded asset from cache.
        /// </summary>
        /// <remarks>
        /// The specified asset will only unload if there is a lazy loader registered for its asset ID.
        /// </remarks>
        /// <param name="assetID">The asset to unregister.</param>
        /// <returns>True if the asset has been unloaded. Otherwise false.</returns>
        public static bool TryUnloadCachedLazy( string assetID )
        {
            if( _lazyRegistry.ContainsKey( assetID ) )
            {
                if( _loaded.TryGetValue( assetID, out object asset ) )
                    _inverseLoaded.Remove( asset );
                _loaded.Remove( assetID );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unloads the specified lazy-loaded asset from cache.
        /// </summary>
        /// <remarks>
        /// The specified asset will only unload if there is a lazy loader registered for its asset ID.
        /// </remarks>
        /// <param name="asset">The asset to unregister.</param>
        /// <returns>True if the asset has been unloaded. Otherwise false.</returns>
        public static bool TryUnloadCachedLazy( object asset )
        {
            if( _inverseLoaded.TryGetValue( asset, out string assetID ) )
            {
                if( _lazyRegistry.ContainsKey( assetID ) )
                {
                    _loaded.Remove( assetID );
                    _inverseLoaded.Remove( asset );
                    return true;
                }
            }
            return false;
        }
    }
}