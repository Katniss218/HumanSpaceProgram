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
    /// A readout for the current universal time.
    /// </summary>
    public class UTReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        void LateUpdate()
        {
            Text.Text = $"{TimeStepManager.UT:#0} s";
        }
    }
}