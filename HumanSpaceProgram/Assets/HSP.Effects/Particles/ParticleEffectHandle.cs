using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Effects.Particles
{
    public readonly struct ParticleEffectHandle : IEffectHandle
    {
        private readonly int _version;
        private readonly ParticleEffectPoolItem _poolItem;

        // Expose internally so we don't have to expose every property here again and add essentially duplicated code.
        internal ParticleEffectPoolItem poolItem => _poolItem;

        internal ParticleEffectHandle( ParticleEffectPoolItem poolItem )
        {
            _poolItem = poolItem;
            _version = poolItem.version;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void EnsureValid()
        {
            // Null check because the _poolItem might've been destroyyed for whatever reason.
            if( _poolItem == null || _version != _poolItem.version )
                throw new ObjectDisposedException( nameof( ParticleEffectHandle ), $"The {nameof( ParticleEffectPoolItem )} backing the {nameof( ParticleEffectHandle )} has been disposed or reused and is no longer valid for this handle." );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool IsValid()
        {
            // Null check because the _poolItem might've been destroyyed for whatever reason.
            return _poolItem != null && _version == _poolItem.version;
        }

        /// <summary>
        /// The state that the audio handle is currently in.
        /// </summary>
        public ObjectPoolItemState State
        {
            get
            {
                EnsureValid();
                return _poolItem.State;
            }
        }

        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Transform TargetTransform
        {
            get
            {
                EnsureValid();
                return _poolItem.TargetTransform;
            }
            set
            {
                EnsureValid();
                _poolItem.TargetTransform = value;
            }
        }

        public float Size
        {
            get
            {
                EnsureValid();
                return _poolItem.Size;
            }
            set
            {
                EnsureValid();
                _poolItem.Size = value;
            }
        }

        public Material Material
        {
            get
            {
                EnsureValid();
                return _poolItem.Material;
            }
            set
            {
                EnsureValid();
                _poolItem.Material = value;
            }
        }



        //
        //  Playback controls
        //

        /// <summary>
        /// Starts the playback immediately.
        /// </summary>
        public void Play()
        {
            EnsureValid();
            _poolItem.Play();
        }

        /// <summary>
        /// Stops the playback immediately.
        /// </summary>
        public void Stop()
        {
            EnsureValid();
            _poolItem.Stop();
        }

        public bool TryPlay()
        {
            if( !this.IsValid() || this.State != ObjectPoolItemState.Ready )
                return false;

            this.Play();
            return true;
        }

        public bool TryStop()
        {
            if( !this.IsValid() || this.State != ObjectPoolItemState.Playing )
                return false;

            this.Stop();
            return true;
        }
    }
}