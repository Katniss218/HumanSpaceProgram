using HSP.Vessels;
using HSP.Vessels.Serialization;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Content.AssetLoaders
{
    /// <summary>
    /// Finds and registers json parts in GameData.
    /// </summary>
    public sealed class GameDataJsonVesselFactory : PartFactory
    {        
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

            var data = new JsonSerializedDataHandler( Path.Combine( filePath, "gameobjects.json" ) )
                .Read();

            return SerializationUnit.Deserialize<GameObject>( data, refMap );
        }

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_HSP + ".load_vessels" )]
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