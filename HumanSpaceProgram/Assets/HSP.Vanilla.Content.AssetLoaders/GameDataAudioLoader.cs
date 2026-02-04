using HSP.Content;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
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
            return ext == ".wav" || ext == ".ogg";
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, CancellationToken ct )
        {
            string ext = handle.FormatHint;

            if( ext == ".wav" )
            {
                using Stream stream = await handle.OpenMainStreamAsync( ct );
                using MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync( ms, 81920, ct );
                return WAV.Importer.LoadWAV( ms.ToArray(), "WAV_Asset" );
            }
            else if( ext == ".ogg" )
            {
                if( handle.TryGetLocalFilePath( out string path ) )
                {
                    // Use streaming loader
                    return OGG.Importer.Load( path );
                }
                else
                {
                    // OGG streaming requires a path or URL for UnityWebRequest. 
                    // If we are in a zip or procedural, we can't easily stream without writing to disk.
                    throw new NotSupportedException( "OGG loading currently requires a physical file path." );
                }
            }

            return null;
        }
    }

    public class GameDataAudioLoader
    {

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_AUDIO )]
        public static void ReloadAudio2()
        {
            GameDataAudioLoader.ReloadAudio();
        }

        public static void ReloadAudio()
        {
            foreach( var modPath in HumanSpaceProgramContent.GetAllModDirectories() )
            {
                string[] files = Directory.GetFiles( modPath, "*.wav", SearchOption.AllDirectories );
                foreach( var file in files )
                {
                    AssetRegistry.RegisterLazy( HumanSpaceProgramContent.GetAssetID( file ), () => LoadWAV( file ), true );
                }
            }
        }

        private static AudioClip LoadWAV( string fileName )
        {
            return WAV.Importer.LoadWAV( fileName );
        }
    }
}