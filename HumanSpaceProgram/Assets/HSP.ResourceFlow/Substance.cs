using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    [Serializable]
    public class Substance : ISubstance
    {
        // --- Backing Fields for Reciprocal Caching ---
        private double _specificGasConstant = 287;
        private double _invSpecificGasConstant = 1.0 / 287.0; // Cache 1/R

        private double _bulkModulus = 2e9f;
        private double _invBulkModulus = 1.0 / 2e9; // Cache 1/K

        private double _referenceDensity = 1000f;
        private double _invReferenceDensity = 1.0 / 1000.0; // Cache 1/rho

        public string ID { get; }
        public string DisplayName { get; set; }
        public Color DisplayColor { get; set; }
        public string[] Tags { get; set; }
        public SubstancePhase Phase { get; set; }
        public double MolarMass { get; set; }
        public double AdiabaticIndex { get; set; } = 1.4;
        public double? FlashPoint { get; set; }
        public double ReferencePressure { get; set; } = 101325f;

        public double SpecificGasConstant
        {
            get => _specificGasConstant;
            set
            {
                _specificGasConstant = value;
                // Avoid divide by zero if user sets bad data
                _invSpecificGasConstant = (Math.Abs( value ) > 1e-9) ? 1.0 / value : 0.0;
            }
        }

        public double BulkModulus
        {
            get => _bulkModulus;
            set
            {
                _bulkModulus = value;
                _invBulkModulus = (Math.Abs( value ) > 1e-9f) ? 1.0 / value : 0.0;
            }
        }

        public double ReferenceDensity
        {
            get => _referenceDensity;
            set
            {
                _referenceDensity = value;
                _invReferenceDensity = (Math.Abs( value ) > 1e-9f) ? 1.0 / value : 0.0;
            }
        }

        // --- Coefficients ---
        public double[] ViscosityCoeffs { get; set; } = new double[] { 0.001 };
        public double[] ConductivityCoeffs { get; set; } = new double[] { 0.6 };
        public double[] SpecificHeatCoeffs { get; set; } = new double[] { 4184 };
        public double[] AntoineCoeffs { get; set; } = new double[] { 10.196, 1730.63, -39.724 };
        public double LatentHeatVap { get; set; } = 2.26e6;
        public double LatentHeatFusion { get; set; } = 3.34e5;

        public Substance( string id )
        {
            ID = id;
            // Ensure reciprocals are set correctly on init
            _invSpecificGasConstant = 1.0 / _specificGasConstant;
            _invBulkModulus = 1.0 / _bulkModulus;
            _invReferenceDensity = 1.0 / _referenceDensity;
        }

        public double GetPressure( double temperature, double density )
        {
            if( Phase == SubstancePhase.Gas )
            {
                if( temperature <= 0f ) return 0f;
                return density * _specificGasConstant * temperature;
            }

            // Optimization: Use cached inverse ReferenceDensity to multiply instead of divide
            // P = P0 + K * (rho * (1/rho0) - 1)
            return ReferencePressure + _bulkModulus * (density * _invReferenceDensity - 1.0);
        }

        public double GetDensity( double temperature, double pressure )
        {
            if( Phase == SubstancePhase.Gas )
            {
                if( temperature <= 0f ) return 0f;
                // Optimization: pressure * (1/R) * (1/T)
                return pressure * _invSpecificGasConstant * (1.0 / temperature);
            }

            // Optimization: Use cached inverse BulkModulus
            // Rho = Rho0 * (1 + (P - P0) * (1/K))
            double density = _referenceDensity * (1.0 + (pressure - ReferencePressure) * _invBulkModulus);

            // Realism: Prevent negative density (vacuum/suction limits)
            return density > 1e-6 ? density : 1e-6;
        }

        public double GetViscosity( double temperature, double pressure )
        {
            double v = EvaluatePolynomialHorner( temperature, ViscosityCoeffs );
            return v < 1e-9 ? 1e-9 : v;
        }

        public double GetThermalConductivity( double temperature, double pressure )
        {
            double k = EvaluatePolynomialHorner( temperature, ConductivityCoeffs );
            return k < 0 ? 0 : k;
        }

        public double GetSpecificHeatCapacity( double temperature, double pressure )
        {
            double cp = EvaluatePolynomialHorner( temperature, SpecificHeatCoeffs );
            return cp < 0 ? 0 : cp;
        }

        public double GetSpeedOfSound( double temperature, double pressure )
        {
            if( Phase == SubstancePhase.Gas )
            {
                if( temperature <= 0 ) return 0;
                return Math.Sqrt( AdiabaticIndex * _specificGasConstant * temperature );
            }
            else
            {
                // Re-calculate density here locally or pass it in? 
                // Using formula c = sqrt(K / rho)
                double rho = GetDensity( temperature, pressure );
                if( rho <= 1e-4 ) return 0;
                return Math.Sqrt( _bulkModulus / rho );
            }
        }

        public double GetVaporPressure( double temperature )
        {
            // Antoine: log10(P) = A - B / (T + C)
            if( AntoineCoeffs == null || AntoineCoeffs.Length < 3 ) return 0.0;

            double denominator = temperature + AntoineCoeffs[2];
            // Safety: Avoid divide by zero singularity
            if( Math.Abs( denominator ) < 1e-5 ) return 0.0;

            double exponent = AntoineCoeffs[0] - (AntoineCoeffs[1] / denominator);
            return Math.Pow( 10, exponent );
        }

        public double GetBoilingPoint( double pressure )
        {
            if( pressure <= 1e-6 ) return 0;
            if( AntoineCoeffs == null || AntoineCoeffs.Length < 3 ) return 373.15;

            double logP = Math.Log10( pressure );
            double denominator = AntoineCoeffs[0] - logP;

            if( Math.Abs( denominator ) < 1e-5 ) return double.MaxValue;

            return (AntoineCoeffs[1] / denominator) - AntoineCoeffs[2];
        }

        public double GetLatentHeatOfVaporization() => LatentHeatVap;
        public double GetLatentHeatOfFusion() => LatentHeatFusion;

        /// <summary>
        /// Evaluates polynomial using Horner's Method.
        /// O(N) operations, minimal multiplications.
        /// y = c0 + x(c1 + x(c2 + ...))
        /// </summary>
        private static double EvaluatePolynomialHorner( double x, double[] coeffs )
        {
            if( coeffs == null || coeffs.Length == 0 ) return 0.0;

            // Loop backwards
            double result = coeffs[coeffs.Length - 1];
            for( int i = coeffs.Length - 2; i >= 0; i-- )
            {
                result = result * x + coeffs[i];
            }
            return result;
        }

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
                // Note: The setters will trigger, automatically updating the cached reciprocals
                .WithMember( "specific_gas_constant", o => o.SpecificGasConstant )
                .WithMember( "adiabatic_index", o => o.AdiabaticIndex )
                .WithMember( "flash_point", o => o.FlashPoint )
                .WithMember( "bulk_modulus", o => o.BulkModulus )
                .WithMember( "reference_density", o => o.ReferenceDensity )
                .WithMember( "reference_pressure", o => o.ReferencePressure )
                .WithMember( "viscosity_coeffs", o => o.ViscosityCoeffs )
                .WithMember( "conductivity_coeffs", o => o.ConductivityCoeffs )
                .WithMember( "specific_heat_coeffs", o => o.SpecificHeatCoeffs )
                .WithMember( "antoine_coeffs", o => o.AntoineCoeffs )
                .WithMember( "latent_heat_vap", o => o.LatentHeatVap )
                .WithMember( "latent_heat_fus", o => o.LatentHeatFusion );
        }
    }
}