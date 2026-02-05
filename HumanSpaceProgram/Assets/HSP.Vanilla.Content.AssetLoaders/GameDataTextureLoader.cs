using HSP.Vanilla.Content.AssetLoaders.DDS;
using HSP.Vanilla.Content.AssetLoaders.Metadata;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityPlus;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class TextureLoader : IAssetLoader
    {
        public const string RELOAD_TEXTURES = HSPEvent.NAMESPACE_HSP + ".gdtl.reload_textures";
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_TEXTURES )]
        private static void RegisterTextureLoader()
        {
            AssetRegistry.RegisterLoader( new TextureLoader() );
        }

        public Type OutputType => typeof( Texture2D );

        public bool CanLoad( AssetDataHandle handle )
        {
            string ext = handle.FormatHint;
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".dds";
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, CancellationToken ct )
        {
            // 1. Load Metadata (Background)
            Texture2DMetadata meta = new Texture2DMetadata();
            if( await handle.TryOpenSidecarAsync( ".json", out Stream metaStream, ct ) )
            {
                using( metaStream )
                using( StreamReader sr = new StreamReader( metaStream ) )
                {
                    string json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    SerializedData data = new JsonStringReader( json ).Read();
                    meta = SerializationUnit.Deserialize<Texture2DMetadata>( data );
                }
            }

            // 2. Read Data (Background)
            // We read to memory first to avoid blocking the main thread with I/O during the creation phase.
            byte[] rawBytes;
            using( Stream stream = await handle.OpenMainStreamAsync( ct ).ConfigureAwait(false) )
            {
                rawBytes = await ReadAllBytes( stream, ct ).ConfigureAwait(false);
            }

            string ext = handle.FormatHint;

            // 3. Create Unity Object (Main Thread)
            return await MainThreadDispatcher.RunAsync( () =>
            {
                if( ext == ".dds" )
                {
                    using MemoryStream ms = new MemoryStream( rawBytes );
                    return Importer.LoadDDS( ms, meta, "DDS_Asset" );
                }
                else
                {
                    // PNG/JPG/TGA
                    Texture2D tex = new Texture2D( 2, 2, meta.GraphicsFormat, meta.MipMapCount, TextureCreationFlags.MipChain );
                    tex.wrapMode = meta.WrapMode;
                    tex.filterMode = meta.FilterMode;
                    tex.anisoLevel = meta.AnisoLevel;
                    tex.LoadImage( rawBytes, !meta.Readable );
                    return tex;
                }
            } ).ConfigureAwait(false);
        }

        private async Task<byte[]> ReadAllBytes( Stream stream, CancellationToken ct )
        {
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync( ms, 81920, ct ).ConfigureAwait(false);
            return ms.ToArray();
        }
    }
}