using System;
using System.IO;
using System.IO.Compression;

namespace HSP.Content
{
    public class BackupUtil
    {
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
    }
}