using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class CelestialBodyManager : MonoBehaviour
    {
        public static List<CelestialBody> Bodies { get; set; } = new List<CelestialBody>();
    }
}