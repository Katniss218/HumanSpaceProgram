using HSP.Content;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
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

        public bool CanLoad( AssetDataHandle handle ) => handle.FormatHint == ".jsonmat";

        public async Task<object> LoadAsync( AssetDataHandle handle, CancellationToken ct )
        {
            using Stream stream = await handle.OpenMainStreamAsync( ct );
            using StreamReader sr = new StreamReader( stream );
            string json = await sr.ReadToEndAsync();

            SerializedData data = new JsonStringReader( json ).Read();

            return SerializationUnit.Deserialize<Material>( data );
        }
    }
}