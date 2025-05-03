using HSP.ReferenceFrames;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HSP.Audio
{
    public interface IPlayingAudio
    {
        public void Pause();
        public void Resume();

        public void Stop();
    }

    public class AudioSourcePoolItem : MonoBehaviour, IPlayingAudio
    {
        public Transform Owner { get; internal set; }

        public bool IsFree => UnityEngine.Time.time > endPlayingTime;

        internal AudioSource audioSource;
        internal float startPlayingTime;
        internal float endPlayingTime;
        internal bool loop;

        public void Pause()
        {
            audioSource.Pause();
        }

        public void Resume()
        {
            audioSource.UnPause();
        }

        public void Stop()
        {
            audioSource.Stop();
            Destroy( gameObject );
        }
    }

    public class AudioSourcePool
    {
        internal List<AudioSourcePoolItem> poolItems = new();

        private GameObject poolParent;

        /// <summary>
        /// Plays a new audio that will follow the given transform until it ends playing
        /// </summary>
        public IPlayingAudio Play( AudioClip clip, Transform owner, bool loop, float volume = 1.0f, float pitch = 1.0f )
        {
            // Try to reuse an existing pool element first.
            foreach( var poolItem in this.poolItems )
            {
                if( poolItem.IsFree )
                {
                    SetAudioData( poolItem, owner, clip, loop, volume, pitch );
                    return poolItem;
                }
            }

            // No free elements in the pool, create a new one.
            var newPoolItem = CreatePoolItem( clip, owner, loop, volume, pitch );
            this.poolItems.Add( newPoolItem );
            return newPoolItem;
        }


        private AudioSourcePoolItem CreatePoolItem( AudioClip clip, Transform owner, bool loop, float volume = 1.0f, float pitch = 1.0f )
        {
            GameObject gameObject = new GameObject( "AudioSourcePoolItem" );

            if( poolParent == null )
            {
                poolParent = new GameObject( "AudioSourcePool pool parent" );
            }
            gameObject.transform.SetParent( poolParent.transform, false );

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();

            AudioSourcePoolItem poolItem = gameObject.AddComponent<AudioSourcePoolItem>();
            poolItem.audioSource = audioSource;

            SetAudioData( poolItem, owner, clip, loop, volume, pitch );

            poolItem.audioSource.Play();

            return poolItem;
        }

        private void SetAudioData( AudioSourcePoolItem poolItem, Transform owner, AudioClip clip, bool loop, float volume = 1.0f, float pitch = 1.0f )
        {
            const float VOLUME_MIN_DISTANCE_MULTIPLIER = 1f;
            const float VOLUME_MAX_DISTANCE_MULTIPLIER = 1000f;

            poolItem.audioSource.clip = clip;
            poolItem.audioSource.loop = loop;
            poolItem.audioSource.volume = volume;
            poolItem.audioSource.pitch = pitch;

            poolItem.Owner = owner;
            poolItem.startPlayingTime = UnityEngine.Time.time;
            poolItem.endPlayingTime = UnityEngine.Time.time + clip.length;
            poolItem.loop = loop;
            poolItem.audioSource.spatialBlend = (owner == null) ? 0.0f : 1.0f;
            poolItem.audioSource.dopplerLevel = 0.0f; // Doppler sounds blergh when you move the camera around.
            poolItem.audioSource.minDistance = (volume * volume) * VOLUME_MIN_DISTANCE_MULTIPLIER;
            poolItem.audioSource.maxDistance = (volume * volume) * VOLUME_MAX_DISTANCE_MULTIPLIER;
        }
    }

    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {

        // playing single-shot audio clips

        // audio position follows reference frame switches

        // looping audio

        // audio position follows either absolute position or is pinned to an object (with offset possible)

        // non-spatial audio.


        AudioSourcePool pool = new AudioSourcePool();

        private void OnEnable()
        {
            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch += ReferenceSwitchListener;

        }

        private void OnDisable()
        {
            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch -= ReferenceSwitchListener;
        }

        private void ReferenceSwitchListener( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {

        }

        /// <summary>
        /// Plays a new audio that will follow the given transform until it ends playing
        /// </summary>
        public static IPlayingAudio Play( AudioClip clip, Transform transform, bool loop, float volume = 1.0f, float pitch = 1.0f )
        {
            // earth moves, so 'position'-based audio will soon get out of range, unless its pinned to something.

            return instance.pool.Play( clip, transform, loop, volume, pitch );
        }

        /// <summary>
        /// Plays a new audio that will follow the given transform until it ends playing
        /// </summary>
        public static IPlayingAudio Play( AudioClip clip, bool loop, float volume = 1.0f, float pitch = 1.0f )
        {
            // earth moves, so 'position'-based audio will soon get out of range, unless its pinned to something.

            return instance.pool.Play( clip, null, loop, volume, pitch );
        }

        private void Update()
        {
            foreach( var poolItem in this.pool.poolItems )
            {
                if( poolItem.IsFree )
                {
                    continue;
                }

                if( poolItem.Owner != null )
                    poolItem.transform.position = poolItem.Owner.position;
            }
        }

        // saving/loading audio, how will that work?
        // audio manager handles what audios are playing.
        // abstract audios to not expose gameobjects directly, neither to the user nor to the serialization system.

        // audios need to be their own gameobjects not part of anything else.

        // maybe group audios by what they're playing, like grouping a bunch of engines.
        // - needs a marker that the audio is 'stochastic', otherwise the phase of the audio signal matters and can't be grouped.
        // - if the audio is 'stochastic', then slight differences in phase across several sources can produce nasty audible artifacts.
    }
}