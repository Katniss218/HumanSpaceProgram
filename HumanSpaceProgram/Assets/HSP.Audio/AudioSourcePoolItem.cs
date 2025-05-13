using System;
using UnityEngine;

namespace HSP.Audio
{
    [RequireComponent( typeof( AudioSource ) )]
    public class AudioSourcePoolItem : MonoBehaviour, IAudioHandle
    {
        public AudioClip Clip { get => _audioSource.clip; internal set => _audioSource.clip = value; }

        [field: SerializeField]
        public AudioHandleState State { get; private set; }

        public float Volume { get => _audioSource.volume; set => _audioSource.volume = value; }

        public float Pitch { get => _audioSource.pitch; set => _audioSource.pitch = value; }

        public bool Loop { get => _audioSource.loop; internal set => _audioSource.loop = value; }

        Transform _transformToFollow;

        AudioSource _audioSource;

        [field: SerializeField]
        float _timeWhenFinished;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if( State == AudioHandleState.Finished || State == AudioHandleState.Ready )
                return;

            if( _transformToFollow != null )
                this.transform.position = _transformToFollow.position;

            if( State == AudioHandleState.Playing )
            {
                if( UnityEngine.Time.time >= _timeWhenFinished ) // When finished, just stop.
                {
                    SetState_Finished();
                }
            }
        }

        private void ResetState()
        {
            State = AudioHandleState.Ready;
            _audioSource.time = 0.0f;
        }

        private void SetState_Playing()
        {
            this.gameObject.SetActive( true );

            _audioSource.Play();

            _timeWhenFinished = Loop
                ? float.MaxValue
                : UnityEngine.Time.time + _audioSource.clip.length;

            State = AudioHandleState.Playing;
        }

        private void SetState_Finished()
        {
            _audioSource.Stop();
            State = AudioHandleState.Finished;
            gameObject.SetActive( false ); // Disable to stop empty update calls.
        }

        internal void SetAudioData( Transform transformToFollow, AudioClip clip, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            const float VOLUME_MIN_DISTANCE_MULTIPLIER = 1f;
            const float VOLUME_MAX_DISTANCE_MULTIPLIER = 1000f;

            this._audioSource.clip = clip;
            this._audioSource.loop = loop;

            this.Volume = volume;
            this.Pitch = pitch;
            this._transformToFollow = transformToFollow;
            this.Loop = loop;
            this._audioSource.spatialBlend = channel.Is3D() ? 1.0f : 0.0f;
            this._audioSource.dopplerLevel = 0.0f; // Doppler sounds blergh when you move the camera around.
            this._audioSource.minDistance = (volume * volume) * VOLUME_MIN_DISTANCE_MULTIPLIER;
            this._audioSource.maxDistance = (volume * volume) * VOLUME_MAX_DISTANCE_MULTIPLIER;
            this._audioSource.outputAudioMixerGroup = channel.GetAudioMixerGroup();
            this._audioSource.playOnAwake = false;
            this._audioSource.priority = channel.GetPriority();
            this.ResetState();
        }

        public void Play()
        {
            if( State != AudioHandleState.Ready )
                throw new InvalidOperationException( $"Audio can only be played when in the {nameof( AudioHandleState.Ready )} state." );

            SetState_Playing();
        }

        public void Stop()
        {
            if( State != AudioHandleState.Playing )
                throw new InvalidOperationException( $"Audio can only be stopped when in the {nameof( AudioHandleState.Playing )} state." );

            SetState_Finished();
        }
    }
}