using HSP.Content;
using System.IO;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Content.AssetLoaders
{
    internal class GameDataJsonDataLoader
    {
        public const string RELOAD_JSON_DATA = HSPEvent.NAMESPACE_HSP + ".gdml.reload_json_data";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_JSON_DATA )]
        public static void ReloadJsonData()
        {
            foreach( var modPath in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modId = HumanSpaceProgramContent.GetModID( modPath );

                string[] files = Directory.GetFiles( modPath, "*.json", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    string assetID = HumanSpaceProgramContent.GetAssetID( file );

                    if( AssetRegistry.IsRegisteredLazy( assetID ) ) // Don't overwrite potentially more important assets if anything else also reads plain JSON files.
                        continue;

                    AssetRegistry.RegisterLazy( assetID, () =>
                    {
                        var data = new JsonSerializedDataHandler( file )
                            .Read();

                        return SerializationUnit.Deserialize<object>( data ); // Will create the real instance according to the "$type" property.
                    }, true );
                }
            }
        }
    }
}