using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Responsible solely for locating data and returning a handle to it.
    /// Does not parse the asset. Runs on Background Threads.
    /// </summary>
    public interface IAssetResolver : ITopologicallySortable<string>
    {
        /// <summary>
        /// Returns true if this resolver can handle the given Asset ID schema for the requested type.
        /// </summary>
        bool CanResolve( AssetUri uri, Type targetType );

        /// <summary>
        /// Asynchronously fetches the data handle.
        /// Must be thread-safe.
        /// </summary>
        Task<AssetDataHandle> ResolveAsync( AssetUri uri, Type targetType, CancellationToken ct );
    }
}