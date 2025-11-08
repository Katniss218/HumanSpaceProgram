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

        /// <summary>
        /// Amount of substance, tracked using mass, in [kg].
        /// </summary>
        [field: SerializeField]
        public float Temperature { get; }


        public SubstanceState( float massAmount, Substance resource, float temperature )
        {
            this.MassAmount = massAmount;
            this.Substance = resource;
            this.Temperature = temperature;
        }

        public SubstanceState( SubstanceState original, float massAmount, float temperature )
        {
            this.Substance = original.Substance;
            this.MassAmount = massAmount;
            this.Temperature = temperature;
        }



        public float GetDensityAtPressure( float pressure )
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

        public float GetPressureAtVolume( float volume )
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

        [MapsInheritingFrom( typeof( SubstanceState ) )]
        public static SerializationMapping SubstanceStateMapping()
        {
            return new MemberwiseSerializationMapping<SubstanceState>()
                .WithMember( "substance", ObjectContext.Asset, o => o.Substance )
                .WithMember( "mass_amount", o => o.MassAmount )
                .WithMember( "temperature", o => o.Temperature );
        }
    }
}