using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.UI
{
    public class FPSCounterUI : MonoBehaviour
    {
        [field: SerializeField]
        public TMPro.TextMeshProUGUI TextBox { get; set; }

        float[] _fps = new float[128];
        int _max = 0;
        int _index = 0;

        private float GetFps()
        {
            return 1.0f / Time.unscaledDeltaTime; // unscaled so timewarp / pausing doesn't fuck with it.
        }

        private void SampleFPS()
        {
            _fps[_index] = GetFps();
            _index = (_index + 1) % _fps.Length;
            if( _max < _fps.Length )
            {
                _max++;
            }
        }

        private float GetAverageFps()
        {
            float acc = 0;
            for( int i = 0; i < _max; i++ )
            {
                acc += _fps[i];
            }
            acc /= _max;
            return acc;
        }

        void Update()
        {
            SampleFPS();
            TextBox.text = $"FPS: {Mathf.CeilToInt( GetAverageFps() )}";
        }
    }
}