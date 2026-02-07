using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content
{
    public class FileAssetDataHandle : AssetDataHandle
    {
        private readonly string _filePath;
        private readonly AssetFormat _format;

        public FileAssetDataHandle( string filePath )
        {
            _filePath = filePath;
            _format = AssetFormat.FromExtension( Path.GetExtension( filePath ) );
        }

        public override AssetFormat Format => _format;

        public override bool TryGetLocalFilePath( out string val )
        {
            val = _filePath;
            return true;
        }

        public override async Task<byte[]> PeekBytesAsync( int count, CancellationToken ct )
        {
            // Use Task.Run to ensure IO happens on thread pool, not main thread synchronously
            return await Task.Run( async () =>
            {
                byte[] buffer = new byte[count];
                using( FileStream fs = new FileStream( _filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                {
                    await fs.ReadAsync( buffer, 0, count, ct ).ConfigureAwait( false );
                }
                return buffer;
            }, ct ).ConfigureAwait( false );
        }

        public override Task<Stream> OpenMainStreamAsync( CancellationToken ct )
        {
            // FileStream constructor can sometimes block slightly depending on OS/Antivirus.
            // We wrap in Task.Run to be strictly async safe.
            return Task.Run<Stream>( () =>
            {
                return new FileStream( _filePath, FileMode.Open, FileAccess.Read, FileShare.Read );
            }, ct );
        }

        public override Task<bool> TryOpenSidecarAsync( string sidecarExtension, out Stream stream, CancellationToken ct )
        {
            // Synchronous file check is unavoidable here unless we want to wrap everything.
            // File.Exists is generally fast enough, but technically blocking.

            // New Logic: Append extension (e.g. "file.png" + ".json" -> "file.png.json")
            // Assuming sidecarExtension contains the dot.
            string sidecarPath = _filePath + sidecarExtension;

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