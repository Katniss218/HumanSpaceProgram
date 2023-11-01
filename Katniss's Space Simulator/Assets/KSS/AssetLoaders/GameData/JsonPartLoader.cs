using KSS.Core;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;

namespace KSS.AssetLoaders.GameData
{
    public static class JsonPartLoader
    {
        static JsonSingleExplicitHierarchyStrategy _strat = new JsonSingleExplicitHierarchyStrategy( () => throw new NotSupportedException( $"Tried to save something using a part *loader*" ) ); // this can be used to save a part too.
        static Loader _loader = new Loader( null, null, _strat.Load_Object, _strat.Load_Data );

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".load_parts" )]
        private static void OnStartup( object e )
        {
            // <mod_folder>/Parts/<part_id>/objects.json, data.json, _part.json
            string gameDataPath = HumanSpaceProgram.GetGameDataDirectoryPath();
            string[] modDirectories = Directory.GetDirectories( gameDataPath );

            List<string> partDirectories = new List<string>();
            foreach( var modDirectory in modDirectories )
            {
                string partsDir = Path.Combine( modDirectory, "Parts" );
                if( Directory.Exists( partsDir ) )
                    partDirectories.AddRange( Directory.GetDirectories( partsDir ) );
            }

            // register a loader for each part.
            foreach( var partPath in partDirectories )
            {
                PartMetadata partMeta = new PartMetadata( partPath );
                partMeta.ReadDataFromDisk();
                AssetRegistry.Register( $"part::m/{partMeta.ID}", partMeta );
                AssetRegistry.RegisterLazy( $"part::h/{partMeta.ID}", () =>
                {
                    _strat.ObjectsFilename = Path.Combine( partPath, "objects.json" );
                    _strat.DataFilename = Path.Combine( partPath, "data.json" );
                    _loader.Load();
                    return _strat.LastSpawnedRoot;
                }, isCacheable: false );
            }
        }
    }
}