using HSP.Content;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public static class GameDataTextureLoader
    {
        public const string RELOAD_TEXTURES = HSPEvent.NAMESPACE_HSP + ".reload_textures";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_TEXTURES )]
        public static void ReloadTextures2()
        {
            GameDataTextureLoader.ReloadTextures();
        }

        public static void ReloadTextures()
        {
            string gameDataPath = HumanSpaceProgramContent.GetContentDirectoryPath();
            string[] modDirectories = Directory.GetDirectories( gameDataPath );

            foreach( var modPath in modDirectories )
            {
                string modId = Path.GetFileName( modPath );
                // <mod_id>::xyz

                string[] files = Directory.GetFiles( modPath, "*.png", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    // texture
                    // sprite
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadPNG( file, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.MipChain ), true );
                }

                files = Directory.GetFiles( modPath, "*.jpg", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadJPG( file, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.MipChain ), true );
                }

                files = Directory.GetFiles( modPath, "*.tga", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadTGA( file, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.MipChain ), true );
                }

                files = Directory.GetFiles( modPath, "*.dds", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadDDS( file ), true );
                }
            }
        }

        private static string FixPath( string assetPath )
        {
            return assetPath.Replace( "\\", "/" ).Split( "." )[0];
        }

        private static Texture2D LoadPNG( string fileName, GraphicsFormat format, TextureCreationFlags flags )
        {
            var fileData = File.ReadAllBytes( fileName );
            Texture2D tex = new Texture2D( 2, 2, format, flags );
            tex.LoadImage( fileData );
            return tex;
        }

        private static Texture2D LoadJPG( string fileName, GraphicsFormat format, TextureCreationFlags flags )
        {
            var fileData = File.ReadAllBytes( fileName );
            Texture2D tex = new Texture2D( 2, 2, format, flags );
            tex.LoadImage( fileData );
            return tex;
        }

        private static Texture2D LoadTGA( string fileName, GraphicsFormat format, TextureCreationFlags flags )
        {
            var fileData = File.ReadAllBytes( fileName );
            Texture2D tex = new Texture2D( 2, 2, format, flags );
            tex.LoadImage( fileData );
            return tex;
        }

        private static Texture2D LoadDDS( string fileName )
        {
            var fileData = File.ReadAllBytes( fileName );

            //if( textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5 )
            //    throw new Exception( "Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method." );

            byte ddsSizeCheck = fileData[4];
            if( ddsSizeCheck != 124 )
                throw new Exception( "Invalid DDS DXTn texture. Unable to read" );  //this header byte should be 124 for DDS image files

            int height = fileData[13] * 256 + fileData[12];
            int width = fileData[17] * 256 + fileData[16];

            const int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[fileData.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy( fileData, DDS_HEADER_SIZE, dxtBytes, 0, fileData.Length - DDS_HEADER_SIZE );

            Texture2D texture = new Texture2D( width, height, TextureFormat.R16, false );
            texture.LoadRawTextureData( dxtBytes );
            texture.Apply();

            return (texture);
        }
    }
}