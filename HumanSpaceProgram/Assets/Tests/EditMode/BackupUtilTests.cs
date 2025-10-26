using HSP.Content;
using NUnit.Framework;
using System.IO;
using System;
using System.Linq;

[TestFixture]
public class BackupUtilTests
{
    private string _tempRootDirectory;

    [SetUp]
    public void SetUp()
    {
        _tempRootDirectory = Path.Combine( Path.GetTempPath(), "HSP_BackupTests_" + Guid.NewGuid().ToString( "N" ) );
        Directory.CreateDirectory( _tempRootDirectory );
    }

    [TearDown]
    public void TearDown()
    {
        if( Directory.Exists( _tempRootDirectory ) )
        {
            // best-effort clear read-only bits then delete
            SetAttributesNormalRecursive( _tempRootDirectory );
            Directory.Delete( _tempRootDirectory, recursive: true );
        }
    }

    [Test]
    public void BackupAndRestore_RecreatesFilesAndHashesMatch()
    {
        // Arrange
        string source = Path.Combine( _tempRootDirectory, "source" );
        Directory.CreateDirectory( source );

        string subdir = Path.Combine( source, "sub" );
        Directory.CreateDirectory( subdir );

        string f1 = Path.Combine( source, "a.txt" );
        string f2 = Path.Combine( subdir, "b.bin" );

        File.WriteAllText( f1, "hello world" );
        File.WriteAllBytes( f2, new byte[] { 1, 2, 3, 4, 5 } );

        // compute hashes before backup (so we can compare after restore)
        string h1_before = BackupUtil.ComputeFileHash( f1 );
        string h2_before = BackupUtil.ComputeFileHash( f2 );

        string backupDir = Path.Combine( _tempRootDirectory, "backups" );
        Directory.CreateDirectory( backupDir );

        // Act - create backup
        BackupUtil.BackupDirectory( source, backupDir );

        // find the produced zip file (there should be exactly one)
        var zips = Directory.GetFiles( backupDir, "*.zip", SearchOption.TopDirectoryOnly );
        Assert.That( zips.Length, Is.GreaterThanOrEqualTo( 1 ), "Expected at least one zip in backup dir" );
        string zipPath = zips.OrderByDescending( File.GetCreationTimeUtc ).First();

        // remove source directory to simulate loss
        SetAttributesNormalRecursive( source );
        Directory.Delete( source, recursive: true );
        Assert.False( Directory.Exists( source ) );

        // Act - restore
        BackupUtil.RestoreBackup( zipPath, source );

        // Assert - files exist and hashes match
        string f1_after = Path.Combine( source, "a.txt" );
        string f2_after = Path.Combine( source, "sub", "b.bin" );

        Assert.True( File.Exists( f1_after ), "a.txt should exist after restore" );
        Assert.True( File.Exists( f2_after ), "sub/b.bin should exist after restore" );

        string h1_after = BackupUtil.ComputeFileHash( f1_after );
        string h2_after = BackupUtil.ComputeFileHash( f2_after );

        Assert.That( h1_after, Is.EqualTo( h1_before ), "File hash for a.txt must match original" );
        Assert.That( h2_after, Is.EqualTo( h2_before ), "File hash for b.bin must match original" );
    }

    [Test]
    public void Restore_ReplacesExistingDirectory()
    {
        // Arrange: original source content
        string original = Path.Combine( _tempRootDirectory, "original" );
        Directory.CreateDirectory( original );
        File.WriteAllText( Path.Combine( original, "keep.txt" ), "original content" );
        File.WriteAllText( Path.Combine( original, "willbeoverwritten.txt" ), "original" );

        string backupDir = Path.Combine( _tempRootDirectory, "backups2" );
        Directory.CreateDirectory( backupDir );
        BackupUtil.BackupDirectory( original, backupDir );

        var zip = Directory.GetFiles( backupDir, "*.zip", SearchOption.TopDirectoryOnly ).OrderByDescending( File.GetCreationTimeUtc ).First();

        // Create an existing restore path with different contents
        string restorePath = Path.Combine( _tempRootDirectory, "restoreTarget" );
        Directory.CreateDirectory( restorePath );
        File.WriteAllText( Path.Combine( restorePath, "some_old_file.txt" ), "old" );
        File.WriteAllText( Path.Combine( restorePath, "willbeoverwritten.txt" ), "different" );

        // Sanity: file that should be gone exists before restore
        Assert.True( File.Exists( Path.Combine( restorePath, "some_old_file.txt" ) ) );

        // Act
        BackupUtil.RestoreBackup( zip, restorePath );

        // Assert: old file removed, original files restored
        Assert.False( File.Exists( Path.Combine( restorePath, "some_old_file.txt" ) ), "Old files in restore target must be removed by restore" );
        Assert.True( File.Exists( Path.Combine( restorePath, "keep.txt" ) ), "keep.txt must be restored" );
        Assert.True( File.Exists( Path.Combine( restorePath, "willbeoverwritten.txt" ) ), "willbeoverwritten.txt must be restored" );

        // Verify contents match original by hash
        string originalKeepHash = BackupUtil.ComputeFileHash( Path.Combine( original, "keep.txt" ) );
        string restoredKeepHash = BackupUtil.ComputeFileHash( Path.Combine( restorePath, "keep.txt" ) );
        Assert.That( restoredKeepHash, Is.EqualTo( originalKeepHash ) );
    }

    [Test]
    public void BackupDirectory_DoesNotOverwriteExistingZip_AddsSuffix()
    {
        // Arrange
        string source = Path.Combine( _tempRootDirectory, "src_for_dup" );
        Directory.CreateDirectory( source );
        File.WriteAllText( Path.Combine( source, "x.txt" ), "data" );

        string backupDir = Path.Combine( _tempRootDirectory, "backups3" );
        Directory.CreateDirectory( backupDir );

        // Act - create two backups in quick succession
        BackupUtil.BackupDirectory( source, backupDir );
        // ensure the timestamp might be the same in some envs; call a second time
        BackupUtil.BackupDirectory( source, backupDir );

        // Assert - at least two zip files exist and names are distinct
        var zips = Directory.GetFiles( backupDir, "*.zip", SearchOption.TopDirectoryOnly );
        Assert.That( zips.Length, Is.GreaterThanOrEqualTo( 2 ), "Expected at least 2 zip files after two backups" );

        var names = zips.Select( Path.GetFileName ).ToArray();
        Assert.That( names.Distinct().Count(), Is.EqualTo( names.Length ), "Zip filenames should be distinct" );
    }

    [Test]
    public void RestoreBackup_MissingZip_ThrowsFileNotFoundException()
    {
        string missing = Path.Combine( _tempRootDirectory, "no_such_file.zip" );
        string restorePath = Path.Combine( _tempRootDirectory, "restore_missing" );

        var ex = Assert.Throws<FileNotFoundException>( () => BackupUtil.RestoreBackup( missing, restorePath ) );
        Assert.That( ex.Message, Does.Contain( "Backup file not found" ) );
    }


    private static void SetAttributesNormalRecursive( string path )
    {
        if( string.IsNullOrEmpty( path ) || !Directory.Exists( path ) ) return;

        foreach( var file in Directory.GetFiles( path, "*", SearchOption.AllDirectories ) )
        {
            try { File.SetAttributes( file, FileAttributes.Normal ); } catch { }
        }

        foreach( var dir in Directory.GetDirectories( path, "*", SearchOption.AllDirectories ) )
        {
            try { var d = new DirectoryInfo( dir ); d.Attributes = FileAttributes.Normal; } catch { }
        }

        try { var root = new DirectoryInfo( path ); root.Attributes = FileAttributes.Normal; } catch { }
    }
}