using HSP.Content;
using HSP.Timelines;
using HSP.Vanilla.Content.AssetLoaders.DDS;
using HSP.Vanilla.Content.AssetLoaders.Metadata;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEngine.Tilemaps;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

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
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadPNG( file, metadata ), true );
                }

                files = Directory.GetFiles( modPath, "*.jpg", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadJPG( file, metadata ), true );
                }

                files = Directory.GetFiles( modPath, "*.tga", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadTGA( file, metadata ), true );
                }

                files = Directory.GetFiles( modPath, "*.dds", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string relPath = FixPath( Path.GetRelativePath( modPath, file ) );
                    Texture2DMetadata metadata = GetTextureMetadata( Path.ChangeExtension( file, ".json" ) );
                    // sprite
                    AssetRegistry.RegisterLazy( $"{modId}::{relPath}", () => LoadDDS( file, metadata ), true );
                }
            }
        }

        private static Texture2DMetadata GetTextureMetadata( string metaPath )
        {
            if( !File.Exists( metaPath ) )
                return new Texture2DMetadata();

            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( metaPath );
            return SerializationUnit.Deserialize<Texture2DMetadata>( _vesselsDataHandler.Read() );
        }

        private static string FixPath( string assetPath )
        {
            return assetPath.Replace( "\\", "/" ).Split( "." )[0];
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
            BinaryReader binaryReader = new BinaryReader( new MemoryStream( File.ReadAllBytes( fileName ) ) );

            if( binaryReader.ReadUInt32() != FourCC.DDS_ )
            {
                throw new IOException( $"File '{fileName}' is not a DDS file!" );
            }

            DDSHeader ddsHeader = new DDSHeader( binaryReader );

            bool mipChain = ddsHeader.ddsCaps.dwCaps.HasFlag( DDSCaps.MIPMAP );

            if( ddsHeader.ddpfPixelFormat.dwFourCC == 0 )
            {
                // explicit format from bit RGBA mask
                uint rBitMask = ddsHeader.ddpfPixelFormat.dwRBitMask;
                uint gBitMask = ddsHeader.ddpfPixelFormat.dwGBitMask;
                uint bBitMask = ddsHeader.ddpfPixelFormat.dwBBitMask;
                uint aBitMask = ddsHeader.ddpfPixelFormat.dwRGBAlphaBitMask;

                TextureFormat format = TextureFormat.Alpha8;
                if( rBitMask == 0xfff && gBitMask == 0 && bBitMask == 0 && aBitMask == 0 )
                    format = TextureFormat.R8;
                else if( rBitMask == 0xfff && gBitMask == 0xfff && bBitMask == 0 && aBitMask == 0 )
                    format = TextureFormat.RG16;
                else if( rBitMask == 0xfff && gBitMask == 0xfff && bBitMask == 0xfff && aBitMask == 0 )
                    format = TextureFormat.RGB24;
                else if( rBitMask == 0xfff && gBitMask == 0xfff && bBitMask == 0xfff && aBitMask == 0xfff )
                    format = TextureFormat.RGBA32;
                else if( rBitMask == 0xffff && gBitMask == 0 && bBitMask == 0 && aBitMask == 0 )
                    format = TextureFormat.R16;
                else if( rBitMask == 0xffff && gBitMask == 0xffff && bBitMask == 0 && aBitMask == 0 )
                    format = TextureFormat.RG32;
                else if( rBitMask == 0xffff && gBitMask == 0xffff && bBitMask == 0xffff && aBitMask == 0 )
                    format = TextureFormat.RGB48;
                else if( rBitMask == 0xffff && gBitMask == 0xffff && bBitMask == 0xffff && aBitMask == 0xffff )
                    format = TextureFormat.RGBA64;

                Texture2D tex = new Texture2D( (int)ddsHeader.dwWidth, (int)ddsHeader.dwHeight, format, mipChain );
                tex.wrapMode = metadata.WrapMode;
                tex.filterMode = metadata.FilterMode;
                tex.anisoLevel = metadata.AnisoLevel;
                tex.LoadRawTextureData( binaryReader.ReadBytes( (int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) ) );
                tex.Apply( false, !metadata.Readable );
                return tex;
            }
            if( ddsHeader.ddpfPixelFormat.dwFourCC == FourCC.DXT1 )
            {
                Texture2D tex = new Texture2D( (int)ddsHeader.dwWidth, (int)ddsHeader.dwHeight, TextureFormat.DXT1, mipChain );
                tex.wrapMode = metadata.WrapMode;
                tex.filterMode = metadata.FilterMode;
                tex.anisoLevel = metadata.AnisoLevel;
                tex.LoadRawTextureData( binaryReader.ReadBytes( (int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) ) );
                tex.Apply( false, !metadata.Readable );
                return tex;
            }
            else if( ddsHeader.ddpfPixelFormat.dwFourCC == FourCC.DXT2 )
            {
                Debug.Log( "DXT2 format is not supported!" );
            }
            else if( ddsHeader.ddpfPixelFormat.dwFourCC == FourCC.DXT3 )
            {
                Debug.LogError( "DXT3 format is not supported." );
            }
            else if( ddsHeader.ddpfPixelFormat.dwFourCC == FourCC.DXT4 )
            {
                Debug.Log( "DXT4 format is not supported!" );
            }
            else if( ddsHeader.ddpfPixelFormat.dwFourCC == FourCC.DXT5 )
            {
                Texture2D tex = new Texture2D( (int)ddsHeader.dwWidth, (int)ddsHeader.dwHeight, TextureFormat.DXT5, mipChain );
                tex.wrapMode = metadata.WrapMode;
                tex.filterMode = metadata.FilterMode;
                tex.anisoLevel = metadata.AnisoLevel;
                tex.LoadRawTextureData( binaryReader.ReadBytes( (int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) ) );
                tex.Apply( false, !metadata.Readable );
                return tex;
            }
            else if( ddsHeader.ddpfPixelFormat.dwFourCC == FourCC.DX10 )
            {
                Debug.Log( "DX10 format is not supported!" );
            }

            throw new IOException();
        }
    }
}