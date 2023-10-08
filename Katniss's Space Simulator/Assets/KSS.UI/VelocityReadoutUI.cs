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
    public class VelocityReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        void LateUpdate()
        {
            Text.Text = VesselManager.ActiveVessel == null ? "" : $"{VesselManager.ActiveVessel.PhysicsObject.Velocity.magnitude:#0} m/s";
        }
    }
}