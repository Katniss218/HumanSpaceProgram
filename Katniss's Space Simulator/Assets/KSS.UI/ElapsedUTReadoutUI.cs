using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    /// <summary>
    /// A readout for the mission elapsed time.
    /// </summary>
    public class ElapsedUTReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        public double ReferenceUT { get; set; }

        void LateUpdate()
        {
            Text.Text = $"{(TimeStepManager.UT - ReferenceUT):#0} s";
        }
    }
}