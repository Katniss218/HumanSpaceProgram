using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// A static registry for assets.
    /// </summary>
    /// <remarks>
    /// Register assets at startup.
    /// </remarks>
    public static class AssetRegistry
    {
        // Registry is a class used to manage shared (singleton) references to `assets`.
        // An `asset` in this context is more broad than just Unity asset, since it can be an arbitrary class (reference type).

        static Dictionary<string, object> _cache = new Dictionary<string, object>();
        static Dictionary<object, string> _inverseCache = new Dictionary<object, string>();

        // registry items don't have to be loaded at startup, if a function is provided to the lazy loader, that can read e.g. an .fbx file, a mesh can be imported at runtime.
        // if a provider is used, the registry can also try to load assets under asset IDs that didn't have an asset registered yet.

        static Dictionary<string, Func<object>> _lazyCache = new Dictionary<string, Func<object>>();

        /// <summary>
        /// Retrieves a registered asset, performs type conversion on the returned asset to the requested type.
        /// </summary>
        /// <remarks>
        /// If nothing is registered under the specified <paramref name="assetID"/>, the registry will try to use an <see cref="IAssetProvider"/> to load an asset with a given <paramref name="assetID"/>.
        /// </remarks>
        /// <typeparam name="T">The requested type.</typeparam>
        /// <param name="assetID">The asset ID under which the asset is registered.</param>
        /// <returns>The registered asset, converted to the specified type <typeparamref name="T"/>. <br />
        /// <see cref="null"/> if no asset can be found. <br />
        /// <see cref="null"/> if the actual type of the asset doesn't match the requested type (actual `as` requested).</returns>
        public static T Get<T>( string assetID ) where T : class
        {
            // Try to get an already loaded asset.
            if( _cache.TryGetValue( assetID, out object val ) )
            {
                return val as T;
            }

            // Try to load a lazy asset.
            if( _lazyCache.TryGetValue( assetID, out Func<object> loader ) )
            {
                object asset = loader();
                if( asset != null )
                {
                    Register( assetID, asset );
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
            // We don't have to bother checking providers, since we expect the parameter object to already come from the registry.

            if( _inverseCache.TryGetValue( assetRef, out string assetID ) )
            {
                return assetID;
            }

            return null;
        }

        public static (string, T)[] GetAll<T>() where T : class
        {
            int count = _cache.Count;
            List<(string, T)> assets = new List<(string, T)>();

            foreach( var kvp in _cache )
            {
                if( kvp.Value is not T )
                    continue;

                assets.Add( (kvp.Key, (T)kvp.Value) );
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
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID to register under can't be null." );
            }

            _cache[assetID] = asset;
            _inverseCache[asset] = assetID;
        }

        /// <summary>
        /// Registers a lazy-loaded (on-demand) asset.
        /// </summary>
        /// <remarks>
        /// A lazy-loaded asset will only be loaded if/when it's requested. <br />
        /// A lazy-loaded asset will still be loaded even if you unregister it after loading.
        /// </remarks>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="loader">The function that will load the asset when requested.</param>
        public static void RegisterLazy( string assetID, Func<object> loader )
        {
            // () => PNGLoad(imagePathVariable); // for example
            _lazyCache[assetID] = loader;
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
                throw new ArgumentNullException( nameof( assetID ), $"Asset ID to unregister from can't be null." );
            }

            if( _cache.TryGetValue( assetID, out object asset ) )
            {
                _inverseCache.Remove( asset );
                _cache.Remove( assetID );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Unregisters an asset.
        /// </summary>
        /// <param name="assetRef">The asset to unregister.</param>
        /// <returns>True if the asset existed in the registry, otherwise false.</returns>
        public static bool Unregister( object assetRef )
        {
            if( _inverseCache.TryGetValue( assetRef, out string assetID ) )
            {
                _inverseCache.Remove( assetRef );
                _cache.Remove( assetID );
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}