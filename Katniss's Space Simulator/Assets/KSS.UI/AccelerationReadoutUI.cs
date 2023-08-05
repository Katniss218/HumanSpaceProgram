using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.UI
{
    public class AccelerationReadoutUI : MonoBehaviour
    {
        [field: SerializeField]
        public TMPro.TextMeshProUGUI TextBox { get; set; }

        void LateUpdate()
        {
            TextBox.text = $"Acceleration: {VesselManager.ActiveVessel.PhysicsObject.Acceleration.magnitude:#0.0} m/s^2";
        }
    }
}