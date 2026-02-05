using System;
using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    public static class Exporter
    {
        private const uint DDS_MAGIC = 0x20534444; // "DDS "

        // Flags
        private const uint DDSD_CAPS = 0x1;
        private const uint DDSD_HEIGHT = 0x2;
        private const uint DDSD_WIDTH = 0x4;
        private const uint DDSD_PIXELFORMAT = 0x1000;
        private const uint DDSD_MIPMAPCOUNT = 0x20000;
        private const uint DDSD_LINEARSIZE = 0x80000;
        private const uint DDSD_PITCH = 0x8;

        private const uint DDPF_ALPHAPIXELS = 0x1;
        private const uint DDPF_FOURCC = 0x4;
        private const uint DDPF_RGB = 0x40;

        private const uint DDSCAPS_Texture = 0x1000;
        private const uint DDSCAPS_Mipmap = 0x400000;
        private const uint DDSCAPS_Complex = 0x8;

        // Formats
        private const uint FOURCC_DXT1 = 0x31545844;
        private const uint FOURCC_DXT5 = 0x35545844;
        private const uint FOURCC_DX10 = 0x30315844;

        public static void Export( string filePath, Texture2D texture )
        {
            if( texture == null )
                throw new ArgumentNullException( nameof( texture ) );

            string dir = Path.GetDirectoryName( filePath );
            if( !string.IsNullOrEmpty( dir ) && !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            using FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write );
            using BinaryWriter bw = new BinaryWriter( fs );

            bw.Write( DDS_MAGIC );

            // 2. Prepare Header Data
            uint height = (uint)texture.height;
            uint width = (uint)texture.width;
            uint mipMapCount = (uint)texture.mipmapCount;
            bool hasMips = mipMapCount > 1;

            uint dwFlags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT;
            if( hasMips )
                dwFlags |= DDSD_MIPMAPCOUNT | DDSD_PIXELFORMAT;

            // Determine Format
            uint pfFlags = 0;
            uint fourCC = 0;
            uint rgbBitCount = 0;
            uint rMask = 0, gMask = 0, bMask = 0, aMask = 0;

            bool writeDX10Header = false;
            DXGIFormat dxgiFormat = DXGIFormat.UNKNOWN;

            // Try to map Unity TextureFormat to DDS
            switch( texture.format )
            {
                case TextureFormat.DXT1:
                    pfFlags = DDPF_FOURCC;
                    fourCC = FOURCC_DXT1;
                    dwFlags |= DDSD_LINEARSIZE;
                    break;
                case TextureFormat.DXT5:
                    pfFlags = DDPF_FOURCC;
                    fourCC = FOURCC_DXT5;
                    dwFlags |= DDSD_LINEARSIZE;
                    break;
                case TextureFormat.BC7:
                    pfFlags = DDPF_FOURCC;
                    fourCC = FOURCC_DX10;
                    writeDX10Header = true;
                    dxgiFormat = DXGIFormat.BC7_UNORM;
                    dwFlags |= DDSD_LINEARSIZE;
                    break;
                case TextureFormat.BC5:
                    pfFlags = DDPF_FOURCC;
                    fourCC = FOURCC_DX10;
                    writeDX10Header = true;
                    dxgiFormat = DXGIFormat.BC5_UNORM;
                    dwFlags |= DDSD_LINEARSIZE;
                    break;
                case TextureFormat.BC4:
                    pfFlags = DDPF_FOURCC;
                    fourCC = FOURCC_DX10;
                    writeDX10Header = true;
                    dxgiFormat = DXGIFormat.BC4_UNORM;
                    dwFlags |= DDSD_LINEARSIZE;
                    break;
                case TextureFormat.RGBA32:
                    pfFlags = DDPF_RGB | DDPF_ALPHAPIXELS;
                    rgbBitCount = 32;
                    rMask = 0x000000FF;
                    gMask = 0x0000FF00;
                    bMask = 0x00FF0000;
                    aMask = 0xFF000000;
                    dwFlags |= DDSD_PITCH;
                    break;
                case TextureFormat.RGB24:
                    pfFlags = DDPF_RGB;
                    rgbBitCount = 24;
                    rMask = 0x000000FF;
                    gMask = 0x0000FF00;
                    bMask = 0x00FF0000;
                    dwFlags |= DDSD_PITCH;
                    break;
                default:
                    // If unknown, fallback to writing as DX10 R8G8B8A8 if possible, or fail.
                    // For now we assume the texture data is raw and uncompressed RGBA32 if not matched above.
                    // Note: This relies on GetRawTextureData returning RGBA32 which isn't guaranteed if the source format is different.
                    // Ideally we should use ImageConversion to get a known format, but DDS requires raw block data.
                    Debug.LogWarning( $"DDS Exporter: Format {texture.format} defaults to uncompressed RGBA32 via legacy header." );
                    pfFlags = DDPF_RGB | DDPF_ALPHAPIXELS;
                    rgbBitCount = 32;
                    rMask = 0x000000FF;
                    gMask = 0x0000FF00;
                    bMask = 0x00FF0000;
                    aMask = 0xFF000000;
                    dwFlags |= DDSD_PITCH;
                    break;
            }

            uint pitchOrLinearSize = 0;
            if( (pfFlags & DDPF_FOURCC) != 0 )
            {
                // For compressed formats, linear size = max(1, width/4) * max(1, height/4) * blockSize
                // DXT1/BC4 = 8 bytes per block. DXT5/BC5/BC7 = 16 bytes per block.
                uint blockSize = (fourCC == FOURCC_DXT1 || dxgiFormat == DXGIFormat.BC4_UNORM) ? 8u : 16u;
                pitchOrLinearSize = Math.Max( 1, (width + 3) / 4 ) * Math.Max( 1, (height + 3) / 4 ) * blockSize;
            }
            else
            {
                // Pitch = (width * bpp + 7) / 8
                pitchOrLinearSize = (width * rgbBitCount + 7) / 8;
            }

            bw.Write( (uint)124 ); // dwSize
            bw.Write( dwFlags );
            bw.Write( height );
            bw.Write( width );
            bw.Write( pitchOrLinearSize );
            bw.Write( (uint)0 ); // dwDepth

            bw.Write( mipMapCount );
            for( int i = 0; i < 11; i++ )
            {
                bw.Write( (uint)0 ); // Reserved
            }

            bw.Write( (uint)32 ); // Size
            bw.Write( pfFlags );
            bw.Write( fourCC );
            bw.Write( rgbBitCount );
            bw.Write( rMask );
            bw.Write( gMask );
            bw.Write( bMask );
            bw.Write( aMask );

            uint caps = DDSCAPS_Texture;
            if( hasMips )
                caps |= DDSCAPS_Mipmap | DDSCAPS_Complex;
            bw.Write( caps );

            bw.Write( (uint)0 ); // Caps2
            bw.Write( (uint)0 ); // Caps3
            bw.Write( (uint)0 ); // Caps4
            bw.Write( (uint)0 ); // Reserved

            if( writeDX10Header )
            {
                bw.Write( (uint)dxgiFormat );
                bw.Write( (uint)3 ); // D3D10_RESOURCE_DIMENSION_TEXTURE2D
                bw.Write( (uint)0 ); // miscFlag
                bw.Write( (uint)1 ); // arraySize
                bw.Write( (uint)0 ); // miscFlags2
            }

            // 5. Write Data
            // We assume the Texture2D is readable or provides raw data.
            // If the texture is compressed in memory, GetRawTextureData() returns compressed bytes.
            // If uncompressed, it returns pixel bytes.
            // NOTE: This requires the texture to be readable in settings if calling from Runtime.
            byte[] data = texture.GetRawTextureData();
            bw.Write( data );
        }
    }
}