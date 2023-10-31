using KSS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

namespace KSS.AssetLoaders.GameData
{
    public static class JsonPartLoader
    {
        const string OBJECTS_SUFFIX = "_o.json";
        const string DATA_SUFFIX = "_d.json";

        static JsonPartStrategy _strat = new JsonPartStrategy(); // this can be used to save a part too.
        static Loader _loader = new Loader( null, null, _strat.Load_Object, _strat.Load_Data );

        private static IEnumerable<string> GroupFiles( IEnumerable<string> files, params string[] suffixes )
        {
            Dictionary<string, int> partGroups = new Dictionary<string, int>();

            foreach( var file in files )
            {
                foreach( var suffix in suffixes )
                {
                    if( file.EndsWith( suffix ) )
                    {
                        string fileBase = file[..(OBJECTS_SUFFIX.Length)];
                        if( partGroups.TryGetValue( fileBase, out var count ) )
                        {
                            partGroups[fileBase] = count + 1;
                        }
                        else
                        {
                            partGroups[fileBase] = 1;
                        }
                    }
                }
            }

            List<string> accepted = new List<string>();
            foreach( var kvp in partGroups )
            {
                if( kvp.Value == suffixes.Length )
                {
                    accepted.Add( kvp.Key );
                }
            }

            return accepted;
        }

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".load_parts" )]
        private static void OnStartup( object e )
        {
            string gameDataPath = HumanSpaceProgram.GetGameDataDirectoryPath();
            string[] jsonFiles = Directory.GetFiles( gameDataPath, "*.json", SearchOption.AllDirectories );

            IEnumerable<string> baseFiles = GroupFiles( jsonFiles, OBJECTS_SUFFIX, DATA_SUFFIX );

            // register a loader for each part.
            foreach( var baseFile in baseFiles )
            {
                string pathRelativeToGameData = Path.GetRelativePath( gameDataPath, baseFile );
                AssetRegistry.RegisterLazy( $"gamedata::{pathRelativeToGameData}", () =>
                {
                    _strat.ObjectsFilename = baseFile + OBJECTS_SUFFIX;
                    _strat.DataFilename = baseFile + DATA_SUFFIX;
                    _loader.Load();
                    return _strat.LastSpawnedRoot;
                }, isCacheable: false );
            }
        }
    }
}