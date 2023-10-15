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
                CelestialBody body = CelestialBodyManager.Get( "main" );
                Vector3Dbl posV = VesselManager.ActiveVessel.AIRFPosition;
                Vector3Dbl posCB = body.AIRFPosition;

                double magn = (posV - posCB).magnitude;
                double alt = magn - body.Radius;

                Text.Text = $"Altitude: {(alt / 1000.0):#0} km";
            }
        }
    }
}
