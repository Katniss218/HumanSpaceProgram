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
    public class Substance : IIdentifiable
    {
        public enum Type
        {
            Inert,
            Fuel,
            Oxidizer,
            Combined
        }

        // kinda ugly, encapsulate, and put in a separate BulkResourceRegistry class even.
        public static Dictionary<string, Substance> RegisteredResources { get; set; } = new Dictionary<string, Substance>();

        [field: SerializeField]
        public string ID { get; set; }

        [field: SerializeField]
        public string DisplayName { get; set; }

        /// <summary>
        /// Density [kg/m^3]
        /// </summary>
        [field: SerializeField]
        public float Density { get; set; }

        /// <summary>
        /// The type of the substance. Mostly for visual purposes.
        /// </summary>
        [field: SerializeField]
        public Type SubstanceType { get; set; }
        // Need to figure out how gasses will work too.
        // how about pre-mixed stuff, like air being nitrogen + oxygen, or kerosene being a mix of hydrocarbons?
    }
}