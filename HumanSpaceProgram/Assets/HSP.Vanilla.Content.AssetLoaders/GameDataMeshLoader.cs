using HSP.Content;
using System.IO;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    internal class GameDataMeshLoader
    {
        public const string RELOAD_MESHES = HSPEvent.NAMESPACE_HSP + ".gdmsl.reload_meshes";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_MESHES )]
        public static void ReloadMeshes()
        {
            foreach( var modPath in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modId = HumanSpaceProgramContent.GetModID( modPath );

                string[] files = Directory.GetFiles( modPath, "*.obj", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () => OBJ.Importer.LoadOBJ( file ), true );
                }
            }
        }
    }
}