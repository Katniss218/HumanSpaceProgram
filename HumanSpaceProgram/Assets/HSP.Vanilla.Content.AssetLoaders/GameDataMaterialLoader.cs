using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class MaterialLoader : IAssetLoader
    {
        public const string RELOAD_MATERIALS = HSPEvent.NAMESPACE_HSP + ".gdml.reload_materials";
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_MATERIALS )]
        private static void RegisterMaterialLoader()
        {
            AssetRegistry.RegisterLoader( new MaterialLoader() );
        }

        public Type OutputType => typeof( Material );

        public bool CanLoad( AssetDataHandle handle, Type targetType )
        {
            return handle.Format == HSPFormats.JsonMat;
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, Type targetType, CancellationToken ct )
        {
            // PHASE 1: Heavy lifting on the background thread.
            // Note: AssetRegistry has already offloaded this method to Task.Run, 
            // but we use ConfigureAwait(false) to be explicit about staying off the context.

            SerializedData data;

            // Open stream (background safe)
            using( Stream stream = await handle.OpenMainStreamAsync( ct ).ConfigureAwait( false ) )
            using( StreamReader sr = new StreamReader( stream ) )
            {
                // Read text (background safe)
                string json = await sr.ReadToEndAsync().ConfigureAwait( false );

                // Parse JSON (background safe)
                data = new JsonStringReader( json ).Read();
            }

            // PHASE 2: Unity Object creation on Main Thread.
            // We use the Dispatcher to queue this work. 
            // If called via AssetRegistry.Get<T>, the main thread is pumping this queue.

            return await MainThreadDispatcher.RunAsync( () =>
            {
                return SerializationUnit.Deserialize<Material>( data );
            } ).ConfigureAwait( false );
        }
    }
}