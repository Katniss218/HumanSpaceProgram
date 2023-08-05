using KSS.Core.TimeWarp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.UI
{
    public class TimewarpReadoutUI : MonoBehaviour
    {
        [field: SerializeField]
        public TMPro.TextMeshProUGUI TextBox { get; set; }

        void Start()
        {
            TimeWarpManager.OnTimescaleChanged += OnTimescaleChanged_Listener;
            UpdateText( TimeWarpManager.TimeScale );
        }

        void UpdateText( float rate )
        {
            if( rate == 0 )
            {
                TextBox.text = $"Warp Rate: PAUSED";
                return;
            }

            TextBox.text = $"Warp Rate: {rate}x";
        }

        void OnTimescaleChanged_Listener( TimeWarpManager.TimeScaleChangedData data )
        {
            UpdateText( data.New );
        }
    }
}