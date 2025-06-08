using HSP.Content;
using System.IO;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Content.AssetLoaders
{
    internal class GameDataMaterialLoader
    {
        public const string RELOAD_MATERIALS = HSPEvent.NAMESPACE_HSP + ".gdml.reload_materials";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_MATERIALS )]
        public static void ReloadMaterials()
        {
            foreach( var modPath in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modId = HumanSpaceProgramContent.GetModID( modPath );

                string[] files = Directory.GetFiles( modPath, "*.jsonmat", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () =>
                    {
                        var data = new JsonSerializedDataHandler( file )
                            .Read();

                        return SerializationUnit.Deserialize<Material>( data );
                    }, true );
                }
            }
        }
    }
}