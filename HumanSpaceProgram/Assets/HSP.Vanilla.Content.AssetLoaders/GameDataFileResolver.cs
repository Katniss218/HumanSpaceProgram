using HSP.Content;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content
{
    public class GameDataFileResolver : IAssetResolver
    {
        public const string REGISTER_FILE_RESOLVER = HSPEvent.NAMESPACE_HSP + ".gdas.register_file_resolver";
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, REGISTER_FILE_RESOLVER )]
        private static void RegisterFileResolver()
        {
            GameDataFileResolver fileResolver = new GameDataFileResolver();
            fileResolver.IndexGameData();
            AssetRegistry.RegisterResolver( fileResolver );
        }

        public const string TOPOLOGICAL_ID = "hsp.gamedatafileresolver";

        // Maps ID -> List of Paths (e.g. "Mod::Item" -> [".../Item.png", ".../Item.json"])
        private readonly ConcurrentDictionary<string, List<string>> _idToPaths = new ConcurrentDictionary<string, List<string>>();

        public string ID => TOPOLOGICAL_ID;
        public string[] Before => null;
        public string[] After => null;
        public string[] Blacklist => null;

        public void IndexGameData()
        {
            string root = HumanSpaceProgramContent.GetContentDirectoryPath();
            if( !Directory.Exists( root ) ) 
                return;

            string[] files = Directory.GetFiles( root, "*", SearchOption.AllDirectories );

            // Create a HashSet for fast lookups to check if a "Parent" file exists.
            // This is used to skip sidecars (e.g. if 'A.png' exists, 'A.png.json' is skipped).
            HashSet<string> fileSet = new( files );

            foreach( string file in files )
            {
                string name = Path.GetFileName( file );

                // 1. Skip system files and specific metadata files
                if( name.StartsWith( "." ) || name == "ModManifest.json" || name == "_mod.json" || name.EndsWith( ".meta" ) )
                    continue;

                // 2. Skip sidecars
                // If this file is "Image.png.json", check if "Image.png" exists.
                // We use Path.ChangeExtension(..., null) to strip the last extension.
                string potentialParent = Path.ChangeExtension( file, null );

                // If the parent file exists in our set, we treat the current file as a sidecar and do NOT index it as a standalone asset.
                if( fileSet.Contains( potentialParent ) )
                    continue;

                void AddToIndex( string assetId, string filePath )
                {
                    _idToPaths.AddOrUpdate( assetId,
                        ( k ) => new List<string> { filePath },
                        ( k, list ) => {
                            lock( list )
                            {
                                if( !list.Contains( filePath ) ) list.Add( filePath );
                            }
                            return list;
                        } );
                }

                // 3. Register Base ID (ModID::RelativePath without extension)
                string baseId = HumanSpaceProgramContent.GetAssetID( file );
                AddToIndex( baseId, file );

                // 4. Register Explicit ID (ModID::RelativePath with extension)
                if( HumanSpaceProgramContent.GetModDirectoryFromAssetPath( file, out string modId ) != null )
                {
                    string relPath = Path.GetRelativePath( HumanSpaceProgramContent.GetModDirectory( modId ), file ).Replace( "\\", "/" );
                    string idWithExt = $"{modId}::{relPath}";

                    if( idWithExt != baseId )
                    {
                        AddToIndex( idWithExt, file );
                    }
                }
            }

            Debug.Log( $"[GameDataFileResolver] Indexed {_idToPaths.Count} asset IDs." );
        }

        public bool CanResolve( AssetUri uri, Type targetType )
        {
            return _idToPaths.ContainsKey( uri.BaseID );
        }

        public Task<IEnumerable<AssetDataHandle>> ResolveAsync( AssetUri uri, CancellationToken ct )
        {
            if( _idToPaths.TryGetValue( uri.BaseID, out List<string> paths ) )
            {
                // Return handles for ALL files associated with this ID.
                // The Registry will check which one works with the requested Type via Loaders.
                // We lock the list copy to be thread safe.
                List<string> pathCopy;
                lock( paths ) 
                    pathCopy = new List<string>( paths );

                IEnumerable<AssetDataHandle> handles = pathCopy.Select( p => (AssetDataHandle)new FileAssetDataHandle( p ) );
                return Task.FromResult( handles );
            }

            return Task.FromResult<IEnumerable<AssetDataHandle>>( null );
        }
    }
}