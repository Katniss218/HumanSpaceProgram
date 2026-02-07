using HSP.Vanilla.Content.AssetLoaders.Metadata;
using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    public static class Importer
    {
        public static Texture2D LoadDDS( string filePath, Texture2DMetadata metadata )
        {
            using FileStream stream = File.OpenRead( filePath );
            return LoadDDS( stream, metadata, filePath );
        }

        public static Texture2D LoadDDS( Stream stream, Texture2DMetadata metadata, string debugLabel = "Stream" )
        {
            // BinaryReader leaves stream open when disposed if leaveOpen is true, but here we don't own the stream in the inner scope?
            // Actually BinaryReader takes ownership by default. We should be careful.
            // We'll wrap it and ensure we don't close the base stream if the caller owns it? 
            // For now, standard practice: Reader wraps stream.

            // However, to keep it safe for the caller who might want to reuse the stream, lets use leaveOpen=true
            using BinaryReader binaryReader = new BinaryReader( stream, System.Text.Encoding.Default, true );

            if( binaryReader.ReadUInt32() != FourCC.DDS_ )
            {
                throw new IOException( $"Data in '{debugLabel}' is not a valid DDS!" );
            }

            DDSHeader ddsHeader = new DDSHeader( binaryReader );
            bool mipChain = ddsHeader.ddsCaps.dwCaps.HasFlag( DDSCaps.MIPMAP );

            bool supported = GetFormat( ddsHeader, out TextureFormat format, debugLabel );

            if( !supported )
            {
                throw new IOException( "Unsupported or Malformed DDS data." );
            }

            Texture2D tex = new Texture2D( (int)ddsHeader.dwWidth, (int)ddsHeader.dwHeight, format, mipChain );
            tex.wrapMode = metadata.WrapMode;
            tex.filterMode = metadata.FilterMode;
            tex.anisoLevel = metadata.AnisoLevel;

            // Read remaining bytes for texture data
            long dataSize = stream.Length - stream.Position;
            byte[] textureData = binaryReader.ReadBytes( (int)dataSize );

            tex.LoadRawTextureData( textureData );
            tex.Apply( false, !metadata.Readable );
            return tex;
        }

        private static bool GetFormat( DDSHeader ddsHeader, out TextureFormat format, string debugLabel )
        {
            format = TextureFormat.RGBA32; // Default

            if( ddsHeader.ddpfPixelFormat.dwFourCC == 0 )
            {
                // Explicit RGBA masks...
                uint rBitMask = ddsHeader.ddpfPixelFormat.dwRBitMask;
                uint gBitMask = ddsHeader.ddpfPixelFormat.dwGBitMask;
                uint bBitMask = ddsHeader.ddpfPixelFormat.dwBBitMask;
                uint aBitMask = ddsHeader.ddpfPixelFormat.dwRGBAlphaBitMask;

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
                else return false;

                return true;
            }

            switch( ddsHeader.ddpfPixelFormat.dwFourCC )
            {
                case FourCC.DXT1:
                    format = TextureFormat.DXT1;
                    return true;
                case FourCC.DXT5:
                    format = TextureFormat.DXT5;
                    return true;
                case FourCC.DX10:
                    if( ddsHeader.dx10.HasValue )
                    {
                        return MapDX10Format( ddsHeader.dx10.Value, out format, debugLabel );
                    }
                    break;
            }

            return false;
        }

        private static bool MapDX10Format( DDSHeaderDX10 dx10, out TextureFormat format, string debugLabel )
        {
            format = TextureFormat.RGBA32;

            if( dx10.resourceDimension != D3D11ResourceDimension.TEXTURE2D ||
               (dx10.miscFlag & D3D11Misc.TEXTURECUBE) != 0 ||
                dx10.arraySize > 1 )
            {
                Debug.LogError( $"DDS Importer: Complex DX10 resources not supported. {debugLabel}" );
                return false;
            }

            switch( dx10.dxgiFormat )
            {
                case DXGIFormat.BC7_TYPELESS:
                case DXGIFormat.BC7_UNORM:
                case DXGIFormat.BC7_UNORM_SRGB:
                    format = TextureFormat.BC7;
                    return true;
                case DXGIFormat.BC6H_TYPELESS:
                case DXGIFormat.BC6H_UF16:
                case DXGIFormat.BC6H_SF16:
                    format = TextureFormat.BC6H;
                    return true;
                case DXGIFormat.BC5_TYPELESS:
                case DXGIFormat.BC5_UNORM:
                case DXGIFormat.BC5_SNORM:
                    format = TextureFormat.BC5;
                    return true;
                case DXGIFormat.BC4_TYPELESS:
                case DXGIFormat.BC4_UNORM:
                case DXGIFormat.BC4_SNORM:
                    format = TextureFormat.BC4;
                    return true;
                case DXGIFormat.R8_UNORM:
                    format = TextureFormat.R8;
                    return true;
                case DXGIFormat.R8G8_UNORM:
                    format = TextureFormat.RG16;
                    return true;
                case DXGIFormat.R8G8B8A8_UNORM:
                case DXGIFormat.R8G8B8A8_UNORM_SRGB:
                    format = TextureFormat.RGBA32;
                    return true;
                case DXGIFormat.B8G8R8A8_UNORM:
                case DXGIFormat.B8G8R8A8_UNORM_SRGB:
                    format = TextureFormat.BGRA32;
                    return true;
                case DXGIFormat.R16_UNORM:
                    format = TextureFormat.R16;
                    return true;
                case DXGIFormat.R16_FLOAT:
                    format = TextureFormat.RHalf;
                    return true;
                case DXGIFormat.R16G16_FLOAT:
                    format = TextureFormat.RGHalf;
                    return true;
                case DXGIFormat.R16G16B16A16_FLOAT:
                    format = TextureFormat.RGBAHalf;
                    return true;
                case DXGIFormat.R32_FLOAT:
                    format = TextureFormat.RFloat;
                    return true;
                case DXGIFormat.R32G32_FLOAT:
                    format = TextureFormat.RGFloat;
                    return true;
                case DXGIFormat.R32G32B32A32_FLOAT:
                    format = TextureFormat.RGBAFloat;
                    return true;
            }

            Debug.LogError( $"DX10 format {dx10.dxgiFormat} is not currently supported." );
            return false;
        }
    }
}
