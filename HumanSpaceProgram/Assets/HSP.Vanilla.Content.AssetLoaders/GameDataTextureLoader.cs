using HSP.Content;
using HSP.Vanilla.Content.AssetLoaders.Metadata;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Json;
using System;

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
            // 1. Load Metadata
            Texture2DMetadata meta = new Texture2DMetadata();
            if( await handle.TryOpenSidecarAsync( ".json", out Stream metaStream, ct ) )
            {
                using( metaStream )
                using( StreamReader sr = new StreamReader( metaStream ) )
                {
                    string json = await sr.ReadToEndAsync();

                    SerializedData data = new JsonStringReader( json ).Read();

                    meta = SerializationUnit.Deserialize<Texture2DMetadata>( data );
                }
            }

            // 2. Load Data
            string ext = handle.FormatHint;

            // DDS
            if( ext == ".dds" )
            {
                using Stream stream = await handle.OpenMainStreamAsync( ct );

                byte[] rawBytes = await ReadAllBytes( stream, ct );

                using MemoryStream ms = new MemoryStream( rawBytes );
                return Importer.LoadDDS( ms, meta, "DDS_Asset" );
            }
            else
            {
#warning TODO - check extension/datatype
                // PNG/JPG/TGA
                using Stream stream = await handle.OpenMainStreamAsync( ct );
                byte[] rawBytes = await ReadAllBytes( stream, ct );

                Texture2D tex = new Texture2D( 2, 2, meta.GraphicsFormat, meta.MipMapCount, TextureCreationFlags.MipChain );
                tex.wrapMode = meta.WrapMode;
                tex.filterMode = meta.FilterMode;
                tex.anisoLevel = meta.AnisoLevel;
                tex.LoadImage( rawBytes, !meta.Readable );
                return tex;
            }
        }

        private async Task<byte[]> ReadAllBytes( Stream stream, CancellationToken ct )
        {
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync( ms, 81920, ct );
            return ms.ToArray();
        }
    }

    public static class GameDataTextureLoader
    {


        private static string ReplaceEnd( this string source, string oldSuffix, string newSuffix )
        {
            if( source.EndsWith( oldSuffix ) )
            {
                return source[..^oldSuffix.Length] + newSuffix;
            }
            return source;
        }

        public static void ReloadTextures()
        {
            foreach( var modPath in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string[] files = Directory.GetFiles( modPath, "*.png", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string assetId = HumanSpaceProgramContent.GetAssetID( file );

                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    string spritePath = file.ReplaceEnd( ".png", "_sprite.json" );
                    if( File.Exists( spritePath ) )
                    {
                        JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( spritePath );
                        SpriteMetadata sm = SerializationUnit.Deserialize<SpriteMetadata>( dataHandler.Read() );
                        AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( spritePath ), () => Sprite.Create( AssetRegistry.Get<Texture2D>( assetId ), sm.Rect, sm.Pivot, 100, 1, SpriteMeshType.Tight, sm.Border ), true );
                    }
                    AssetRegistry.RegisterLazy( assetId, () => LoadPNG( file, metadata ), true );
                }

                files = Directory.GetFiles( modPath, "*.jpg", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () => LoadJPG( file, metadata ), true );
                }

                files = Directory.GetFiles( modPath, "*.tga", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () => LoadTGA( file, metadata ), true );
                }

                files = Directory.GetFiles( modPath, "*.dds", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () => LoadDDS( file, metadata ), true );
                }
            }
        }

        private static Texture2DMetadata GetTextureMetadata( string metaPath )
        {
            if( !File.Exists( metaPath ) )
                return new Texture2DMetadata();

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( metaPath );
            return SerializationUnit.Deserialize<Texture2DMetadata>( dataHandler.Read() );
        }

        private static Texture2D LoadPNG( string fileName, Texture2DMetadata metadata )
        {
            var fileBytes = File.ReadAllBytes( fileName );
            Texture2D tex = new Texture2D( 2, 2, metadata.GraphicsFormat, metadata.MipMapCount, TextureCreationFlags.MipChain );
            tex.wrapMode = metadata.WrapMode;
            tex.filterMode = metadata.FilterMode;
            tex.anisoLevel = metadata.AnisoLevel;
            tex.LoadImage( fileBytes, !metadata.Readable );
            return tex;
        }

        private static Texture2D LoadJPG( string fileName, Texture2DMetadata metadata )
        {
            var fileBytes = File.ReadAllBytes( fileName );
            Texture2D tex = new Texture2D( 2, 2, metadata.GraphicsFormat, metadata.MipMapCount, TextureCreationFlags.MipChain );
            tex.wrapMode = metadata.WrapMode;
            tex.filterMode = metadata.FilterMode;
            tex.anisoLevel = metadata.AnisoLevel;
            tex.LoadImage( fileBytes, !metadata.Readable );
            return tex;
        }

        private static Texture2D LoadTGA( string fileName, Texture2DMetadata metadata )
        {
            var fileBytes = File.ReadAllBytes( fileName );
            Texture2D tex = new Texture2D( 2, 2, metadata.GraphicsFormat, metadata.MipMapCount, TextureCreationFlags.MipChain );
            tex.wrapMode = metadata.WrapMode;
            tex.filterMode = metadata.FilterMode;
            tex.anisoLevel = metadata.AnisoLevel;
            tex.LoadImage( fileBytes, !metadata.Readable );
            return tex;
        }

        private static Texture2D LoadDDS( string fileName, Texture2DMetadata metadata )
        {
            return DDS.Importer.LoadDDS( fileName, metadata );
        }
    }
}