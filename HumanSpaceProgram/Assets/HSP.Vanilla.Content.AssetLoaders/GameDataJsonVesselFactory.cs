using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Content.AssetLoaders
{
    /// <summary>
    /// Finds and registers json parts in GameData.
    /// </summary>
    public sealed class GameDataJsonVesselFactory : PartFactory
    {
        public const string RELOAD_VESSELS = HSPEvent.NAMESPACE_HSP + ".reload_vessels";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_VESSELS )]
        public static void ReloadVessels2()
        {
            GameDataJsonVesselFactory.ReloadVesselsAsParts();
        }


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

        public static void ReloadVesselsAsParts()
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
                PartRegistry.Register( new NamespacedID( "Vessels", vId ), fac );
            }
        }
    }
}