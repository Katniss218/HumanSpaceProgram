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

        public bool CanLoad( AssetDataHandle handle, Type targetType )
        {
            return handle.Format == CoreFormats.Wav;// || ext == ".ogg";
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, Type targetType, CancellationToken ct )
        {
            if( handle.Format == CoreFormats.Wav )
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
            throw new Exception( RELOAD_AUDIO + ": Unsupported audio format: " + handle.Format );
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

        }
    }
}