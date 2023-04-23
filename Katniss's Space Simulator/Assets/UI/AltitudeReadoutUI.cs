using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.UI
{
    public class AltitudeReadoutUI : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI _textBox;

        void LateUpdate()
        {
            CelestialBody body = CelestialBodyManager.Bodies[0];
            Vector3 posV = VesselManager.ActiveVessel.transform.position;
            Vector3 posCB = SceneReferenceFrameManager.WorldSpaceReferenceFrame.InverseTransformPosition( body.AIRFPosition );

            float magn = (posV-posCB).magnitude;
            float alt = magn - (float)body.Radius;

            _textBox.text = $"Altitude: {(alt/1000):#0.00} km";
        }
    }
}
