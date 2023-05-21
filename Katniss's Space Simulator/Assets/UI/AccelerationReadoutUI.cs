using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.UI
{
    public class AccelerationReadoutUI : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI _textBox;

        void LateUpdate()
        {
            _textBox.text = $"Acceleration: {VesselManager.ActiveVessel.PhysicsObject.Acceleration.magnitude:#0.0} m/s^2";
        }
    }
}