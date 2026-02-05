using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus;
using UnityPlus.AssetManagement;

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
            
            if( ext == ".obj" )
            {
                // Read text in background
                string objText;
                using( Stream stream = await handle.OpenMainStreamAsync( ct ).ConfigureAwait(false) )
                using( StreamReader reader = new StreamReader( stream ) )
                {
                    objText = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                // Parse on Main Thread (Importer constructs Mesh directly)
                return await MainThreadDispatcher.RunAsync( () =>
                {
                    using StringReader sr = new StringReader(objText);
                    return OBJ.Importer.LoadOBJ( sr, "OBJ_Asset" );
                } ).ConfigureAwait(false);
            }
            else // .hspm
            {
                // Read binary in background
                byte[] bytes;
                using( Stream stream = await handle.OpenMainStreamAsync( ct ).ConfigureAwait(false) )
                using( MemoryStream ms = new MemoryStream() )
                {
                    await stream.CopyToAsync( ms, 81920, ct ).ConfigureAwait(false);
                    bytes = ms.ToArray();
                }

                // Parse on Main Thread
                return await MainThreadDispatcher.RunAsync( () =>
                {
                    using MemoryStream ms = new MemoryStream(bytes);
                    return HSPM.Importer.Load( ms, "HSPM_Asset" );
                } ).ConfigureAwait(false);
            }
        }
    }
}
