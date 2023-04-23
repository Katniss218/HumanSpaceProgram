using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class CelestialBody : MonoBehaviour
    {
        public Vector3Dbl AIRFPosition { get; set; } // fixed static body for now, global position.

        public double Mass { get; set; }
        public double Radius { get; set; }
    }
}
