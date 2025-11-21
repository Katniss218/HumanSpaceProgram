using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Contains the data about a bulk resource (one that can be held in e.g. a tank). <br/>
    /// Examples: Propellant
    /// </summary>
    [Serializable]
    public class Substance : ISubstance
    {
        public string ID { get; }

        public string DisplayName { get; set; }

        public Color DisplayColor { get; set; }

        public string[] Tags { get; set; }



        public SubstancePhase Phase { get; set; }

        public double MolarMass { get; set; }

        public double SpecificGasConstant { get; set; } = 287;

        public double? FlashPoint { get; set; }

        /// <summary>
        /// For gases: unused. <br/>
        /// For liquids: The bulk modulus, in [Pa]. Typical liquids ~1e9 Pa. Lower values => more compressible. <br/>
        /// For solids: The bulk modulus.
        /// </summary>
        public float BulkModulus { get; set; } = 2e9f;

        public float ReferenceDensity { get; set; } = 1000f;

        /// <summary>
        /// Reference pressure at which <see cref="Density"/> applies (p0), in [Pa].
        /// </summary>
        public float ReferencePressure { get; set; } = 101325f;

        public Substance( string id )
        {
            ID = id;
        }

        public double GetPressure( double temperature, double density )
        {
            if( Phase == SubstancePhase.Gas )
            {
                if( SpecificGasConstant <= 0f || temperature <= 0f )
                    return 0f;
                return density * SpecificGasConstant * temperature;
            }

            if( BulkModulus <= 0f )
                return ReferencePressure;

            return ReferencePressure + BulkModulus * (density / ReferenceDensity - 1f);
        }

        public double GetDensity( double temperature, double pressure )
        {
            if( Phase == SubstancePhase.Gas )
            {
                if( SpecificGasConstant <= 0f || temperature <= 0f )
                    return 0f;
                return pressure / (SpecificGasConstant * temperature);
            }

            if( BulkModulus <= 0f )
                return ReferenceDensity;

            return ReferenceDensity * (1f + (pressure - ReferencePressure) / BulkModulus);
        }

        public double GetViscosity( double temperature, double pressure )
        {
            // TODO - implement viscosities for substances.
            return 0.001; // Pa.s (water at room temp)
        }
        public double GetSpeedOfSound( double temperature, double pressure )
        {
            throw new NotImplementedException();
        }


        public double GetThermalConductivity( double temperature, double pressure )
        {
            throw new NotImplementedException();
        }

        public double GetSpecificHeatCapacity( double temperature, double pressure )
        {
            throw new NotImplementedException();
        }

        public double GetLatentHeatOfVaporization( double temperature )
        {
            throw new NotImplementedException();
        }

        public double GetLatentHeatOfFusion( double temperature )
        {
            throw new NotImplementedException();
        }

        public double GetVaporPressure( double temperature )
        {
            // TODO - implement vapor pressure curves for substances.
            return 0.0;
        }

        public double GetBoilingPoint( double pressure )
        {
            throw new NotImplementedException();
        }

#warning TODO - boiling/melting at partial vacuum (vapor pressure).

        // Need to figure out how gasses will work too.
        // how about pre-mixed stuff, like air being nitrogen + oxygen, or kerosene being a mix of hydrocarbons?

        [MapsInheritingFrom( typeof( Substance ) )]
        public static SerializationMapping SubstanceMapping()
        {
            return new MemberwiseSerializationMapping<Substance>()
                .WithReadonlyMember( "id", o => o.ID )
                .WithFactory<string>( id => new Substance( id ) )
                .WithMember( "display_name", o => o.DisplayName )
                .WithMember( "display_color", o => o.DisplayColor )
                .WithMember( "tags", o => o.Tags )
                .WithMember( "phase", o => o.Phase )
                .WithMember( "molar_mass", o => o.MolarMass )
                .WithMember( "specific_gas_constant", o => o.SpecificGasConstant )
                .WithMember( "flash_point", o => o.FlashPoint )
                .WithMember( "bulk_modulus", o => o.BulkModulus )
                .WithMember( "reference_density", o => o.ReferenceDensity )
                .WithMember( "reference_pressure", o => o.ReferencePressure );
        }
    }
}