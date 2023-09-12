using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// Manages the speed at which the time flows.
    /// </summary>
    [DisallowMultipleComponent]
    public class TimeManager : MonoBehaviour
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
        //      All code modifying the time scale should use this class instead of `UnityEngine.Time.timeScale` for consistent behaviour.
        //

        /// <summary>
        /// The multiplier for the speed at which the time currently flows.
        /// </summary>
        public static float TimeScale { get => _timeScale; }

        /// <summary>
        /// Checks if the game is currently paused.
        /// </summary>
        public static bool IsPaused { get => _timeScale == 0.0f; }

        /// <summary>
        /// Prevents the time scale from being changed. Useful for e.g. asynchronous saving/loading.
        /// </summary>
        public static bool LockTimescale { get; set; } = false;

        /// <summary>
        /// Invoked when the current time scale is changed successfully (including when <see cref="TimeScaleChangedData.Old"/> and <see cref="TimeScaleChangedData.New"/> are the same).
        /// </summary>
        public static event Action<TimeScaleChangedData> OnTimescaleChanged;

#warning TODO - add a deltatime wrapper here (or split into several classes) because the deltatimes can be different.
        static float _maxTimeScale = 128.0f;

        private static float _timeScale;
        private static float _oldTimeScale = 0;

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
        /// Gets the current value of the time scale.
        /// </summary>
        public static float GetTimeScale()
        {
            return _timeScale;
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

            _oldTimeScale = _timeScale;
            _timeScale = timeScale;
            UnityEngine.Time.timeScale = timeScale;
            OnTimescaleChanged?.Invoke( new TimeScaleChangedData() { Old = _oldTimeScale, New = timeScale } );
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

        void Start()
        {
            SetTimeScale( 1 );
        }

        void Update()
        {
            if( LockTimescale ) // short-circuit exit before checking anything.
            {
                return;
            }

            if( Input.GetKeyDown( KeyCode.Period ) )
            {
                if( IsPaused )
                {
                    SetTimeScale( 1f );
                    return;
                }
                float newscale = _timeScale * 2f;

                if( newscale > GetMaxTimescale() )
                    return;
                SetTimeScale( newscale );
            }

            if( Input.GetKeyDown( KeyCode.Comma ) )
            {
                if( _timeScale <= 1f )
                    SetTimeScale( 0.0f );
                else
                    SetTimeScale( _timeScale / 2f );
            }
        }
    }
}