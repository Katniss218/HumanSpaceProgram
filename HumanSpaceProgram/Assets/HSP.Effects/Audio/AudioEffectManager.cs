using UnityEngine;
using UnityEngine.Audio;
using UnityPlus;
using UnityPlus.AssetManagement;

namespace HSP.Effects.Audio
{
    public class AudioEffectManager : SingletonMonoBehaviour<AudioEffectManager>
    {
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
                    _audioMixer = AssetRegistry.Get<AudioMixer>( "builtin::HSP.Effects/Audio/NewAudioMixer" );
                }
                return _audioMixer;
            }
        }

        static ObjectPool<AudioEffectPoolItem, IAudioEffectData> _pool = new(
            ( i, data ) =>
            {
                i.SetAudioData( data );
            },
            i => i.State == AudioEffectState.Finished );

        // The earth moves, so 'position'-based audio will soon get out of range, unless its pinned to something.
        // We can't use absolute position, or scene position for those reasons. Everything in 3D should be pinned to something else, possibly with an offset.


        public static AudioEffectHandle Prepare( IAudioEffectData data )
        {
            var poolItem = _pool.Get( data );

            return poolItem.currentHandle;
        }

        public static AudioEffectHandle Play( IAudioEffectData data )
        {
            var poolItem = _pool.Get( data );

            poolItem.Play();
            return poolItem.currentHandle;
        }

        /// <summary>
        /// Prepares a new audio, but doesn't start playing it.
        /// </summary>
        /// <remarks>
        /// The audio should be started with <see cref="IAudioEffectHandle.Play"/>. This is useful for preparing an audio that will be played later.
        /// </remarks>
        /// <param name="transform">The source of the sound. The audio will be played at the position of the transform.</param>
        public static AudioEffectHandle PrepareInWorld( Transform transform, AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioEffectDefinition() { Clip = clip, TargetTransform = transform, Loop = loop, Channel = channel, Volume = new( volume ), Pitch = new( pitch ) } );

            return poolItem.currentHandle;
        }

        /// <summary>
        /// Plays a new audio that will follow the given transform until it ends playing.
        /// </summary>
        /// <param name="transform">The source of the sound. The audio will be played at the position of the transform.</param>
        public static AudioEffectHandle PlayInWorld( Transform transform, AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioEffectDefinition() { Clip = clip, TargetTransform = transform, Loop = loop, Channel = channel, Volume = new( volume ), Pitch = new( pitch ) } );

            poolItem.Play();
            return poolItem.currentHandle;
        }

        /// <summary>
        /// Prepares a new audio that is not located in the game world, and is not affected by its effects, but doesn't start playing it.
        /// </summary>
        /// <remarks>
        /// The audio should be started with <see cref="IAudioEffectHandle.Play"/>. This is useful for preparing an audio that will be played later.
        /// </remarks>
        public static AudioEffectHandle Prepare( AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioEffectDefinition() { Clip = clip, TargetTransform = null, Loop = loop, Channel = channel, Volume = new( volume ), Pitch = new( pitch ) } );

            return poolItem.currentHandle;
        }

        /// <summary>
        /// Plays a new audio that is not located in the game world, and is not affected by its effects.
        /// </summary>
        public static AudioEffectHandle Play( AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            var poolItem = _pool.Get( new AudioEffectDefinition() { Clip = clip, TargetTransform = null, Loop = loop, Channel = channel, Volume = new( volume ), Pitch = new( pitch ) } );

            poolItem.Play();
            return poolItem.currentHandle;
        }


        // saving/loading audio, how will that work?


        // maybe group audios by what they're playing, like grouping a bunch of engines.
        // - needs a marker that the audio is 'stochastic', otherwise the phase of the audio signal matters and can't be grouped.
        // - if the audio is 'stochastic', then slight differences in phase across several sources can produce nasty audible artifacts.
    }
}