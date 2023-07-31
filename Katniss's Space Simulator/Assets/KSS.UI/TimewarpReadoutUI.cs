using KSS.Core.TimeWarp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.UI
{
    public class TimewarpReadoutUI : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI _textBox;

        void Start()
        {
            TimeWarpManager.OnTimescaleChanged += OnTimescaleChanged_Listener;
        }

        void UpdateText( float rate )
        {
            if( rate == 0 )
            {
                _textBox.text = $"Warp Rate: PAUSED";
                return;
            }

            _textBox.text = $"Warp Rate: {rate}x";
        }

        void OnTimescaleChanged_Listener( TimeWarpManager.TimeScaleChangedData data )
        {
            UpdateText( data.New );
        }
    }
}