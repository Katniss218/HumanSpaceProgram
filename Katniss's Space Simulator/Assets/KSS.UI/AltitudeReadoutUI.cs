using KSS.Core;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class AltitudeReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        void LateUpdate()
        {
            CelestialBody body = CelestialBodyManager.Bodies[0];
            Vector3 posV = VesselManager.ActiveVessel.transform.position;
            Vector3 posCB = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( body.AIRFPosition );

            float magn = (posV - posCB).magnitude;
            float alt = magn - (float)body.Radius;

            Text.text = $"Altitude: {(alt / 1000.0f):#0} km";
        }
    }
}
