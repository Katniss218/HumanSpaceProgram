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
using UnityPlus.Serialization.Strategies;

namespace KSS.AssetLoaders
{
    /// <summary>
    /// Finds and registers json parts in GameData.
    /// </summary>
    public sealed class GameDataJsonVesselFactory : PartFactory
    {
        private static JsonSeparateFileSerializedDataHandler _handler = new JsonSeparateFileSerializedDataHandler();
        private static JsonSingleExplicitHierarchyStrategy _strat = new JsonSingleExplicitHierarchyStrategy( _handler, () => throw new NotSupportedException( $"Tried to save something using a part *loader*" ) );

        private static Loader _loader = new Loader( null, null, _strat.Load_Object, _strat.Load_Data );


        private string _vesselId;

        public override PartMetadata LoadMetadata()
        {
            VesselMetadata vesselMetadata = new VesselMetadata( _vesselId );
            vesselMetadata.ReadDataFromDisk();

            PartMetadata partMeta = new PartMetadata( vesselMetadata.GetRootDirectory() + "/" + _vesselId );
            partMeta.Name = vesselMetadata.Name;
            return partMeta;
        }

        public override GameObject Load()
        {
            string filePath = VesselMetadata.GetRootDirectory( _vesselId );

            _handler.ObjectsFilename = Path.Combine( filePath, "objects.json" );
            _handler.DataFilename = Path.Combine( filePath, "data.json" );
            _loader.Load();
            return _strat.LastSpawnedRoot;
        }

        // TODO - This can also be used to load saved vessels - saved vessels serialize as their root parts.

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".load_vessels" )]
        private static void OnStartup()
        {
            string modsPath = HumanSpaceProgram.GetSavedVesselsDirectoryPath();
            string[] modDirectories = Directory.GetDirectories( modsPath );

            // register a loader for each part.
            foreach( var vesselPath in modDirectories )
            {
                string vId = Path.GetFileName( vesselPath );
                GameDataJsonVesselFactory fac = new GameDataJsonVesselFactory()
                {
                    _vesselId = vId
                };
                PartRegistry.Register( new NamespacedIdentifier( "Vessels", vId ), fac );
            }
        }
    }
}