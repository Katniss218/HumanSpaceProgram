using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.Core
{
    /// <summary>
    /// Contains the data about a bulk resource (one that can be held in e.g. a tank). <br/>
    /// Examples: Propellant
    /// </summary>
    [Serializable]
    public class Substance
    {
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