using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// A static registry for assets. <br/>
    /// Register assets at startup. Request them after registering.
    /// </summary>
    public static class AssetRegistry
    {
        struct LazyAsset
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

        static Dictionary<string, object> _cache = new Dictionary<string, object>();
        static Dictionary<object, string> _inverseCache = new Dictionary<object, string>();

        // Assets don't have to all be loaded and cached immediately at startup.
        // An asset can be `lazy-loaded` if a loader method is provided.
        // With this an asset loading and caching can be deferred to when the asset is requested.

        static Dictionary<string, LazyAsset> _lazyRegistry = new Dictionary<string, LazyAsset>();

        /// <summary>
        /// The number of cached assets in the registry.
        /// </summary>
        public static int CachedCount => _cache.Count;
        /// <summary>
        /// The number of lazy loaders in the registry.
        /// </summary>
        public static int LazyCount => _lazyRegistry.Count;

        /// <summary>
        /// Retrieves a registered asset, performs type conversion on the returned asset to the requested type.
        /// </summary>
        /// <remarks>
        /// If nothing is registered under the specified <paramref name="assetID"/>, the registry will try to use a delegate to lazy-load an asset with a given <paramref name="assetID"/>. <br/>
        /// Lazy-loaded assets that are cacheable will be cached after loading, and a cached instance will be returned.
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
            if( _cache.TryGetValue( assetID, out object val ) )
            {
                return val as T;
            }

            // Try to load a lazy asset.
            if( _lazyRegistry.TryGetValue( assetID, out LazyAsset lazy ) )
            {
                object asset = lazy.loader();
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

            if( _inverseCache.TryGetValue( assetRef, out string assetID ) )
            {
                return assetID;
            }

            return null;
        }

        /// <summary>
        /// Returns all registered assets, optionally with a given prefix.
        /// </summary>
        /// <remarks>
        /// This method doesn't include lazy-loaded assets, unless already cached.
        /// </remarks>
        /// <param name="path">The optional prefix that all the returned assets must match.</param>
        public static (string assetID, T asset)[] GetAll<T>( string path = null ) where T : class
        {
            List<(string, T)> assets = new List<(string, T)>();

            foreach( var kvp in _cache )
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
        /// Registers an object as an asset.
        /// </summary>
        /// <remarks>
        /// Replaces the previous entry, if any is registered.
        /// </remarks>
        /// <param name="assetID">The asset ID to register the object under.</param>
        /// <param name="asset">The asset object to register.</param>
        public static void Register( string assetID, object asset )
        {
            if( asset == null )
            {
                throw new ArgumentNullException( nameof( asset ), $"Asset to register can't be null." );
            }
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID of an asset to register can't be null." );
            }

            _cache[assetID] = asset;
            _inverseCache[asset] = assetID;
        }

        /// <summary>
        /// Registers a lazy-loaded (on-demand) asset.
        /// </summary>
        /// <remarks>
        /// A lazy-loaded asset will only be loaded if/when it's requested. <br />
        /// A loaded asset will remain cached after you call <see cref="UnregisterLazy"/> on it. <br />
        /// The lazy-loader is *NOT* unloaded, so the cached asset itself can be later unloaded and then reloaded if needed.
        /// </remarks>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="loader">The function that will load the asset when requested.</param>
        /// <param name="isCacheable">Whether or not the loaded value can be cached. <br/> True if the asset is persistent and singleton, otherwise should be false.</param>
        public static void RegisterLazy( string assetID, Func<object> loader, bool isCacheable )
        {
            // loader example: `() => PNGLoad(imagePathVariable);`
            _lazyRegistry[assetID] = new LazyAsset( loader, isCacheable );
        }

        /// <summary>
        /// Unregisters an asset.
        /// </summary>
        /// <param name="assetID">The asset ID that the asset is registered under.</param>
        /// <returns>True if the asset existed in the registry, otherwise false.</returns>
        public static bool Unregister( string assetID )
        {
            if( assetID == null )
            {
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID of an asset to unregister can't be null." );
            }

            if( _cache.TryGetValue( assetID, out object asset ) )
            {
                _inverseCache.Remove( asset );
                _cache.Remove( assetID );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregisters an asset.
        /// </summary>
        /// <param name="assetRef">The asset to unregister.</param>
        /// <returns>True if the asset existed in the registry, otherwise false.</returns>
        public static bool Unregister( object assetRef )
        {
            if( assetRef == null )
            {
                throw new ArgumentNullException( nameof( assetRef ), $"Asset to unregister can't be null." );
            }

            if( _inverseCache.TryGetValue( assetRef, out string assetID ) )
            {
                _inverseCache.Remove( assetRef );
                _cache.Remove( assetID );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregisters a lazy loader.
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
        /// Removes the specified asset from cache.
        /// </summary>
        /// <remarks>
        /// If there is no lazy loader registered for the specified asset ID, it will not unload.
        /// </remarks>
        /// <param name="assetID">The asset to unregister.</param>
        /// <returns>True if the asset has been removed from cache. Otherwise false.</returns>
        public static bool TryUncache( string assetID )
        {
            if( _lazyRegistry.ContainsKey( assetID ) )
            {
                if( _cache.TryGetValue( assetID, out object asset ) )
                    _inverseCache.Remove( asset );
                _cache.Remove( assetID );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the specified asset from cache.
        /// </summary>
        /// <remarks>
        /// If there is no lazy loader registered for the asset ID of the specified asset, it will not unload.
        /// </remarks>
        /// <param name="asset">The asset to unregister.</param>
        /// <returns>True if the asset has been removed from cache. Otherwise false.</returns>
        public static bool TryUncache( object asset )
        {
            if( _inverseCache.TryGetValue( asset, out string assetID ) )
            {
                if( _lazyRegistry.ContainsKey( assetID ) )
                {
                    _cache.Remove( assetID );
                    _inverseCache.Remove( asset );
                    return true;
                }
            }
            return false;
        }
    }
}