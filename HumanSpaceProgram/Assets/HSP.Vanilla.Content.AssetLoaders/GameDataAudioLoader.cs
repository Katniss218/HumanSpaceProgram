using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class AudioLoader : IAssetLoader
    {
        public const string RELOAD_AUDIO = HSPEvent.NAMESPACE_HSP + ".gdal.reload_audio";
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_AUDIO )]
        private static void RegisterAudioLoader()
        {
            AssetRegistry.RegisterLoader( new AudioLoader() );
        }

        public Type OutputType => typeof( AudioClip );

        public bool CanLoad( AssetDataHandle handle )
        {
            string ext = handle.FormatHint;
            return ext == ".wav";// || ext == ".ogg";
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, CancellationToken ct )
        {
            string ext = handle.FormatHint;

            if( ext == ".wav" )
            {
                // Read bytes in background
                byte[] bytes;
                using( Stream stream = await handle.OpenMainStreamAsync( ct ).ConfigureAwait( false ) )
                using( MemoryStream ms = new MemoryStream() )
                {
                    await stream.CopyToAsync( ms, ct ).ConfigureAwait( false );
                    bytes = ms.ToArray();
                }

                // Create clip on Main Thread
                return await MainThreadDispatcher.RunAsync( () =>
                {
                    return WAV.Importer.LoadWAV( bytes, "WAV_Asset" );
                } ).ConfigureAwait( false );
            }
            /*else if( ext == ".ogg" )
            {
                if( handle.TryGetLocalFilePath( out string path ) )
                {
                    // UnityWebRequest must originate on Main Thread
                    return await MainThreadDispatcher.RunAsync( () =>
                    {
                        return OGG.Importer.Load( path );
                    } ).ConfigureAwait( false );
                }
                else
                {
                    // OGG streaming requires a path or URL for UnityWebRequest. 
                    throw new NotSupportedException( "OGG loading currently requires a physical file path." );
                }
            }*/

            return null;
        }
    }
}