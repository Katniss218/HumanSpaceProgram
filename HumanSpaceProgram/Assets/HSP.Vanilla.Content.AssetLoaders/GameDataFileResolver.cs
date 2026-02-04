using HSP.Content;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class GameDataFileResolver : IAssetResolver
    {
        public const string TOPOLOGICAL_ID = "hsp.gamedatafileresolver";

        private readonly ConcurrentDictionary<string, string> _idToPath = new ConcurrentDictionary<string, string>();

        public string ID => TOPOLOGICAL_ID;
        public string[] Before => null;
        public string[] After => null;
        public string[] Blacklist => null;

        public static bool ShouldIgnoreFile( string filePath )
        {
            string name = Path.GetFileName( filePath );
            return name.StartsWith( "." ) || name == ModMetadata.MOD_MANIFEST_FILENAME;
        }

        public void IndexGameData()
        {
            string root = HumanSpaceProgramContent.GetContentDirectoryPath();
            // Get all files recursively
            string[] files = Directory.GetFiles( root, "*", SearchOption.AllDirectories );

            foreach( string file in files )
            {
                // Skip manifests and other 'ignored' files.
                if( ShouldIgnoreFile( file ) )
                    continue;

                // Generate ID: ModID::RelativePath
                // Index the base ID (without extension) if unique
                 string id = HumanSpaceProgramContent.GetAssetID( file );
                if( !_idToPath.TryAdd( id, file ) )
                {
                    // If ID collision (e.g. tex.png vs tex.json), we might want logic here.
                    // For now, first wins for the base ID.
                }

                if( HumanSpaceProgramContent.GetModDirectoryFromAssetPath( file, out string modId ) != null )
                {
                    // RelPath relative to mod folder
                    string relPath = Path.GetRelativePath( HumanSpaceProgramContent.GetModDirectory( modId ), file ).Replace( "\\", "/" );
                    string idWithExt = $"{modId}::{relPath}";
                    _idToPath.TryAdd( idWithExt, file );
                }
            }

            Debug.Log( $"[GameDataFileResolver] Indexed {_idToPath.Count} asset IDs." );
        }

        public bool CanResolve( AssetUri uri, Type targetType )
        {
            // We resolve against the BaseId (ModId::Path).
            // This ignores any query parameters (e.g. ?size=4), allowing this resolver to find the file even if the user appended params for a loader down the line.

            // Do not check targetType here, because this resolver just maps ID -> FilePath.
            // The validity of the file content vs requested type is checked by the Loaders.

            return _idToPath.ContainsKey( uri.BaseID );
        }

        public Task<AssetDataHandle> ResolveAsync( AssetUri uri, Type targetType, CancellationToken ct )
        {
            if( _idToPath.TryGetValue( uri.BaseID, out string path ) )
            {
                return Task.FromResult<AssetDataHandle>( new FileAssetDataHandle( path ) );
            }
            return Task.FromResult<AssetDataHandle>( null );
        }
    }
}