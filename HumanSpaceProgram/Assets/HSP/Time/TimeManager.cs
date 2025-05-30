﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;
using UnityPlus.Serialization;

namespace HSP.Time
{
    /// <summary>
    /// Manages the speed at which the time flows.
    /// </summary>
    public class TimeManager : SingletonMonoBehaviour<TimeManager>
    {
        public struct TimeScaleChangedData
        {
            /// <summary>
            /// The old time scale (before it was updated).
            /// </summary>
            public float Old { get; set; }
            /// <summary>
            /// The new time scale (after it was updated).
            /// </summary>
            public float New { get; set; }
        }

        //
        //      All code relating to time should use this class instead of `UnityEngine.Time` for consistent behaviour.
        //

        /// <summary>
        /// The multiplier for the speed at which the time currently flows.
        /// </summary>
        public static float TimeScale { get => _timeScale; }

        /// <summary>
        /// Returns the current frame's universal time, in [s].
        /// </summary>
        public static double UT { get; private set; }

        public static float FixedUnscaledDeltaTime { get => UnityEngine.Time.fixedUnscaledDeltaTime; }
        public static float UnscaledDeltaTime { get => UnityEngine.Time.unscaledDeltaTime; }

        /// <summary>
        /// Returns the delta-time for the current frame.
        /// </summary>
        public static float DeltaTime { get => UnityEngine.Time.deltaTime; } // TODO - use when implementing on-rails warp and variable-step time

        /// <summary>
        /// Returns the fixed delta-time (use in FixedUpdate) for the current frame.
        /// </summary>
        public static float FixedDeltaTime { get => UnityEngine.Time.fixedDeltaTime; } // TODO - use when implementing on-rails warp and variable-step time

        /// <summary>
        /// Checks if the game is currently paused.
        /// </summary>
        public static bool IsPaused { get => _timeScale == 0.0f; }

        /// <summary>
        /// Prevents the time scale from being changed. Useful for e.g. asynchronous saving/loading.
        /// </summary>
        public static bool LockTimescale { get; set; } = false;

        /// <summary>
        /// Invoked after the current time scale is changed successfully (including when <see cref="TimeScaleChangedData.Old"/> and <see cref="TimeScaleChangedData.New"/> are the same).
        /// </summary>
        public static event Action<TimeScaleChangedData> OnAfterTimescaleChanged;

        static float _maxTimeScale = 128.0f;

        private static float _timeScale = 1;
        private static float _oldTimeScale = 1;

        /// <summary>
        /// Gets the current maximum time scale.
        /// </summary>
        public static float GetMaxTimescale()
        {
#if UNITY_EDITOR
            return 100f;
#else
            return _maxTimeScale;
#endif
        }

        /// <summary>
        /// Sets the current maximum time scale.
        /// </summary>
        public static void SetMaxTimeScale( float value )
        {
#if UNITY_EDITOR
            if( value > 100f )
            {
                Debug.LogWarning( $"Inside Unity Editor, timeScale can be at most 100 :(." );
                value = 100f;
            }
#endif
            _maxTimeScale = value;
        }

        /// <summary>
        /// Sets the current time scale to the specified value, if the time scale is not locked.
        /// </summary>
        public static void SetTimeScale( float timeScale )
        {
            if( timeScale < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( timeScale ), $"Time scale must be greater or equal to 0." );
            }
            float max = GetMaxTimescale();
            if( timeScale > max )
            {
                throw new ArgumentOutOfRangeException( nameof( timeScale ), $"Time scale must be smaller or equal to maximum time scale (currently {max})." );
            }

            if( LockTimescale )
            {
                return;
            }

            //  UnityEngine.Time.fixedDeltaTime = Mathf.Clamp( 0.02f * (timeScale / 8.0f), 0.02f, 0.08f );

            _oldTimeScale = _timeScale;
            _timeScale = timeScale;
            UnityEngine.Time.timeScale = timeScale;
            OnAfterTimescaleChanged?.Invoke( new TimeScaleChangedData() { Old = _oldTimeScale, New = timeScale } );
        }

        public static void SetUT( double ut )
        {
            UT = ut;
        }

        /// <summary>
        /// Pauses the game (sets the current time scale to 0), if the time scale is not locked.
        /// </summary>
        public static void Pause()
        {
            SetTimeScale( 0.0f );
        }

        /// <summary>
        /// Unpauses the game (sets the current time scale to its previous value), if the time scale is not locked.
        /// </summary>
        public static void Unpause()
        {
            if( _oldTimeScale == 0 )
                SetTimeScale( 1.0f );
            else
                SetTimeScale( _oldTimeScale );
        }

        //
        // ---
        //

        void Awake()
        {
            UnityEngine.Time.fixedDeltaTime = 0.02f;
            UnityEngine.Time.maximumDeltaTime = 0.06f;
            UnityEngine.Time.maximumParticleDeltaTime = 0.03f;
        }

        void Start()
        {
            SetTimeScale( 1 );
        }


        void OnEnable()
        {
            PlayerLoopUtils.InsertSystemBefore<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.ClearLines ) );
        }

        void OnDisable()
        {
            PlayerLoopUtils.RemoveSystem<FixedUpdate>( in _playerLoopSystem );
        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( TimeManager ),
            updateDelegate = PlayerLoopImmediatelyAtTheStartOfFixedUpdate,
            subSystemList = null
        };

        private static void PlayerLoopImmediatelyAtTheStartOfFixedUpdate()
        {
            if( !instanceExists )
                return;

            UT += FixedDeltaTime;
        }


        [MapsInheritingFrom( typeof( TimeManager ) )]
        public static SerializationMapping TimeStepManagerMapping()
        {
            return new MemberwiseSerializationMapping<TimeManager>()
                .WithMember( "ut", o => TimeManager.UT, ( o, value ) => TimeManager.UT = value );
        }
    }
}