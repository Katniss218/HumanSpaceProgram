using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class VirtualResizeAssetDataHandle : AssetDataHandle
    {
        public string BaseAssetID { get; }
        public int TargetWidth { get; }
        public int TargetHeight { get; }

        public VirtualResizeAssetDataHandle( string baseAssetId, int width, int height )
        {
            BaseAssetID = baseAssetId;
            TargetWidth = width;
            TargetHeight = height;
        }

        // We use a custom extension to ensure only the TextureResizingLoader picks this up.
        public override AssetFormat Format => (AssetFormat)".virtual_resize_op";

        public override Task<byte[]> PeekBytesAsync( int count, CancellationToken ct )
        {
            return Task.FromResult( new byte[0] );
        }

        public override Task<Stream> OpenMainStreamAsync( CancellationToken ct )
        {
            return Task.FromResult<Stream>( null );
        }

        public override Task<bool> TryOpenSidecarAsync( string sidecarExtension, out Stream stream, CancellationToken ct )
        {
            stream = null;
            return Task.FromResult( false );
        }

        public override void Dispose()
        {
            // No resources to dispose
        }
    }
}