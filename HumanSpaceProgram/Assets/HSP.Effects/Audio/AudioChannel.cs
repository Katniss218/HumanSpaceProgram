using System.Collections.Generic;
using UnityEngine.Audio;

namespace HSP.Effects.Audio
{
    public enum AudioChannel
    {
        // World (in-scene)
        Main_3D,
        Ambient_3D,
        Ambient_2D,

        // External to the world
        Main_2D,
        Music,
        UI
    }

    public static class AudioChannel_Ex
    {
        private static Dictionary<AudioChannel, AudioMixerGroup> _audioMixerGroups = new();

        public static bool Is3D( this AudioChannel channel )
        {
            return channel == AudioChannel.Main_3D || channel == AudioChannel.Ambient_3D;
        }

        public static bool IsWorld( this AudioChannel channel )
        {
            return channel == AudioChannel.Main_3D || channel == AudioChannel.Ambient_3D || channel == AudioChannel.Ambient_2D;
        }

        public static int GetPriority( this AudioChannel channel )
        {
            return 1;
        }

        public static AudioMixerGroup GetAudioMixerGroup( this AudioChannel channel )
        {
            if( _audioMixerGroups.TryGetValue( channel, out var group ) )
                return group;

            switch( channel )
            {
                case AudioChannel.Main_3D:
                    group = AudioEffectManager.AudioMixer.FindMatchingGroups( "3D Main" )[0];
                    break;
                case AudioChannel.Ambient_3D:
                    group = AudioEffectManager.AudioMixer.FindMatchingGroups( "3D Ambient" )[0];
                    break;
                case AudioChannel.Ambient_2D:
                    group = AudioEffectManager.AudioMixer.FindMatchingGroups( "2D Ambient" )[0];
                    break;

                case AudioChannel.Main_2D:
                    group = AudioEffectManager.AudioMixer.FindMatchingGroups( "2D Main" )[0];
                    break;
                case AudioChannel.Music:
                    group = AudioEffectManager.AudioMixer.FindMatchingGroups( "Music" )[0];
                    break;
                case AudioChannel.UI:
                    group = AudioEffectManager.AudioMixer.FindMatchingGroups( "UI" )[0];
                    break;
            }
            _audioMixerGroups.Add( channel, group );
            return group;
        }
    }
}