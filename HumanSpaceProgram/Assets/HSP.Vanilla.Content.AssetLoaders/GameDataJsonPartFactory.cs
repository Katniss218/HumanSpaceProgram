using HSP.Content;
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
    public sealed class GameDataJsonPartFactory : PartFactory
    {
        public const string RELOAD_PARTS = HSPEvent.NAMESPACE_HSP + ".reload_parts";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_PARTS )]
        public static void ReloadParts2()
        {
            GameDataJsonPartFactory.ReloadParts();
        }

#warning TODO - change to namespaced mod-global ID
        private string _filePath;

        public override PartMetadata LoadMetadata()
        {
            PartMetadata partMeta = PartMetadata.LoadFromDisk( _filePath );
            return partMeta;
        }

        public override GameObject Load( IForwardReferenceMap refMap )
        {
            var data = new JsonSerializedDataHandler( Path.Combine( _filePath, "gameobjects.json" ) )
                .Read();

            return SerializationUnit.Deserialize<GameObject>( data, refMap );
        }

        public static void ReloadParts()
        {
            // <mod_folder>/Parts/<part_id>/objects.json, data.json, _part.json
            string gameDataPath = HumanSpaceProgramContent.GetContentDirectoryPath();
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
                    PartRegistry.Register( new NamespacedID( Path.GetFileName( modPath ), Path.GetFileName( partPath ) ), fac );
                }
            }
        }
    }
}