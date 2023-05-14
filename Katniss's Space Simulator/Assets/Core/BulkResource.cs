using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// Contains the data about a bulk resource (one that can be held in e.g. a tank). <br/>
    /// Examples: Propellant
    /// </summary>
    public class BulkResource : IIdentifiable
    {
        // kinda ugly, encapsulate, and put in a separate BulkResourceRegistry class even.
        public static Dictionary<string, BulkResource> RegisteredResources { get; set; } = new Dictionary<string, BulkResource>();

        public string ID { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// Density [kg/m^3] vs Temperature [K]
        /// </summary>
        public Func<float, float> DensityCurve { get; set; }

        // Need to figure out how gasses will work too.

        // how about pre-mixed stuff?
    }
}