using KSS.Core;
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
            TimeManager.OnTimescaleChanged += OnTimescaleChanged_Listener;
            UpdateText( TimeManager.TimeScale );
        }

        void UpdateText( float rate )
        {
            if( rate == 0 )
            {
                Text.Text = $"Warp Rate: PAUSED";
                return;
            }

            Text.Text = $"Warp Rate: {rate}x";
        }

        void OnTimescaleChanged_Listener( TimeManager.TimeScaleChangedData data )
        {
            UpdateText( data.New );
        }
    }
}