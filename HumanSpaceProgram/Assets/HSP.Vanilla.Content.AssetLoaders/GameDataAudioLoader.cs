using HSP.Content;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System.IO;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class GameDataAudioLoader
    {
        public const string RELOAD_AUDIO = HSPEvent.NAMESPACE_HSP + ".gdal.reload_audio";

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