using System;
using UnityEngine;

namespace HSP.Audio
{
    [RequireComponent( typeof( AudioSource ) )]
    public class AudioSourcePoolItem : MonoBehaviour, IAudioHandle
    {
        public AudioClip Clip => _audioSource.clip;

        public AudioHandleState State { get; private set; }

        float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if( State == AudioHandleState.Playing ) // Only set right away if playing (not fading).
                    _audioSource.volume = value;
                _audioSource.volume = value;
            }
        }

        public float Pitch { get => _audioSource.pitch; set => _audioSource.pitch = value; }

        public bool Loop { get => _audioSource.loop; internal set => _audioSource.loop = value; }

        private Transform _transformToFollow;
        private AudioSource _audioSource;

        private float timeAtNextState;
        private float fadeSeconds;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if( State == AudioHandleState.Finished || State == AudioHandleState.Ready )
                return;

            if( _transformToFollow != null )
                transform.position = _transformToFollow.position;

            if( State == AudioHandleState.PlayingDelay )
            {
                if( UnityEngine.Time.time >= timeAtNextState )
                {
                    SetState_PlayingFade();
                }
            }
            else if( State == AudioHandleState.FinishedDelay )
            {
                if( UnityEngine.Time.time >= timeAtNextState )
                {
                    SetState_FinishedFade();
                }
            }

            if( State == AudioHandleState.PlayingFade )
            {
                float remainingFadeSeconds = timeAtNextState - UnityEngine.Time.time; // We know when the fade will end (timeAtNextState) and how long to fade for in total (fadeSeconds).
                float fadePercent = 1.0f - (remainingFadeSeconds / fadeSeconds);
                _audioSource.volume = Mathf.Lerp( 0.0f, Volume, fadePercent );

                if( UnityEngine.Time.time >= timeAtNextState )
                {
                    SetState_Playing();
                }
            }
            else if( State == AudioHandleState.FinishedFade )
            {
                float remainingFadeSeconds = timeAtNextState - UnityEngine.Time.time; // We know when the fade will end (timeAtNextState) and how long to fade for in total (fadeSeconds).
                float fadePercent = 1.0f - (remainingFadeSeconds / fadeSeconds);
                _audioSource.volume = Mathf.Lerp( Volume, 0.0f, fadePercent );

                if( UnityEngine.Time.time >= timeAtNextState )
                {
                    SetState_Finished();
                }
            }

            if( State == AudioHandleState.Playing )
            {
                if( UnityEngine.Time.time >= timeAtNextState ) // When finished, just stop.
                {
                    SetState_Finished();
                }
            }
        }

        private void ResetState()
        {
            State = AudioHandleState.Ready;
            _audioSource.time = 0.0f;
            this.gameObject.SetActive( true );
        }
        private void SetState_PlayingDelay( float delaySeconds )
        {
            timeAtNextState = UnityEngine.Time.time + delaySeconds;
            State = AudioHandleState.PlayingDelay;
        }
        private void SetState_PlayingFade()
        {
            _audioSource.Play();
            timeAtNextState = UnityEngine.Time.time + fadeSeconds;
            State = AudioHandleState.PlayingFade;
        }
        private void SetState_Playing()
        {
            timeAtNextState = Loop
                ? float.MaxValue
                : UnityEngine.Time.time + (_audioSource.clip.length - fadeSeconds);

            State = AudioHandleState.Playing;
        }
        private void SetState_FinishedDelay( float delaySeconds )
        {
            timeAtNextState = UnityEngine.Time.time + delaySeconds;
            State = AudioHandleState.FinishedDelay;
        }
        private void SetState_FinishedFade()
        {
            timeAtNextState = UnityEngine.Time.time + fadeSeconds;
            State = AudioHandleState.FinishedFade;
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

            this.fadeSeconds = 0.0f;
            _audioSource.Play();
            SetState_Playing();
        }

        public void Play( float delaySeconds, float fadeSeconds )
        {
            if( State != AudioHandleState.Ready )
                throw new InvalidOperationException( $"Audio can only be played when in the {nameof( AudioHandleState.Ready )} state." );

            if( delaySeconds == 0.0f )
            {
                if( fadeSeconds == 0.0f )
                {
                    this.fadeSeconds = 0.0f;
                    _audioSource.Play();
                    SetState_Playing();
                }
                else
                {
                    this.fadeSeconds = fadeSeconds;
                    SetState_PlayingFade();
                }
            }
            else
            {
                this.fadeSeconds = fadeSeconds;
                SetState_PlayingDelay( delaySeconds );
            }
        }

        public void Stop()
        {
            if( State == AudioHandleState.Finished )
                throw new InvalidOperationException( $"Audio can only be stopped when not in the {nameof( AudioHandleState.Finished )} state." );

            SetState_Finished();
        }

        public void Stop( float delaySeconds, float fadeSeconds )
        {
            if( State == AudioHandleState.Ready )
            {
                State = AudioHandleState.Finished;
                gameObject.SetActive( false );
            }

            if( State == AudioHandleState.Finished )
                throw new InvalidOperationException( $"Audio can only be stopped when not in the {nameof( AudioHandleState.Finished )} state." );

            if( delaySeconds == 0.0f )
            {
                if( fadeSeconds == 0.0f )
                {
                    SetState_Finished();
                }
                else
                {
                    this.fadeSeconds = fadeSeconds;
                    SetState_FinishedFade();
                }
            }
            else
            {
                this.fadeSeconds = fadeSeconds;
                SetState_FinishedDelay( delaySeconds );
            }
        }
    }
}