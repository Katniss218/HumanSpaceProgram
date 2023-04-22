using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Managers
{
    public class TimeWarpManager : MonoBehaviour
    {
        public struct TimeScaleChangedData
        {
            public float Old { get; set; }
            public float New { get; set; }
        }

        private static float _timeScale;

        public static bool IsPaused { get => _timeScale == 0.0f; }

        public static event Action<TimeScaleChangedData> OnTimescaleChanged;

        public static void Pause()
        {
            SetTimeScale( 0.0f );
        }

        public static void SetTimeScale( float timeScale )
        {
            if( timeScale < 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( timeScale ), "Time scale must be nonnegative." );
            }

            float oldTimeScale = _timeScale;
            _timeScale = timeScale;
            Time.timeScale = timeScale;
            OnTimescaleChanged?.Invoke( new TimeScaleChangedData() { Old = oldTimeScale, New = timeScale } );
        }

        public static float GetTimeScale()
        {
            return _timeScale;
        }

        void Start()
        {
            SetTimeScale( 1 );
        }

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.Period ) )
            {
                if( IsPaused )
                {
                    SetTimeScale( 1 );
                    return;
                }
                if( _timeScale >= 16 )
                    return;
                SetTimeScale( _timeScale * 2f );
            }
            if( Input.GetKeyDown( KeyCode.Comma ) )
            {
                if( _timeScale <= 1 )
                    Pause();
                else
                    SetTimeScale( _timeScale / 2f );
            }
        }
    }
}