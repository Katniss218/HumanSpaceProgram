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
    public class AccelerationReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        void LateUpdate()
        {
            Text.Text = $"Acceleration: {VesselManager.ActiveVessel.PhysicsObject.Acceleration.magnitude:#0.0} m/s^2";
        }
    }
}