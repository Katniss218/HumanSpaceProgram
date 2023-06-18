using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// Contains the data about a bulk resource (one that can be held in e.g. a tank). <br/>
    /// Examples: Propellant
    /// </summary>
    [Serializable]
    public class Substance
    {
        // kinda ugly, encapsulate, and put in a separate BulkResourceRegistry class even.
        public static Dictionary<string, Substance> RegisteredResources { get; set; } = new Dictionary<string, Substance>()
        {
            { "substance.f", new Substance() { Density = 1000, DisplayName = "Fuel", UIColor = new Color( 1.0f, 0.3764706f, 0.2509804f ) } },
            { "substance.ox", new Substance() { Density = 1000, DisplayName = "Oxidizer", UIColor = new Color( 0.2509804f, 0.5607843f, 1.0f ) } },
        };

        [field: SerializeField]
        public string DisplayName { get; set; }

        /// <summary>
        /// Density [kg/m^3]
        /// </summary>
        [field: SerializeField]
        public float Density { get; set; }

        /// <summary>
        /// The color of the substance, used when drawing UIs.
        /// </summary>
        [field: SerializeField]
        public Color UIColor { get; set; }
        // Need to figure out how gasses will work too.
        // how about pre-mixed stuff, like air being nitrogen + oxygen, or kerosene being a mix of hydrocarbons?
    }
}