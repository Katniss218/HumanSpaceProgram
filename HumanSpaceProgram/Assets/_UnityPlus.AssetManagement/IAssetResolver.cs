using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Responsible solely for locating data and returning handles to it.
    /// Does not parse the asset. Runs on Background Threads.
    /// </summary>
    public interface IAssetResolver : ITopologicallySortable<string>, IOverridable<string>
    {
        /// <summary>
        /// Returns true if this resolver can handle the given Asset ID schema and requested type.
        /// </summary>
        /// <remarks>
        /// The requested type should only be used for semantic checks, and shouldn't affect the actual resolution logic.
        /// </remarks>
        bool CanResolve( AssetUri uri, Type targetType );

        /// <summary>
        /// Asynchronously fetches all potential data handles for the given URI.
        /// Returns an enumerable because one ID might map to multiple files (e.g. .png and .json) 
        /// which are disambiguated later by the requested Type.
        /// </summary>
        Task<IEnumerable<AssetDataHandle>> ResolveAsync( AssetUri uri, CancellationToken ct );
    }
}