using System;
using System.Collections.Generic;
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
        public void EnsureValid()
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

        public bool IsSkinned => _poolItem.IsSkinned;

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

        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Mesh Mesh
        {
            get
            {
                EnsureValid();
                return _poolItem.meshFilter.sharedMesh;
            }
            set
            {
                EnsureValid();
                if( _poolItem.meshFilter != null )
                    _poolItem.meshFilter.sharedMesh = value;
                if( _poolItem.skinnedMeshRenderer != null )
                    _poolItem.skinnedMeshRenderer.sharedMesh = value;
            }
        }

        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Material Material
        {
            get
            {
                EnsureValid();
                return _poolItem.meshRenderer.sharedMaterial;
            }
            set
            {
                EnsureValid();
                if( _poolItem.meshRenderer != null )
                    _poolItem.meshRenderer.sharedMaterial = value;
                if( _poolItem.skinnedMeshRenderer != null )
                    _poolItem.skinnedMeshRenderer.sharedMaterial = value;
            }
        }

        public float Duration
        {
            get
            {
                EnsureValid();
                return _poolItem.duration;
            }
            set
            {
                EnsureValid();
                _poolItem.duration = value;
            }
        }
        public bool Loop
        {
            get
            {
                EnsureValid();
                return _poolItem.loop;
            }
            set
            {
                EnsureValid();
                _poolItem.loop = value;
            }
        }
        public Vector3 Position
        {
            get
            {
                EnsureValid();
                return _poolItem.localPosition;
            }
            set
            {
                EnsureValid();
                _poolItem.localPosition = value;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                EnsureValid();
                return _poolItem.localRotation;
            }
            set
            {
                EnsureValid();
                _poolItem.localRotation = value;
            }
        }
        public Vector3 Scale
        {
            get
            {
                EnsureValid();
                return _poolItem.localScale;
            }
            set
            {
                EnsureValid();
                _poolItem.localScale = value;
            }
        }

        public IReadOnlyList<Transform> Bones
        {
            get
            {
                EnsureValid();
                if( !this.IsSkinned )
                    throw new InvalidOperationException( "Cannot access bones of a non-skinned mesh." );
                return _poolItem.skinnedMeshRenderer.bones;
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