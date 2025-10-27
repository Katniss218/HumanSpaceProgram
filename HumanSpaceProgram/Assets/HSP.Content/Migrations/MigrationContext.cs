using System.Collections.Generic;
using System.IO;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Content.Migrations
{
    public interface IMigrationContext
    {
        /// <summary>
        /// Use this to determine which class of file is being migrated (vessels/saves/etc).
        /// </summary>
        public string RootPath { get; }

        // File operations

        public void RenameFile( string oldPath, string newPath );
        public void MoveFile( string sourcePath, string destPath );
        public void DeleteFile( string path );
        public SerializedData ReadFile( string path );
        public void WriteFile( string path, SerializedData data );

        // Directory operations

        public void CreateDirectory( string path );
        public void DeleteDirectory( string path, bool recursive );
        public IEnumerable<string> GetFiles( string searchPattern, SearchOption options );
        public IEnumerable<string> GetDirectories( string searchPattern, SearchOption options );
    }

    public sealed class MigrationContext : IMigrationContext
    {
        public string RootPath { get; }

        public MigrationContext( string rootPath )
        {
            this.RootPath = rootPath;
        }

        // File operations

        public void RenameFile( string oldPath, string newPath )
        {
            oldPath = Path.GetFullPath( oldPath, RootPath );
            newPath = Path.GetFullPath( newPath, RootPath );
            if( !File.Exists( oldPath ) )
                throw new FileNotFoundException( $"File not found: {oldPath}" );

            File.Move( oldPath, newPath );
        }

        public void MoveFile( string sourcePath, string destPath )
        {
            sourcePath = Path.GetFullPath( sourcePath, RootPath );
            destPath = Path.GetFullPath( destPath, RootPath );
            if( !File.Exists( sourcePath ) )
                throw new FileNotFoundException( $"File not found: {sourcePath}" );

            Directory.CreateDirectory( Path.GetDirectoryName( destPath )! ); // Ensure destination folder exists
            File.Move( sourcePath, destPath );
        }

        public void DeleteFile( string path )
        {
            path = Path.GetFullPath( path, RootPath );
            if( File.Exists( path ) )
                File.Delete( path );
        }

        public SerializedData ReadFile( string path )
        {
            path = Path.GetFullPath( path, RootPath );
            var handler = new JsonSerializedDataHandler( path );
            return handler.Read();
        }

        public void WriteFile( string path, SerializedData data )
        {
            path = Path.GetFullPath( path, RootPath );
            var handler = new JsonSerializedDataHandler( path );
            handler.Write( data );
        }

        // Directory operations

        public void CreateDirectory( string path )
        {
            path = Path.GetFullPath( path, RootPath );
            Directory.CreateDirectory( path );
        }

        public void DeleteDirectory( string path, bool recursive )
        {
            path = Path.GetFullPath( path, RootPath );
            if( Directory.Exists( path ) )
                Directory.Delete( path, recursive );
        }

        public IEnumerable<string> GetFiles( string searchPattern, SearchOption options )
        {
            return Directory.GetFiles( this.RootPath, searchPattern, options );
        }

        public IEnumerable<string> GetDirectories( string searchPattern, SearchOption options )
        {
            return Directory.GetDirectories( this.RootPath, searchPattern, options );
        }
    }
}