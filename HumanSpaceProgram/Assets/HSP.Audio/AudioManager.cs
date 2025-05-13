using UnityEngine;
using UnityEngine.Audio;
using UnityPlus;
using UnityPlus.AssetManagement;

namespace HSP.Audio
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        private struct AudioHandlePoolData
        {
            public AudioClip clip;
            public Transform transform;
            public bool loop;
            public AudioChannel channel;
            public float volume;
            public float pitch;

            public AudioHandlePoolData( AudioClip clip, Transform transform, bool loop, AudioChannel channel, float volume, float pitch )
            {
                this.clip = clip;
                this.transform = transform;
                this.loop = loop;
                this.channel = channel;
                this.volume = volume;
                this.pitch = pitch;
            }
        }

        static AudioMixer _audioMixer;
        /// <summary>
        /// Gets the audio mixer used by HSP.
        /// </summary>
        public static AudioMixer AudioMixer
        {
            get
            {
                if( _audioMixer == null )
                {
                    _audioMixer = AssetRegistry.Get<AudioMixer>( "builtin::HSP.Audio/NewAudioMixer" );
                }
                return _audioMixer;
            }
        }

        static ObjectPool<AudioSourcePoolItem, AudioHandlePoolData> _pool = new(
            ( i, data ) =>
            {
                i.SetAudioData( data.transform, data.clip, data.loop, data.channel, data.volume, data.pitch );
            },
            i => i.State == AudioHandleState.Finished );

        // The earth moves, so 'position'-based audio will soon get out of range, unless its pinned to something.
        // We can't use absolute position, or scene position for those reasons. Everything in 3D should be pinned to something else, possibly with an offset.


        /// <summary>
        /// Prepares a new audio, but doesn't start playing it.
        /// </summary>
        /// <remarks>
        /// The audio should be started with <see cref="IAudioHandle.Play"/>. This is useful for preparing an audio that will be played later.
        /// </remarks>
        /// <param name="transform">The source of the sound. The audio will be played at the position of the transform.</param>
        public static IAudioHandle PrepareInWorld( Transform transform, AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioHandlePoolData( clip, transform, loop, channel, volume, pitch ) );

            return poolItem;
        }

        /// <summary>
        /// Plays a new audio that will follow the given transform until it ends playing.
        /// </summary>
        /// <param name="transform">The source of the sound. The audio will be played at the position of the transform.</param>
        public static IAudioHandle PlayInWorld( Transform transform, AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioHandlePoolData( clip, transform, loop, channel, volume, pitch ) );

            poolItem.Play();
            return poolItem;
        }

        /// <summary>
        /// Prepares a new audio that is not located in the game world, and is not affected by its effects, but doesn't start playing it.
        /// </summary>
        /// <remarks>
        /// The audio should be started with <see cref="IAudioHandle.Play"/>. This is useful for preparing an audio that will be played later.
        /// </remarks>
        public static IAudioHandle Prepare( AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioHandlePoolData( clip, null, loop, channel, volume, pitch ) );

            return poolItem;
        }

        /// <summary>
        /// Plays a new audio that is not located in the game world, and is not affected by its effects.
        /// </summary>
        public static IAudioHandle Play( AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioHandlePoolData( clip, null, loop, channel, volume, pitch ) );

            poolItem.Play();
            return poolItem;
        }


        // saving/loading audio, how will that work?


        // maybe group audios by what they're playing, like grouping a bunch of engines.
        // - needs a marker that the audio is 'stochastic', otherwise the phase of the audio signal matters and can't be grouped.
        // - if the audio is 'stochastic', then slight differences in phase across several sources can produce nasty audible artifacts.
    }
}