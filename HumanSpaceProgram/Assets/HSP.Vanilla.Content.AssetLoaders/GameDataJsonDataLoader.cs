using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class JsonLoader : IAssetLoader
    {
        public const string RELOAD_JSON_DATA = HSPEvent.NAMESPACE_HSP + ".gdml.reload_json_data";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_JSON_DATA )]
        private static void RegisterJsonLoader()
        {
            AssetRegistry.RegisterLoader( new JsonLoader() );
        }

        public Type OutputType => typeof( object );

        public bool CanLoad( AssetDataHandle handle, Type targetType )
        {
            return handle.Format == CoreFormats.Json;
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, Type targetType, CancellationToken ct )
        {
            using Stream stream = await handle.OpenMainStreamAsync( ct );
            using StreamReader sr = new StreamReader( stream );
            string json = await sr.ReadToEndAsync();

            SerializedData data = new JsonStringReader( json ).Read();

            return SerializationUnit.Deserialize<object>( data ); // Will create the real instance according to the "$type" property.
        }
    }
}