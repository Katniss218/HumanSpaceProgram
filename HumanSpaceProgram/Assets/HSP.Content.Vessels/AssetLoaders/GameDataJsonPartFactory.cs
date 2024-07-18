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
    public sealed class GameDataJsonPartFactory : PartFactory
    {
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

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".load_parts" )]
        private static void OnStartup()
        {
            // <mod_folder>/Parts/<part_id>/objects.json, data.json, _part.json
            string gameDataPath = ContentUtils.GetModDirectoryPath();
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