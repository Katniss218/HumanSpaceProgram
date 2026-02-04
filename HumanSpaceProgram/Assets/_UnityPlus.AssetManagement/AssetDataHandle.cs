using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// An abstract representation of an asset's data source.
    /// Acts as a bridge between the Resolver and the Loader.
    /// </summary>
    public abstract class AssetDataHandle : IDisposable
    {
        /// <summary>
        /// The "hint" for the file type (Extension or MIME type). Can be null.
        /// </summary>
        public abstract string FormatHint { get; }

        /// <summary>
        /// Reads the first <paramref name="count"/> bytes from the beginning of the stream.
        /// </summary>
        /// <remarks>
        /// This method must be IDEMPOTENT regarding the stream position.
        /// Calling PeekBytesAsync(4) multiple times must always return the same first 4 bytes
        /// without advancing the cursor for <see cref="OpenMainStreamAsync"/>.
        /// </remarks>
        public abstract Task<byte[]> PeekBytesAsync( int count, CancellationToken ct );

        /// <summary>
        /// Opens the main data stream from the beginning.
        /// Returns null if the asset is purely procedural/metadata-based and has no stream.
        /// </summary>
        public abstract Task<Stream> OpenMainStreamAsync( CancellationToken ct );

        /// <summary>
        /// Attempts to open a sidecar file (e.g., .meta).
        /// </summary>
        public abstract Task<bool> TryOpenSidecarAsync( string sidecarExtension, out Stream stream, CancellationToken ct );

        /// <summary>
        /// Optimization: Returns a valid local file path if available.
        /// Loaders like VideoPlayer that require file paths should check this.
        /// Returns false if the data is in memory, network-streamed, or generated.
        /// </summary>
        public virtual bool TryGetLocalFilePath( out string val )
        {
            val = null;
            return false;
        }

        public abstract void Dispose();
    }
}