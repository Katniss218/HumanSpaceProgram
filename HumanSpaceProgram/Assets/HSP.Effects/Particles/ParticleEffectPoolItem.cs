using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Effects.Particles
{
    [RequireComponent( typeof( ParticleSystem ) )]
    internal class ParticleEffectPoolItem : MonoBehaviour
    {
        private struct CachedEntry
        {
            public Action Setter;
        }


        internal ParticleEffectState State { get; private set; } = ParticleEffectState.Ready;

        private CachedEntry[] _cachedEntriesWithoutDrivers; // set only on init
        private CachedEntry[] _cachedEntriesWithDrivers; // set in update, or whenever the value changed.

        internal Transform TargetTransform { get; set; }

        internal int version;
        internal ParticleEffectHandle currentHandle; // kind of singleton (per pool item) with the handle management.

        // Expose internally so we don't have to expose every property here again and add essentially duplicated code.
        internal ParticleSystem particleSystem => _particleSystem;
        internal ParticleSystem.MainModule main => _main;
        internal ParticleSystemRenderer renderer => _renderer;

        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule _main;
        private ParticleSystemRenderer _renderer;

        private float _timeWhenFinished;
        private IParticleEffectData _data;

        internal bool Loop { get => _main.loop; set => _main.loop = value; }

        internal float Size
        {
            get => _particleSystem.main.startSize.constant;
            set
            {
#warning TODO - come up with some way to simplify this, I don't want to have 1000 properties for this.
                var val = _main.startSize;
                val.constant = value;
                _main.startSize = val;
            }
        }

        internal Material Material
        {
            get => _renderer.sharedMaterial;
            set
            {
                _renderer.sharedMaterial = value;
                _renderer.SetActiveVertexStreams( new List<ParticleSystemVertexStream>()
                {
                    ParticleSystemVertexStream.Position,
                    ParticleSystemVertexStream.Color,
                    ParticleSystemVertexStream.UV
                } );
            }
        }

        void Awake()
        {
            _particleSystem = this.GetComponent<ParticleSystem>();
            _main = _particleSystem.main;
            _renderer = this.GetComponent<ParticleSystemRenderer>();
            _main.playOnAwake = false;
            _particleSystem.Stop();
        }

        void Update()
        {
            if( State == ParticleEffectState.Finished || State == ParticleEffectState.Ready )
                return;

            if( TargetTransform != null )
            {
                this.transform.SetPositionAndRotation( TargetTransform.position, TargetTransform.rotation );
            }

            _data.OnUpdate( this.currentHandle );

            if( UnityEngine.Time.time >= _timeWhenFinished ) // When finished, just stop.
            {
                SetState_Finished();
            }
        }

        private void ResetState()
        {
            State = ParticleEffectState.Ready;
            _particleSystem.time = 0.0f;
        }

        private void SetState_Playing()
        {
            this.gameObject.SetActive( true );

            _data.OnInit( this.currentHandle );

            _timeWhenFinished = Loop
                ? float.MaxValue
                : UnityEngine.Time.time + _main.duration;

            _particleSystem.Play();

            State = ParticleEffectState.Playing;
        }

        private void SetState_Finished()
        {
            _particleSystem.Stop();
            State = ParticleEffectState.Finished;
            gameObject.SetActive( false ); // Disables the gameobject to stop empty update calls and other processing.
        }

        internal void SetParticleData( IParticleEffectData data )
        {
            _data = data;
            version++;
            currentHandle = new ParticleEffectHandle( this );

            this.ResetState();
        }

        internal void Play()
        {
            if( State != ParticleEffectState.Ready )
                throw new InvalidOperationException( $"Particles can only be played when in the {nameof( ParticleEffectState.Ready )} state." );

            SetState_Playing();
        }

        internal void Stop()
        {
            if( State != ParticleEffectState.Playing )
                throw new InvalidOperationException( $"Particles can only be stopped when in the {nameof( ParticleEffectState.Playing )} state." );

            _particleSystem.Stop();
            _timeWhenFinished = UnityEngine.Time.time + _main.duration;
            // wait for the particles to disappear before reclaiming it.
        }
    }
}