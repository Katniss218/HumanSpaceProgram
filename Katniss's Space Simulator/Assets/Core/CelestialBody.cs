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

#warning TODO - orbits.

        public Vector3Large GIRFPosition { get; set; } // fixed static body for now, global position.

        public double Mass { get; set; }
        public double Radius { get; set; }
    }
}
