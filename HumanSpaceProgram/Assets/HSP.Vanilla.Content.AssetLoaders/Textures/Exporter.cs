using System;
using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.Textures
{
    public static class Exporter
    {
        public enum ImageFormat
        {
            PNG,
            JPG,
            TGA,
            EXR
        }

        public static void Export( string filePath, Texture2D texture, ImageFormat format )
        {
            if( texture == null )
                throw new ArgumentNullException( nameof( texture ) );

            string dir = Path.GetDirectoryName( filePath );
            if( !string.IsNullOrEmpty( dir ) && !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            byte[] data = null;

            // Note: Texture must be readable.
            // If the format is not R8G8B8A8 or compatible, ImageConversion might fail or require conversion.
            // We assume the user provides a valid texture.

            switch( format )
            {
                case ImageFormat.PNG:
                    data = ImageConversion.EncodeToPNG( texture );
                    break;
                case ImageFormat.JPG:
                    data = ImageConversion.EncodeToJPG( texture, 90 );
                    break;
                case ImageFormat.TGA:
                    data = ImageConversion.EncodeToTGA( texture );
                    break;
                case ImageFormat.EXR:
                    data = ImageConversion.EncodeToEXR( texture );
                    break;
            }

            if( data != null )
            {
                File.WriteAllBytes( filePath, data );
            }
            else
            {
                Debug.LogError( $"Failed to encode texture to {format}" );
            }
        }
    }
}