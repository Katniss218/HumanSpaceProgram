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
        [SerializeField]
        TMPro.TextMeshProUGUI _textBox;

        void LateUpdate()
        {
            _textBox.text = $"Velocity: {VesselManager.ActiveVessel.PhysicsObject.Velocity.magnitude:#0} m/s";
        }
    }
}