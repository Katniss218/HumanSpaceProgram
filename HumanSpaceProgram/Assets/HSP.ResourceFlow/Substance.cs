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

    public interface ISubstance
    {
        string ID { get; }
        string DisplayName { get; }
        string[] Tags { get; } // for filtering. matches convention for parts, vessels, scenarios, etc.

        SubstancePhase Phase { get; }
        double MolarMass { get; }

        double GetDensity( double temperature, double pressure );
        double GetSpecificHeatCp( double temperature, double pressure );
        double GetViscosity( double temperature, double pressure );
        double GetVaporPressure( double temperature ); 
        
        double ToMass( double moles ); 
        double ToMoles( double mass ); // in [kg]
    }
    public static class ISubstance_Ex
    {
        /// <summary>
        /// Calculates the dynamic pressure of a flowing liquid (per unit volume).
        /// </summary>
        /// <param name="density">The density of the fluid, in [kg/m^3].</param>
        /// <param name="velocity">The velocity of the fluid, in [m/s].</param>
        /// <returns>The dynamic pressure in [Pa].</returns>
        public static float GetDynamicPressure( float density, float velocity )
        {
            return 0.5f * density * (velocity * velocity);
        }

        /// <summary>
        /// Calculates the static (hydrostatic) pressure at a given depth of liquid.
        /// </summary>
        /// <param name="density">The density of the fluid, in [kg/m^3].</param>
        /// <param name="height">The height of the fluid column (measured along the acceleration vector), in [m].</param>
        /// <param name="acceleration">The magnitude of the acceleration vector, in [m/s^2].</param>
        /// <returns>The static pressure in [Pa]</returns>
        public static float GetStaticPressure( float density, float height, float acceleration )
        {
            return acceleration * density * height;
        }

        public static float ComputeMinimalLiquidVolume( SubstanceState[] substances )
        {
            if( substances == null )
                throw new ArgumentNullException( nameof( substances ) );

            float sumVolume = 0f;
            foreach( var s in substances )
            {
                if( s.Substance.Phase != SubstancePhase.Liquid )
                    continue;

                float density = s.Substance.Density;
                if( density <= 0f )
                    continue;
                sumVolume += s.MassAmount / density;
            }

            return sumVolume;
        }

        public static float TotalReferenceLiquidVolume( SubstanceStateCollection states )
        {
            float Vref = 0f;
            foreach( var s in states )
            {
                if( s.Substance.Phase != SubstancePhase.Gas )
                {
                    float rho0 = s.Substance.Density;
                    if( rho0 <= 0f )
                        continue;
                    Vref += s.MassAmount / rho0;
                }
            }
            return Vref;
        }

        public static SubstancePhase GetEquilibriumPhase( ISubstance substance, double temperature, double pressure )
        {
            double pSat = substance.GetVaporPressure( temperature );
            if( double.IsNaN( pSat ) )
            {
                // No meaningful vapor pressure known - assume condensed (liquid/solid).
                return SubstancePhase.Liquid;
            }

            if( pressure <= pSat )
                return SubstancePhase.Gas;
            return SubstancePhase.Liquid;
        }

        // ...


        /// <summary>
        /// Computes the results of a vaporization at the specific volume and temperature.
        /// </summary>
        public static (ISubstanceStateCollection, FluidState) ComputeFlash( IReadonlySubstanceStateCollection contents, FluidState input, double volume )
        {
#warning TODO - this is a very rough approximation. Improve later with better compressibility models.
            if( contents == null || contents.Length == 0 )
                throw new ArgumentException( "contents must contain at least one SubstanceState", nameof( contents ) );
            if( volume <= 0f )
                throw new ArgumentOutOfRangeException( nameof( volume ) );

            // Total mass and quick-phase totals
            float totalMass = 0f;
            float gasMass = 0f;
            float liquidMass = 0f;

            foreach( var s in contents )
            {
                totalMass += s.MassAmount;
                if( s.Substance.Phase == SubstancePhase.Gas )
                    gasMass += s.MassAmount;
                else liquidMass += s.MassAmount;
            }

            // If totalMass is zero - treat as near-vacuum gas at zero pressure.
            if( totalMass <= 1e-5f )
                return 0f;

            // Decide predominant phase by mass (simple heuristic)
            bool predominantlyGas = gasMass >= liquidMass;

            if( predominantlyGas )
            {
                // mass-weighted R and T
                float Rw = 0f;
                float Tw = 0f;
                foreach( var s in contents )
                {
                    float w = s.MassAmount / totalMass;
                    Rw += w * s.Substance.SpecificGasConstant;
                    Tw += w * Temperature;
                }
                // Guard against invalid numbers
                if( Rw <= 0f || Tw <= 0f )
                    return 0f;
                return SubstanceState.GasPressureFromMassAndVolume( totalMass, volume, Rw, Tw );
            }
            else
            {
                // Liquids (or liquids + small gas fraction) - approximate with mass-weighted reference properties
#warning TODO - needs to handle the case when there is too much volume for the liquid to fill.
                float rho0w = 0f;
                float p0w = 0f;
                float Kw = 0f;
                foreach( var s in contents )
                {
                    float w = s.MassAmount / totalMass;
                    rho0w += w * s.Substance.Density;
                    p0w += w * s.Substance.ReferencePressure;
                    Kw += w * s.Substance.BulkModulus;
                }

                return SubstanceState.LiquidPressureFromMassAndVolume( totalMass, volume, rho0w, p0w, Kw );
            }
        }
    }

    /// <summary>
    /// Contains the data about a bulk resource (one that can be held in e.g. a tank). <br/>
    /// Examples: Propellant
    /// </summary>
    [Serializable]
    public class Substance : ISubstance
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

        /// <summary>
        /// In [kg/mol].
        /// </summary>
        public double MolarMass { get; set; }

#warning TODO - boiling/melting at partial vacuum (vapor pressure).

        /// <summary> Compute pressure from gas mass m, volume V, R and T:
        /// p = (m/V) * R * T
        /// </summary>
        public static float GasPressureFromMassAndVolume( float mass, float volume, float R, float T )
        {
            if( volume <= 0f )
                throw new ArgumentOutOfRangeException( nameof( volume ), "Volume must be positive and non-zero." );
            float rho = mass / volume;
            return rho * R * T;
        }
        // Given p, V, R, T => compute mass m = p * V / (R * T)
        public static float GasMassFromPressureAndVolume( float p, float V, float R, float T )
        {
            if( R <= 0f || T <= 0f || V <= 0f )
                return 0f;
            return (p * V) / (R * T);
        }

        public float GetVolumeAtPressure( float mass, float pressure, float Temperature )
        {
            if( pressure <= 0f )
                throw new ArgumentOutOfRangeException( nameof( pressure ), "Pressure must be positive and non-zero." );

            if( Phase == SubstancePhase.Gas )
            {
                if( SpecificGasConstant <= 0f || Temperature <= 0f )
                    return float.PositiveInfinity;
                float density = pressure / (SpecificGasConstant * Temperature);
                return mass / density;
            }
            else // liquids and solids.
            {
                if( BulkModulus <= 0f )
                    return mass / Density;
                float density = Density * (1f + (pressure - ReferencePressure) / BulkModulus);
                return mass / density;
            }
        }

        public float GetPressureAtVolume( float mass, float volume, float Temperature )
        {
            if( volume <= 0f )
                throw new ArgumentOutOfRangeException( nameof( volume ), "Volume must be positive and non-zero." );

            if( Phase == SubstancePhase.Gas )
            {
                float rho = mass / volume;
                return rho * SpecificGasConstant * Temperature;
            }
            else // liquids and solids.
            {
                float rho = mass / volume;
                return ReferencePressure + BulkModulus * (rho / Density - 1f);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mass">in [kg]</param>
        /// <returns></returns>
        public double GetMoles( double mass )
        {
            if( MolarMass <= 0.0 )
                return 0.0;
            return mass * MolarMass;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moles"></param>
        /// <returns>The mass in [kg]</returns>
        public double GetMass( double moles )
        {
            if( MolarMass <= 0.0 )
                return 0.0;
            return moles * MolarMass;
        }

        /// <summary>
        /// For liquids: given mass and volume, compute pressure p such that rho = mass/volume = rho0*(1+(p-p0)/K)
        /// Solve for p: p = p0 + K * (rho/rho0 - 1)
        /// </summary>
        public static float LiquidPressureFromMassAndVolume( float mass, float volume, float rho0, float p0, float bulkModulus )
        {
            if( volume <= 0f )
                throw new ArgumentOutOfRangeException( nameof( volume ), "Volume must be positive and non-zero." );
            float rho = mass / volume;
            return p0 + bulkModulus * (rho / rho0 - 1f);
        }

        public float GetDensityAtPressure( float pressure, float Temperature )
        {
            if( Phase == SubstancePhase.Gas )
            {
                if( SpecificGasConstant <= 0f || Temperature <= 0f )
                    return 0f;
                return pressure / (SpecificGasConstant * Temperature);
            }
            if( BulkModulus <= 0f )
                return Density;
            return Density * (1f + (pressure - ReferencePressure) / BulkModulus);
        }

        public float GetReferenceDensity()
        {
            // International Standard Atmosphere
            const float standardPressure = 101325f; // Pa
            const float standardTemperature = 288.15f; // K (15 C)

            if( Phase == SubstancePhase.Gas )
            {
                if( SpecificGasConstant <= 0f || standardTemperature <= 0f )
                    return 0f;
                return standardPressure / (SpecificGasConstant * standardTemperature);
            }
            if( BulkModulus <= 0f )
                return Density;
            return Density * (1f + (standardPressure - ReferencePressure) / BulkModulus);
        }

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