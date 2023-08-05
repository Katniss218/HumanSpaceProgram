using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.UI
{
    public class VelocityReadoutUI : MonoBehaviour
    {
        [field: SerializeField]
        public TMPro.TextMeshProUGUI TextBox { get; set; }

        void LateUpdate()
        {
            TextBox.text = $"{VesselManager.ActiveVessel.PhysicsObject.Velocity.magnitude:#0} m/s";
        }
    }
}