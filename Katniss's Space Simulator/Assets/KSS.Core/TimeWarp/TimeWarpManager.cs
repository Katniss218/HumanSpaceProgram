using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.TimeWarp
{
    /// <summary>
    /// Manages the speed at which the time flows.
    /// </summary>
    [DisallowMultipleComponent]
    public class TimeWarpManager : MonoBehaviour
    {
        public struct TimeScaleChangedData
        {
            /// <summary>
            /// The old timescale (before it was updated).
            /// </summary>
            public float Old { get; set; }
            /// <summary>
            /// The new timescale (after it was updated).
            /// </summary>
            public float New { get; set; }
        }

        /// <summary>
        /// The current timescale.
        /// </summary>
        public static float TimeScale { get => _timeScale; }

        /// <summary>
        /// Checks if the game is currently paused.
        /// </summary>
        public static bool IsPaused { get => _timeScale == 0.0f; }

        /// <summary>
        /// Invoked when the current timescale is changed.
        /// </summary>
        public static event Action<TimeScaleChangedData> OnTimescaleChanged;

        static float _maxTimeScale = 128.0f;

        private static float _timeScale;
        private static float _oldTimeScale = 0;

        /// <summary>
        /// Gets the current maximum timescale.
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
        /// Sets the current maximum timescale.
        /// </summary>
        public static void SetMaxTimeScale( float value )
        {
#if UNITY_EDITOR
            if( value > 100f )
            {
                Debug.LogWarning( $"Inside Unity Editor, timescale can be at most 100 :(." );
                value = 100f;
            }
#endif
            _maxTimeScale = value;
        }

        /// <summary>
        /// Pauses the game (sets the timescale to 0).
        /// </summary>
        public static void Pause()
        {
            SetTimeScale( 0.0f );
        }

        /// <summary>
        /// Unpauses the game (sets the timescale to its previous value).
        /// </summary>
        public static void Unpause()
        {
            if( _oldTimeScale == 0 )
                SetTimeScale( 1.0f );
            else
                SetTimeScale( _oldTimeScale );
        }

        /// <summary>
        /// Gets the current value of the timescale.
        /// </summary>
        public static float GetTimeScale()
        {
            return _timeScale;
        }

        /// <summary>
        /// Sets the timescale to the specified value.
        /// </summary>
        public static void SetTimeScale( float timeScale )
        {
            if( timeScale < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( timeScale ), $"Timescale must be greater or equal to 0." );
            }
            float max = GetMaxTimescale();
            if( timeScale > max )
            {
                throw new ArgumentOutOfRangeException( nameof( timeScale ), $"Timescale must be smaller or equal to maximum timescale (currently {max})." );
            }

            _oldTimeScale = _timeScale;
            _timeScale = timeScale;
            Time.timeScale = timeScale;
            OnTimescaleChanged?.Invoke( new TimeScaleChangedData() { Old = _oldTimeScale, New = timeScale } );
        }

        //
        // ---
        //

        /// <summary>
        /// Use this to disable changing the timescale by user input.
        /// </summary>
        /// <remarks>
        /// This only prevents the internal user input queries from triggering. Any external user input must be checked separately using this property.
        /// </remarks>
        public static bool PreventPlayerChangingTimescale { get; set; } = false;

        void Start()
        {
            SetTimeScale( 1 );
        }

        void Update()
        {
            if( PreventPlayerChangingTimescale )
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