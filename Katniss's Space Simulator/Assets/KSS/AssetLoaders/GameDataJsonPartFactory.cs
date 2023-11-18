using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Strategies;

namespace KSS.AssetLoaders
{
    /// <summary>
    /// Finds and registers json parts in GameData.
    /// </summary>
    public sealed class GameDataJsonPartFactory : PartFactory
    {
        private static JsonSeparateFileSerializedDataHandler _handler = new JsonSeparateFileSerializedDataHandler();
        private static SingleExplicitHierarchyStrategy _strat = new SingleExplicitHierarchyStrategy( _handler, () => throw new NotSupportedException( $"Tried to save something using a part *loader*" ) );

        private static Loader _loader = new Loader( null, null, null, _strat.Load_Object, _strat.Load_Data );

        private string _filePath;

        public override PartMetadata LoadMetadata()
        {
            PartMetadata partMeta = new PartMetadata( _filePath );
            partMeta.ReadDataFromDisk();
            return partMeta;
        }

        public override GameObject Load()
        {
            _handler.ObjectsFilename = Path.Combine( _filePath, "objects.json" );
            _handler.DataFilename = Path.Combine( _filePath, "data.json" );
            _loader.RefMap = new ForwardReferenceStore();
            _loader.Load();
            return _strat.LastSpawnedRoot;
        }

        // TODO - This can also be used to load saved vessels - saved vessels serialize as their root parts.

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".load_parts" )]
        private static void OnStartup()
        {
            // <mod_folder>/Parts/<part_id>/objects.json, data.json, _part.json
            string gameDataPath = HumanSpaceProgramMods.GetModDirectoryPath();
            string[] modDirectories = Directory.GetDirectories( gameDataPath );

            foreach( var modPath in modDirectories )
            {
                string partsDir = Path.Combine( modPath, "Parts" );
                if( !Directory.Exists( partsDir ) )
                {
                    continue;
                }
                string[] partDirectories = Directory.GetDirectories( partsDir );

                // register a loader for each part.
                foreach( var partPath in partDirectories )
                {
                    GameDataJsonPartFactory fac = new GameDataJsonPartFactory()
                    {
                        _filePath = partPath
                    };
                    PartRegistry.Register( new NamespacedIdentifier( Path.GetFileName( modPath ), Path.GetFileName( partPath ) ), fac );
                }
            }
        }
    }
}