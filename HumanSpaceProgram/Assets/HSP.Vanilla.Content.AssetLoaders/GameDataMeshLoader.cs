using GLTFast;
using HSP.Content;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    internal class GameDataMeshLoader
    {
        public const string RELOAD_MATERIALS = HSPEvent.NAMESPACE_HSP + ".gdmel.reload_meshes";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_MATERIALS )]
        public static void ReloadMeshes()
        {
            foreach( var modPath in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string modId = HumanSpaceProgramContent.GetModID( modPath );

                var files = Directory.GetFiles( modPath, "*.gltf", SearchOption.AllDirectories )
                    .Union( Directory.GetFiles( modPath, "*.glb", SearchOption.AllDirectories ) );
                foreach( var file in files )
                {
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () =>
                    {
                        Mesh mesh = Task.Run( async () =>
                        {
                            var gltf = new GLTFast.GltfImport( deferAgent: new UninterruptedDeferAgent() );
                            await gltf.LoadFile( file );
                            return gltf.GetMesh( 0, 0 );
                        } ).Result;

                        return mesh;
                    }, true );
                }
            }
        }
    }
}