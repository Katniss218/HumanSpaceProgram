using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class FileAssetDataHandle : AssetDataHandle
    {
        private readonly string _filePath;
        private readonly string _formatHint;

        public FileAssetDataHandle( string filePath )
        {
            _filePath = filePath;
            _formatHint = Path.GetExtension( filePath ).ToLowerInvariant();
        }

        public override string FormatHint => _formatHint;

        public override bool TryGetLocalFilePath( out string val )
        {
            val = _filePath;
            return true;
        }

        public override async Task<byte[]> PeekBytesAsync( int count, CancellationToken ct )
        {
            // For a file, we open, read, and close.
            // Since this method is supposed to be stateless/idempotent, we don't keep the stream open here.
            // Performance note: OS file caching makes this cheap.
            byte[] buffer = new byte[count];
            using( FileStream fs = new FileStream( _filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
                await fs.ReadAsync( buffer, 0, count, ct );
            }
            return buffer;
        }

        public override Task<Stream> OpenMainStreamAsync( CancellationToken ct )
        {
            // Returns a FileStream. Caller owns it and must dispose it.
            return Task.FromResult<Stream>( new FileStream( _filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) );
        }

        public override Task<bool> TryOpenSidecarAsync( string sidecarExtension, out Stream stream, CancellationToken ct )
        {
            string sidecarPath = Path.ChangeExtension( _filePath, sidecarExtension );
            // Or append? Unity meta files append .meta.
            // But texture metadata usually replaces extension or appends.
            // Let's check both if needed, but for now assuming replacement or strict convention.
            // For textures, existing loader used Path.ChangeExtension(file, ".json").

            if( File.Exists( sidecarPath ) )
            {
                stream = new FileStream( sidecarPath, FileMode.Open, FileAccess.Read, FileShare.Read );
                return Task.FromResult( true );
            }

            stream = null;
            return Task.FromResult( false );
        }

        public override void Dispose()
        {
            // Nothing to dispose for a simple file handle that doesn't own resources until stream is opened.
        }
    }
}