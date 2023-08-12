using KSS.Core.TimeWarp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class TimewarpReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        void Start()
        {
            TimeWarpManager.OnTimescaleChanged += OnTimescaleChanged_Listener;
            UpdateText( TimeWarpManager.TimeScale );
        }

        void UpdateText( float rate )
        {
            if( rate == 0 )
            {
                Text.text = $"Warp Rate: PAUSED";
                return;
            }

            Text.text = $"Warp Rate: {rate}x";
        }

        void OnTimescaleChanged_Listener( TimeWarpManager.TimeScaleChangedData data )
        {
            UpdateText( data.New );
        }
    }
}