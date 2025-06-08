using HSP.Vanilla.Content.AssetLoaders.Metadata;
using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    public static class Importer
    {
        public static Texture2D LoadDDS( string filePath, Texture2DMetadata metadata )
        {
            BinaryReader binaryReader = new BinaryReader( new MemoryStream( File.ReadAllBytes( filePath ) ) );

            if( binaryReader.ReadUInt32() != FourCC.DDS_ )
            {
                throw new IOException( $"File '{filePath}' is not a DDS file!" );
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
