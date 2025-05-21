using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Effects.Meshes
{
    /// <summary>
    /// Represents an audio that has been prepared to play, and/or may be currently playing.
    /// </summary>
    public readonly struct MeshEffectHandle : IEffectHandle
    {
        private readonly int _version;
        private readonly MeshEffectPoolItem _poolItem;

        internal MeshEffectHandle( MeshEffectPoolItem poolItem )
        {
            _poolItem = poolItem;
            _version = poolItem.version;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EnsureValid()
        {
            // Null check because the _poolItem might've been destroyyed for whatever reason.
            if( _poolItem == null || _version != _poolItem.version )
                throw new ObjectDisposedException( nameof( MeshEffectHandle ), $"The {nameof( MeshEffectPoolItem )} backing the {nameof( MeshEffectHandle )} has been disposed or reused and is no longer valid for this handle." );
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