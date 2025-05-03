using HSP.Content;
using HSP.Vanilla.Content.AssetLoaders.Metadata;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public static class GameDataTextureLoader
    {
        public const string RELOAD_TEXTURES = HSPEvent.NAMESPACE_HSP + ".gdtl.reload_textures";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_TEXTURES )]
        public static void ReloadTextures2()
        {
            GameDataTextureLoader.ReloadTextures();
        }

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