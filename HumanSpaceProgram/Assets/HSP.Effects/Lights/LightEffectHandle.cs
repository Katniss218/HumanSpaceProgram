using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Effects.Lights
{
    /// <summary>
    /// Represents an audio that has been prepared to play, and/or may be currently playing.
    /// </summary>
    public readonly struct LightEffectHandle : IEffectHandle
    {
        private readonly int _version;
        private readonly LightEffectPoolItem _poolItem;

        internal LightEffectHandle( LightEffectPoolItem poolItem )
        {
            _poolItem = poolItem;
            _version = poolItem.version;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void EnsureValid()
        {
            // Null check because the _poolItem might've been destroyyed for whatever reason.
            if( _poolItem == null || _version != _poolItem.version )
                throw new ObjectDisposedException( nameof( LightEffectHandle ), $"The {nameof( LightEffectPoolItem )} backing the {nameof( LightEffectHandle )} has been disposed or reused and is no longer valid for this handle." );
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

        public LightType Type
        {
            get
            {
                EnsureValid();
                return _poolItem.light.type;
            }
            set
            {
                EnsureValid();
                _poolItem.light.type = value;
            }
        }

        public float Intensity
        {
            get
            {
                EnsureValid();
                return _poolItem.light.intensity;
            }
            set
            {
                EnsureValid();
                _poolItem.light.intensity = value;
            }
        }

        public float Range
        {
            get
            {
                EnsureValid();
                return _poolItem.light.range;
            }
            set
            {
                EnsureValid();
                _poolItem.light.range = value;
            }
        }

        public float ConeAngle
        {
            get
            {
                EnsureValid();
                return _poolItem.light.spotAngle;
            }
            set
            {
                EnsureValid();
                _poolItem.light.spotAngle = value;
            }
        }

        public Color Color
        {
            get
            {
                EnsureValid();
                return _poolItem.light.color;
            }
            set
            {
                EnsureValid();
                _poolItem.light.color = value;
            }
        }

        public LightShadows Shadows
        {
            get
            {
                EnsureValid();
                return _poolItem.light.shadows;
            }
            set
            {
                EnsureValid();
                _poolItem.light.shadows = value;
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
        public int CullingMask
        {
            get
            {
                EnsureValid();
                return _poolItem.light.cullingMask;
            }
            set
            {
                EnsureValid();
                _poolItem.light.cullingMask = value;
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