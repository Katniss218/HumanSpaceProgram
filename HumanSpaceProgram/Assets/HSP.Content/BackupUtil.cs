using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace HSP.Content
{
    public class BackupUtil
    {
        public static string ComputeFileHash( string filePath, string algorithmName = "SHA256" )
        {
            if( string.IsNullOrWhiteSpace( filePath ) )
                throw new ArgumentException( "filePath must be a non-empty path.", nameof( filePath ) );

            if( !File.Exists( filePath ) )
                throw new FileNotFoundException( $"File not found: {filePath}" );

            try
            {
                using FileStream stream = File.OpenRead( filePath );

                using HashAlgorithm algorithm = HashAlgorithm.Create( algorithmName )
                    ?? throw new ArgumentException( $"Unsupported hash algorithm: {algorithmName}", nameof( algorithmName ) );

                byte[] hashBytes = algorithm.ComputeHash( stream );

                // Convert the hash bytes to lowercase hex string
                StringBuilder sb = new StringBuilder( hashBytes.Length * 2 );
                foreach( byte b in hashBytes )
                {
                    sb.Append( b.ToString( "x2" ) );
                }

                return sb.ToString();
            }
            catch( Exception ex )
            {
                throw new IOException( $"Failed to compute hash for file '{filePath}'. See inner exception for details.", ex );
            }
        }

        public static string ComputeDirectoryHash( string directoryPath, string algorithmName = "SHA256" )
        {
            if( string.IsNullOrWhiteSpace( directoryPath ) )
                throw new ArgumentException( "directoryPath must be a non-empty path.", nameof( directoryPath ) );

            if( !Directory.Exists( directoryPath ) )
                throw new DirectoryNotFoundException( $"Directory not found: {directoryPath}" );

            try
            {
                using HashAlgorithm algorithm = HashAlgorithm.Create( algorithmName )
                    ?? throw new ArgumentException( $"Unsupported hash algorithm: {algorithmName}", nameof( algorithmName ) );

                // Enumerate all files recursively and sort by full path for deterministic hash combination
                IEnumerable<string> files = Directory.EnumerateFiles( directoryPath, "*", SearchOption.AllDirectories )
                    .OrderBy( f => f );

                const byte delimiter = 0; // Null byte as delimiter between path and content

                foreach( string filePath in files )
                {
                    // Include the file name in the hash.
                    string relativePath = Path.GetRelativePath( directoryPath, filePath ).Replace( '\\', '/' );
                    byte[] pathBytes = Encoding.UTF8.GetBytes( relativePath );
                    algorithm.TransformBlock( pathBytes, 0, pathBytes.Length, null, 0 );

                    algorithm.TransformBlock( new[] { delimiter }, 0, 1, null, 0 );

                    // Stream the file content
                    using FileStream stream = File.OpenRead( filePath );
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while( (bytesRead = stream.Read( buffer, 0, buffer.Length )) > 0 )
                    {
                        algorithm.TransformBlock( buffer, 0, bytesRead, null, 0 );
                    }
                }

                // Finalize the hash
                algorithm.TransformFinalBlock( new byte[0], 0, 0 );
                byte[] hashBytes = algorithm.Hash;

                // Convert the hash bytes to lowercase hex string
                StringBuilder sb = new StringBuilder( hashBytes.Length * 2 );
                foreach( byte b in hashBytes )
                {
                    sb.Append( b.ToString( "x2" ) );
                }

                return sb.ToString();
            }
            catch( Exception ex )
            {
                throw new IOException( $"Failed to compute hash for directory '{directoryPath}'. See inner exception for details.", ex );
            }
        }

        /// <summary>
        /// Retrieves the path to the latest backup ZIP file in <paramref name="backupPath"/> for the given <paramref name="sourceDirName"/>.
        /// </summary>
        /// <remarks>
        /// Backup files are expected to follow the naming pattern `{sourceDirName}_{yyyyMMdd_HHmmss}.zip` <br/>
        /// or `{sourceDirName}_{yyyyMMdd_HHmmss}_{counter}.zip` for duplicates created in the same timestamp. <br/>
        /// The latest file is determined first by the parsed timestamp (most recent), and ties are broken by the file's last write time. <br/>
        /// Returns <c>null</c> if no matching backup files are found.
        /// </remarks>
        /// <param name="backupPath">Directory containing backup ZIP files (must exist).</param>
        /// <returns>The full path to the latest backup ZIP file, or <c>null</c> if none found.</returns>
        public static string GetLatestBackupFile( string sourcePath, string backupPath )
        {
            if( string.IsNullOrWhiteSpace( backupPath ) )
                throw new ArgumentException( "backupPath must be a non-empty path.", nameof( backupPath ) );

            if( !Directory.Exists( backupPath ) )
                return null;

            string[] files = Directory.GetFiles( backupPath, "*.zip" );
            string sourceDirName = Path.GetFileName( sourcePath );

            if( files.Length == 0 )
                return null;

            var regex = new Regex( $"^{sourceDirName}" + @"_(\d{8}_\d{6})(_(\d+))*$" );

            DateTime latestDt = DateTime.MinValue;
            DateTime latestFileTime = DateTime.MinValue;
            string latestFile = null;

            foreach( string file in files )
            {
                string fileName = Path.GetFileNameWithoutExtension( file );

                Match match = regex.Match( fileName );
                if( !match.Success )
                    continue;

                string timestampStr = match.Groups[1].Value;

                if( !DateTime.TryParseExact( timestampStr, "yyyyMMdd_HHmmss", null, DateTimeStyles.None, out DateTime dt ) )
                    continue;

                FileInfo fileInfo = new FileInfo( file );
                DateTime fileTime = fileInfo.LastWriteTime;

                if( dt > latestDt || (dt == latestDt && fileTime > latestFileTime) )
                {
                    latestDt = dt;
                    latestFileTime = fileTime;
                    latestFile = file;
                }
            }

            return latestFile;
        }

        /// <summary>
        /// Zip the directory at <paramref name="sourcePath"/> and place the zip into <paramref name="backupPath"/>.
        /// </summary>
        /// <remarks>
        /// The produced file will be named `{sourceDirName}_{yyyyMMdd_HHmmss}.zip` <br/>
        /// If a file with that name already exists, a numeric suffix like "_1", "_2", ... will be added.
        /// </remarks>
        /// <param name="sourcePath">Directory to back up (must exist).</param>
        /// <param name="backupPath">Directory where zip file will be placed (will be created if missing).</param>
        public static void BackupDirectory( string sourcePath, string backupPath )
        {
            if( string.IsNullOrWhiteSpace( sourcePath ) )
                throw new ArgumentException( "sourcePath must be a non-empty path.", nameof( sourcePath ) );
            if( string.IsNullOrWhiteSpace( backupPath ) )
                throw new ArgumentException( "backupPath must be a non-empty path.", nameof( backupPath ) );

            if( !Directory.Exists( sourcePath ) )
                throw new DirectoryNotFoundException( $"Source directory not found: {sourcePath}" );

            Directory.CreateDirectory( backupPath );

            if( sourcePath.EndsWith( Path.DirectorySeparatorChar ) )
            {
                sourcePath = sourcePath.TrimEnd( Path.DirectorySeparatorChar );
            }

            string sourceDirName = Path.GetFileName( sourcePath );
            if( string.IsNullOrEmpty( sourceDirName ) )
                sourceDirName = "backup";

            string timestamp = DateTime.Now.ToString( "yyyyMMdd_HHmmss" );

            string baseFileName = $"{sourceDirName}_{timestamp}";
            string fileName = baseFileName + ".zip";
            string destPath = Path.Combine( backupPath, fileName );

            // Check if the file already exists, avoid overwriting by adding a numeric suffix.
            int counter = 1;
            while( File.Exists( destPath ) )
            {
                fileName = $"{baseFileName}_{counter}.zip";
                destPath = Path.Combine( backupPath, fileName );
                counter++;
            }

            try
            {
                ZipFile.CreateFromDirectory( sourcePath, destPath, CompressionLevel.Optimal, includeBaseDirectory: false );
            }
            catch( Exception ex )
            {
                throw new IOException( $"Failed to create backup zip '{destPath}'. See inner exception for details.", ex );
            }
        }

        /// <summary>
        /// Restore a backup zip to <paramref name="restorePath"/>. The zip contents are first extracted to a temporary
        /// directory, then the existing <paramref name="restorePath"/> (if any) is removed and replaced with the extracted contents.
        /// </summary>
        /// <param name="backupZipPath">Full path to the backup zip file.</param>
        /// <param name="restorePath">Directory path that will be replaced by the restored files.</param>
        public static void RestoreBackup( string backupZipPath, string restorePath )
        {
            if( string.IsNullOrWhiteSpace( backupZipPath ) )
                throw new ArgumentException( "backupZipPath must be a non-empty path.", nameof( backupZipPath ) );
            if( string.IsNullOrWhiteSpace( restorePath ) )
                throw new ArgumentException( "restorePath must be a non-empty path.", nameof( restorePath ) );

            if( !File.Exists( backupZipPath ) )
                throw new FileNotFoundException( $"Backup file not found: {backupZipPath}" );

            // Create a unique temporary extraction directory
            string tempDir = Path.Combine( Path.GetTempPath(), "HSP_Restore_" + Guid.NewGuid().ToString( "N" ) );

            try
            {
                Directory.CreateDirectory( tempDir );
                ZipFile.ExtractToDirectory( backupZipPath, tempDir );

                // Ensure parent of restorePath exists (if restorePath is like ".../MyDir", ensure its parent exists).
                string restoreParent = Path.GetDirectoryName( restorePath );
                if( !string.IsNullOrEmpty( restoreParent ) && !Directory.Exists( restoreParent ) )
                    Directory.CreateDirectory( restoreParent );

                if( Directory.Exists( restorePath ) )
                {
                    Directory.Delete( restorePath, recursive: true );
                }

                CopyDirectory( tempDir, restorePath );
            }
            catch( Exception ex )
            {
                throw new IOException( $"Failed to restore backup '{backupZipPath}' to '{restorePath}'. See inner exception for details.", ex );
            }
            finally
            {
                // Cleanup tempDir if it still exists (i.e. if move didn't happen).
                if( !string.IsNullOrEmpty( tempDir ) && Directory.Exists( tempDir ) )
                {
                    Directory.Delete( tempDir, recursive: true );
                }
            }
        }


        private static void CopyDirectory( string sourceDir, string destDir )
        {
            if( !Directory.Exists( sourceDir ) )
                return;

            if( !Directory.Exists( destDir ) )
                Directory.CreateDirectory( destDir );

            // Copy all files
            foreach( string filePath in Directory.GetFiles( sourceDir ) )
            {
                string destFilePath = Path.Combine( destDir, Path.GetFileName( filePath ) );
                File.Copy( filePath, destFilePath, overwrite: true );
            }

            // Copy all subdirectories recursively
            foreach( string subDirPath in Directory.GetDirectories( sourceDir ) )
            {
                string destSubDirPath = Path.Combine( destDir, Path.GetFileName( subDirPath ) );
                CopyDirectory( subDirPath, destSubDirPath );
            }
        }
    }
}