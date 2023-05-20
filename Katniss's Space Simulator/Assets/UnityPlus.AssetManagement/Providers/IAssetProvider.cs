using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public interface IAssetProvider<T>
    {
        /// <summary>
        /// Returns an asset for a given assetID.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS - Return false if your provider doesn't support individual fetching.
        /// </remarks>
        bool Get( string assetID, out T obj );
        
        /// <summary>
        /// Returns an assetID for a given asset.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS - Return false if your provider doesn't support individual reverse fetching.
        /// </remarks>
        bool GetAssetID( T obj, out string assetID );

        /// <summary>
        /// Returns a map between assetIDs and assets.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS - Return empty if your provider doesn't support batched fetching.
        /// </remarks>
        IEnumerable<(string assetID, T obj)> GetAll();
    }
}