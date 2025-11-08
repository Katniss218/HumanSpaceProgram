using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    public enum SubstancePhase
    {
        /// <summary>
        /// Gas phase, follows ideal gas law.
        /// </summary>
        Gas,
        /// <summary>
        /// Liquid phase, weakly compressible.
        /// </summary>
        Liquid,
        /// <summary>
        /// Solid phase, weakly compressible.
        /// </summary>
        Solid
    }

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
        /// The color of the substance, used when drawing UIs.
        /// </summary>
        [field: SerializeField]
        public Color UIColor { get; set; }

        /// <summary>
        /// Phase of this substance (gas / liquid / solid).
        /// </summary>
        [field: SerializeField]
        public SubstancePhase Phase { get; set; }

        /// <summary>
        /// For gases: unused.
        /// For liquids: The density at <see cref="ReferencePressure"/>, in [kg/m^3]
        /// For solids: The density at <see cref="ReferencePressure"/>, in [kg/m^3]
        /// </summary>
        [field: SerializeField]
        public float Density { get; set; }

        /// <summary>
        /// For gases: The specific gas constant, in [J/(kg*K)]. <br/>
        /// For liquids: unused.
        /// For solids: unused.
        /// </summary>
        [field: SerializeField]
        public float SpecificGasConstant { get; set; } = 287f;

        /// <summary>
        /// For gases: unused. <br/>
        /// For liquids: The bulk modulus, in [Pa]. Typical liquids ~1e9 Pa. Lower values => more compressible. <br/>
        /// For solids: The bulk modulus.
        /// </summary>
        [field: SerializeField]
        public float BulkModulus { get; set; } = 2e9f;

        /// <summary>
        /// Reference pressure at which <see cref="Density"/> applies (p0), in [Pa].
        /// </summary>
        [field: SerializeField]
        public float ReferencePressure { get; set; } = 101325f;

#warning TODO - boiling/melting at partial vacuum (vapor pressure).

        // Need to figure out how gasses will work too.
        // how about pre-mixed stuff, like air being nitrogen + oxygen, or kerosene being a mix of hydrocarbons?

        [MapsInheritingFrom( typeof( Substance ) )]
        public static SerializationMapping SubstanceMapping()
        {
            return new MemberwiseSerializationMapping<Substance>()
                .WithMember( "display_name", o => o.DisplayName )
                .WithMember( "density", o => o.Density )
                .WithMember( "display_color", o => o.UIColor )
                .WithMember( "phase", o => o.Phase )
                .WithMember( "specific_gas_constant", o => o.SpecificGasConstant )
                .WithMember( "bulk_modulus", o => o.BulkModulus )
                .WithMember( "reference_pressure", o => o.ReferencePressure );
        }
    }
}