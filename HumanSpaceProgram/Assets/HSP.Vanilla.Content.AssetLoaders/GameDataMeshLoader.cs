using HSP.Content;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityPlus.AssetManagement;
using System;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class MeshLoader : IAssetLoader
    {
        public const string RELOAD_MESHES = HSPEvent.NAMESPACE_HSP + ".gdmsl.reload_meshes";
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_MESHES )]
        private static void RegisterMeshLoader()
        {
            AssetRegistry.RegisterLoader( new MeshLoader() );
        }

        public Type OutputType => typeof( Mesh );

        public bool CanLoad( AssetDataHandle handle )
        {
            string ext = handle.FormatHint;
            return ext == ".obj" || ext == ".hspm";
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, CancellationToken ct )
        {
            string ext = handle.FormatHint;
            using Stream stream = await handle.OpenMainStreamAsync( ct );

            if( ext == ".obj" )
            {
                using StreamReader reader = new StreamReader( stream );
                return OBJ.Importer.LoadOBJ( reader, "OBJ_Asset" );
            }
            else // .hspm
            {
                return HSPM.Importer.Load( stream, "HSPM_Asset" );
            }
        }
    }
}