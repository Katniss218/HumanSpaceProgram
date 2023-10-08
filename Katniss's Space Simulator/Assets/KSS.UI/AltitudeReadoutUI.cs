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
            if( VesselManager.ActiveVessel == null )
            {
                Text.Text = "";
            }
            else
            {
                CelestialBody body = CelestialBodyManager.CelestialBodies[0];
                Vector3 posV = VesselManager.ActiveVessel.transform.position;
                Vector3 posCB = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( body.AIRFPosition );

                float magn = (posV - posCB).magnitude;
                float alt = magn - (float)body.Radius;

                Text.Text = $"Altitude: {(alt / 1000.0f):#0} km";
            }
        }
    }
}
