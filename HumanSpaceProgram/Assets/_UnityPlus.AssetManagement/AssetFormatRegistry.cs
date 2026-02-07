using System;
using System.Collections.Generic;
using System.IO;

namespace UnityPlus.AssetManagement
{
    public static class AssetFormatRegistry
    {
        private readonly struct AssetSignature
        {
            public readonly byte[] Bytes;
            public readonly int Offset;

            public AssetSignature( byte[] bytes, int offset = 0 )
            {
                Bytes = bytes;
                Offset = offset;
            }
        }

        private static readonly Dictionary<string, AssetFormat> _extToFormat = new( StringComparer.OrdinalIgnoreCase );

        private static readonly Dictionary<string, AssetFormat> _mimeToFormat = new( StringComparer.OrdinalIgnoreCase );

        private static readonly List<(AssetFormat Format, AssetSignature Signature)> _signatures = new();

        private static readonly object _lock = new object();

        static AssetFormatRegistry()
        {
            // Images
            Register( CoreFormats.Png, "image/png", new[] { ".png" }, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } );
            Register( CoreFormats.Jpg, "image/jpeg", new[] { ".jpg", ".jpeg" }, new byte[] { 0xFF, 0xD8, 0xFF } );
            Register( CoreFormats.Tga, "image/tga", new[] { ".tga" } ); // TGA signature is at end of file, complex to detect via head
            Register( CoreFormats.Bmp, "image/bmp", new[] { ".bmp" }, new byte[] { 0x42, 0x4D } );
            Register( CoreFormats.Dds, "image/vnd-ms.dds", new[] { ".dds" }, new byte[] { 0x44, 0x44, 0x53, 0x20 } ); // "DDS "
            Register( CoreFormats.Exr, "image/x-exr", new[] { ".exr" }, new byte[] { 0x76, 0x2f, 0x31, 0x01 } );

            // Audio
            Register( CoreFormats.Wav, "audio/wav", new[] { ".wav" }, new byte[] { 0x52, 0x49, 0x46, 0x46 } ); // "RIFF"
            Register( CoreFormats.Ogg, "audio/ogg", new[] { ".ogg" }, new byte[] { 0x4F, 0x67, 0x67, 0x53 } ); // "OggS"
            Register( CoreFormats.Mp3, "audio/mpeg", new[] { ".mp3" }, new byte[] { 0xFF, 0xFB }, 0 ); // ID3 or Frame sync (simplistic)

            // Models
            Register( CoreFormats.Glb, "model/gltf-binary", new[] { ".glb" }, new byte[] { 0x67, 0x6C, 0x54, 0x46 } ); // "glTF"
            Register( CoreFormats.Gltf, "model/gltf+json", new[] { ".gltf" } );
            Register( CoreFormats.Fbx, "application/octet-stream", new[] { ".fbx" }, new byte[] { 0x4B, 0x61, 0x79, 0x64, 0x61, 0x72, 0x61, 0x20, 0x46, 0x42, 0x58, 0x20, 0x42, 0x69, 0x6E, 0x61, 0x72, 0x79, 0x20, 0x20 } ); // "Kaydara FBX Binary  "
            Register( CoreFormats.Obj, "text/plain", new[] { ".obj" } );

            // Data
            Register( CoreFormats.Json, "application/json", new[] { ".json" } );
            Register( CoreFormats.Xml, "application/xml", new[] { ".xml" } );
            Register( CoreFormats.Yaml, "application/yaml", new[] { ".yaml", ".yml" } );
            Register( CoreFormats.Csv, "text/csv", new[] { ".csv" } );
            Register( CoreFormats.Txt, "text/plain", new[] { ".txt" } );
        }

        private static void Register( AssetFormat format, string mime, string[] extensions, byte[] signature = null, int sigOffset = 0 )
        {
            if( !string.IsNullOrEmpty( mime ) ) RegisterMimeType( mime, format );
            if( extensions != null )
            {
                foreach( var ext in extensions ) RegisterExtension( ext, format );
            }
            if( signature != null ) RegisterSignature( format, signature, sigOffset );
        }

        // ----------------- Registration -----------------

        public static void RegisterExtension( string ext, AssetFormat format )
        {
            if( string.IsNullOrWhiteSpace( ext ) )
                return;

            ext = NormalizeExtension( ext );

            lock( _lock )
                _extToFormat[ext] = format;
        }

        public static void RegisterMimeType( string mimeType, AssetFormat format )
        {
            if( string.IsNullOrWhiteSpace( mimeType ) )
                return;

            mimeType = NormalizeMimeType( mimeType );

            lock( _lock )
                _mimeToFormat[mimeType] = format;
        }

        public static void RegisterSignature( AssetFormat format, byte[] bytes, int offset = 0 )
        {
            if( bytes == null || bytes.Length == 0 )
                return;

            lock( _lock )
                _signatures.Insert( 0, (format, new AssetSignature( bytes, offset )) );
        }

        // ----------------- Resolution -----------------

        public static AssetFormat FromExtension( string ext )
        {
            ext = NormalizeExtension( ext );

            lock( _lock )
            {
                if( _extToFormat.TryGetValue( ext, out var fmt ) )
                    return fmt;
            }

            return AssetFormat.Unknown;
        }

        public static AssetFormat FromMimeType( string mimeType )
        {
            if( string.IsNullOrWhiteSpace( mimeType ) )
                return AssetFormat.Unknown;

            mimeType = NormalizeMimeType( mimeType );

            lock( _lock )
            {
                if( _mimeToFormat.TryGetValue( mimeType, out var fmt ) )
                    return fmt;
            }

            return TryInferFromSubtype( mimeType );
        }

        public static AssetFormat FromByteArray( byte[] buffer )
        {
            if( buffer == null || buffer.Length == 0 )
                return AssetFormat.Unknown;

            lock( _lock )
            {
                foreach( var entry in _signatures )
                {
                    if( MatchSignature( buffer, entry.Signature ) )
                        return entry.Format;
                }
            }

            return AssetFormat.Unknown;
        }

        public static AssetFormat FromStream( Stream stream, int maxBytes = 64 )
        {
            if( stream == null || !stream.CanRead )
                return AssetFormat.Unknown;

            byte[] buffer = new byte[maxBytes];
            int read;

            if( stream.CanSeek )
            {
                long pos = stream.Position;
                read = stream.Read( buffer, 0, buffer.Length );
                stream.Position = pos;
            }
            else
            {
                read = stream.Read( buffer, 0, buffer.Length );
            }

            if( read <= 0 )
                return AssetFormat.Unknown;

            if( read < buffer.Length )
                Array.Resize( ref buffer, read );

            return FromByteArray( buffer );
        }

        // ----------------- Helpers -----------------

        private static bool MatchSignature( byte[] buffer, AssetSignature sig )
        {
            if( buffer.Length < sig.Offset + sig.Bytes.Length )
                return false;

            for( int i = 0; i < sig.Bytes.Length; i++ )
            {
                if( buffer[sig.Offset + i] != sig.Bytes[i] )
                    return false;
            }
            return true;
        }

        private static string NormalizeExtension( string ext )
        {
            ext = ext.Trim();
            if( !ext.StartsWith( ".", StringComparison.Ordinal ) )
                ext = "." + ext;
            return ext.ToUpperInvariant();
        }

        private static string NormalizeMimeType( string mime )
        {
            int semi = mime.IndexOf( ';' );
            if( semi >= 0 )
                mime = mime[..semi];
            return mime.Trim().ToLowerInvariant();
        }

        private static AssetFormat TryInferFromSubtype( string mimeType )
        {
            int slash = mimeType.IndexOf( '/' );
            if( slash < 0 || slash == mimeType.Length - 1 )
                return AssetFormat.Unknown;

            string subtype = mimeType[(slash + 1)..];
            foreach( char c in subtype )
            {
                if( !char.IsLetterOrDigit( c ) && c != '.' && c != '-' )
                    return AssetFormat.Unknown;
            }

            return new AssetFormat( subtype );
        }
    }
}