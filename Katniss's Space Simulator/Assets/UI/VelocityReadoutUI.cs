using KatnisssSpaceSimulator.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.UI
{
    public class VelocityReadoutUI : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI _textBox;

        void LateUpdate()
        {
            _textBox.text = $"Velocity: {VesselManager.ActiveVessel.PhysicsObject.Velocity.magnitude:#0.00} m/s";
        }
    }
}
