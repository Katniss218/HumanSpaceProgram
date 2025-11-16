using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// State information about a single resource.
    /// </summary>
    [Serializable]
    public readonly struct SubstanceState
    {
        /// <summary>
        /// The physical/chemical data about the specific resource.
        /// </summary>
        [field: SerializeField]
        public Substance Substance { get; }

        /// <summary>
        /// Amount of substance, tracked using mass, in [kg].
        /// </summary>
        [field: SerializeField]
        public float MassAmount { get; }


        public SubstanceState( float massAmount, Substance resource )
        {
            this.MassAmount = massAmount;
            this.Substance = resource;
        }

        public SubstanceState( SubstanceState original, float massAmount )
        {
            this.Substance = original.Substance;
            this.MassAmount = massAmount;
        }


        public float GetVolumeAtPressure( float pressure, float Temperature )
        {
            if( pressure <= 0f )
                throw new ArgumentOutOfRangeException( nameof( pressure ), "Pressure must be positive and non-zero." );

            if( Substance.Phase == SubstancePhase.Gas )
            {
                if( Substance.SpecificGasConstant <= 0f || Temperature <= 0f )
                    return float.PositiveInfinity;
                float density = pressure / (Substance.SpecificGasConstant * Temperature);
                return MassAmount / density;
            }
            else // liquids and solids.
            {
                if( Substance.BulkModulus <= 0f )
                    return MassAmount / Substance.Density;
                float density = Substance.Density * (1f + (pressure - Substance.ReferencePressure) / Substance.BulkModulus);
                return MassAmount / density;
            }
        }

        public float GetDensityAtPressure( float pressure, float Temperature )
        {
            if( Substance.Phase == SubstancePhase.Gas )
            {
                if( Substance.SpecificGasConstant <= 0f || Temperature <= 0f )
                    return 0f;
                return pressure / (Substance.SpecificGasConstant * Temperature);
            }
            if( Substance.BulkModulus <= 0f ) 
                return Substance.Density;
            return Substance.Density * (1f + (pressure - Substance.ReferencePressure) / Substance.BulkModulus);
        }

        public float GetPressureAtVolume( float volume, float Temperature )
        {
            if( volume <= 0f )
                throw new ArgumentOutOfRangeException( nameof( volume ), "Volume must be positive and non-zero." );

            if( Substance.Phase == SubstancePhase.Gas )
            {
                float rho = MassAmount / volume;
                return rho * Substance.SpecificGasConstant * Temperature;
            }
            else // liquids and solids.
            {
                float rho = MassAmount / volume;
                return Substance.ReferencePressure + Substance.BulkModulus * (rho / Substance.Density - 1f);
            }
        }

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

        /// <summary>
        /// Compute the pressure of a mixture of substances given the total volume.
        /// Uses mass-weighted averages of per-substance properties as an engineering approximation.
        /// </summary>
        public static (SubstanceState[], FluidState) GetMixturePressure( SubstanceState[] contents, float volume, float Temperature )
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

        [MapsInheritingFrom( typeof( SubstanceState ) )]
        public static SerializationMapping SubstanceStateMapping()
        {
            return new MemberwiseSerializationMapping<SubstanceState>()
                .WithMember( "substance", ObjectContext.Asset, o => o.Substance )
                .WithMember( "mass_amount", o => o.MassAmount );
        }
    }
}