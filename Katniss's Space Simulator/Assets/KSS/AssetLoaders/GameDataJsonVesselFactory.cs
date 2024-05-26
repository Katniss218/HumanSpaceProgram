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

namespace KSS.AssetLoaders
{
    /// <summary>
    /// Finds and registers json parts in GameData.
    /// </summary>
    public sealed class GameDataJsonVesselFactory : PartFactory
    {
        private static JsonSerializedDataHandler _handler = new JsonSerializedDataHandler();
        private static SingleExplicitHierarchyStrategy _strat = new SingleExplicitHierarchyStrategy( _handler, () => throw new NotSupportedException( $"Tried to save something using a part *loader*" ) );

        private static Loader _loader = new Loader( null, null, null, _strat.Load_Object, _strat.Load_Data );

        private string _vesselId;

        public override PartMetadata LoadMetadata()
        {
            VesselMetadata vesselMetadata = VesselMetadata.LoadFromDisk( _vesselId );

            PartMetadata partMeta = new PartMetadata( Path.Combine( vesselMetadata.GetRootDirectory(), _vesselId ) );
            partMeta.Name = vesselMetadata.Name;
            return partMeta;
        }

        public override GameObject Load( IForwardReferenceMap refMap )
        {
            string filePath = VesselMetadata.GetRootDirectory( _vesselId );

            _handler.ObjectsFilename = Path.Combine( filePath, "objects.json" );
            _handler.DataFilename = Path.Combine( filePath, "data.json" );
            _loader.RefMap = refMap;
            _loader.Load();
            return _strat.LastSpawnedRoot;
        }

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