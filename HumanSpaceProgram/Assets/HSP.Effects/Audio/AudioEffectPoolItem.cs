using System;
using UnityEngine;

namespace HSP.Effects.Audio
{
    [RequireComponent( typeof( AudioSource ) )]
    internal class AudioEffectPoolItem : MonoBehaviour
    {
        internal const float VOLUME_MIN_DISTANCE_MULTIPLIER = 1f;
        internal const float VOLUME_MAX_DISTANCE_MULTIPLIER = 1000f;

        internal AudioClip Clip { get => _audioSource.clip; set => _audioSource.clip = value; }

        internal AudioEffectState State { get; private set; }

        internal float Volume
        {
            get => _audioSource.volume;
            set
            {
                float volumeSquared = value * value;

                _audioSource.volume = value;
                _audioSource.minDistance = volumeSquared * VOLUME_MIN_DISTANCE_MULTIPLIER;
                _audioSource.maxDistance = volumeSquared * VOLUME_MAX_DISTANCE_MULTIPLIER;
            }
        }

        internal float Pitch { get => _audioSource.pitch; set => _audioSource.pitch = value; }

        internal bool Loop { get => _audioSource.loop; set => _audioSource.loop = value; }

        AudioChannel _channel;
        internal AudioChannel Channel
        {
            get => _channel;
            set
            {
                _channel = value;
                _audioSource.spatialBlend = _channel.Is3D() ? 1.0f : 0.0f;
                _audioSource.priority = _channel.GetPriority();
                _audioSource.outputAudioMixerGroup = _channel.GetAudioMixerGroup();
            }
        }

        internal Transform TargetTransform { get; set; }

        internal int version;
        internal AudioEffectHandle currentHandle; // kind of singleton (per pool item) with the handle management.

        private AudioSource _audioSource;
        private float _timeWhenFinished;
        private IAudioEffectData _data;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.dopplerLevel = 0.0f; // Doppler sounds blergh when you move the camera around.
            _audioSource.playOnAwake = false;
        }

        void Update()
        {
            if( State == AudioEffectState.Finished || State == AudioEffectState.Ready )
                return;

            if( TargetTransform != null )
                this.transform.position = TargetTransform.position;

            _data.OnUpdate( this.currentHandle );

            if( UnityEngine.Time.time >= _timeWhenFinished ) // When finished, just stop.
            {
                SetState_Finished();
            }
        }

        private void ResetState()
        {
            State = AudioEffectState.Ready;
            _audioSource.time = 0.0f;
        }

        private void SetState_Playing()
        {
            this.gameObject.SetActive( true );

            _data.OnInit( this.currentHandle );

            _timeWhenFinished = Loop
                ? float.MaxValue
                : UnityEngine.Time.time + _audioSource.clip.length;

            _audioSource.Play();

            State = AudioEffectState.Playing;
        }

        private void SetState_Finished()
        {
            _audioSource.Stop();
            State = AudioEffectState.Finished;
            gameObject.SetActive( false ); // Disables the gameobject to stop empty update calls and other processing.
        }

        internal void SetAudioData( IAudioEffectData data )
        {
            _data = data;
            version++;
            currentHandle = new AudioEffectHandle( this );

            this.ResetState();
        }

        internal void Play()
        {
            if( State != AudioEffectState.Ready )
                throw new InvalidOperationException( $"Audio can only be played when in the {nameof( AudioEffectState.Ready )} state." );

            SetState_Playing();
        }

        internal void Stop()
        {
            if( State != AudioEffectState.Playing )
                throw new InvalidOperationException( $"Audio can only be stopped when in the {nameof( AudioEffectState.Playing )} state." );

            SetState_Finished();
        }
    }
}