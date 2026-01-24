using System;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A lightweight, stack-allocated struct that caches the aggregate physical properties 
    /// of the mixture to allow for O(1) pressure calculations inside solver loops.
    /// </summary>
    public struct MixtureProperties
    {
        public readonly double TotalMass;
        public double WeightedInverseDensity;
        public double WeightedInverseMolarMass;
        public readonly double AverageBulkModulus;
        public readonly double Temperature;
        public readonly double TankVolume;
        public readonly double ReferencePressure;

        private readonly double _universalGasConstantTimesTemp;

        public MixtureProperties( IReadonlySubstanceStateCollection contents, FluidState state, double volume )
        {
            TotalMass = contents.GetMass();
            TankVolume = volume;
            Temperature = state.Temperature;
            ReferencePressure = state.Pressure;

            // Sanity check temperature
            if( Temperature <= 0.1 || Temperature > 100_000 )
                Temperature = 293.15;

            _universalGasConstantTimesTemp = 8.314462618 * Temperature;

            WeightedInverseDensity = 0;
            WeightedInverseMolarMass = 0;
            AverageBulkModulus = 2.2e9; // Default fallback (Water-like)

            if( TotalMass <= 1e-12 )
                return;

            double weightedBulkModulusSum = 0;
            double totalLiquidMass = 0;

            // Iterate contents to gather coefficients
            // We use 'ref' to avoid copying this large struct during the loop
            if( contents is SubstanceStateCollection ssc )
            {
                for( int i = 0; i < ssc.Count; i++ )
                    ProcessItem( ssc[i], ref this, ref weightedBulkModulusSum, ref totalLiquidMass );
            }
            else
            {
                foreach( var item in contents )
                    ProcessItem( item, ref this, ref weightedBulkModulusSum, ref totalLiquidMass );
            }

            // Calculate the mass-weighted average Bulk Modulus for the liquid portion
            if( totalLiquidMass > 1e-9 )
            {
                // Average K = Sum(Mass_i * K_i) / TotalLiquidMass
                AverageBulkModulus = weightedBulkModulusSum / totalLiquidMass;
            }
        }

        private static void ProcessItem( (ISubstance s, double m) item, ref MixtureProperties properties, ref double weightedBulkModulusSum, ref double totalLiquidMass )
        {
            if( item.m <= 1e-12 )
                return;

            double massFraction = item.m / properties.TotalMass; // Mass fraction relative to total mixture

            if( item.s.Phase == SubstancePhase.Liquid )
            {
                // We use the ReferencePressure (Start of frame pressure) to look up density/modulus.
                // This linearizes the properties for the duration of the Newton-Raphson step.
                double density = item.s.GetDensity( properties.Temperature, properties.ReferencePressure );
                double bulkModulus = item.s.GetBulkModulus( properties.Temperature, properties.ReferencePressure );

                if( density > 1e-9 )
                {
                    properties.WeightedInverseDensity += massFraction / density;

                    // Accumulate weighted bulk modulus
                    weightedBulkModulusSum += item.m * bulkModulus;
                    totalLiquidMass += item.m;
                }
            }
            else if( item.s.Phase == SubstancePhase.Gas )
            {
                if( item.s.MolarMass > 1e-9 )
                    properties.WeightedInverseMolarMass += massFraction / item.s.MolarMass;
            }
        }

        /// <summary>
        /// Calculates P and dP/dM for a specific scaled mass WITHOUT allocating memory.
        /// </summary>
        public (double P, double dPdM) GetStateAtMass( double currentMass )
        {
            if( currentMass <= 1e-9 )
                return (0, 0);

            // 1. Calculate Liquid Volume based on current mass
            // V_liq = Mass * Sum(w_i / rho_i)
            double liquidVolume = currentMass * WeightedInverseDensity;

            double ullageVolume = TankVolume - liquidVolume;
            double effectiveUllageVolume = Math.Max( ullageVolume, 1e-6 );

            // 2. Check Overfill (Liquid Regime)
            // If gas volume is effectively zero, we are compressing liquid.
            if( ullageVolume <= 1e-6 )
            {
                double fillRatio = liquidVolume / TankVolume;
                double strain = Math.Max( 0.0, fillRatio - 1.0 );

                // Pressure = Strain * BulkModulus
                double pressure = strain * AverageBulkModulus;

                // dP/dM = d(strain)/dM * K 
                // strain = (M * WeightedInvDensity / V) - 1
                // d(strain)/dM = WeightedInvDensity / V
                double pressureDerivative = (WeightedInverseDensity / TankVolume) * AverageBulkModulus;

                return (pressure, pressureDerivative);
            }

            // 3. Gas/Mixed Regime
            // Moles_gas = Mass * Sum(w_i / M_i)
            double totalGasMoles = currentMass * WeightedInverseMolarMass;

            if( totalGasMoles <= 1e-12 )
            {
                // Pure liquid underfilled. The ullage is vacuum, so ullage pressure is zero.
                // The stiffness in this regime comes from approaching the volume limit.
                // A simple model: stiffness is low when far from full, and ramps up to the liquid's
                // compressive stiffness as ullage disappears.
                double fillRatio = liquidVolume / TankVolume;
                if( TankVolume <= 1e-6 ) fillRatio = 1.0;

                // Use a steep power function to make stiffness significant only when almost full.
                // A power of 20 makes stiffness at 99% fill about 80% of max, but at 90% it's only 12%.
                double stiffnessFactor = Math.Pow( Math.Max( 0, fillRatio ), 20 );

                // dP/dM_max = (K_avg / V_tank) * WeightedInvDensity
                double max_dPdM = (AverageBulkModulus / TankVolume) * WeightedInverseDensity;

                return (1e-6, max_dPdM * stiffnessFactor);
            }


            // P = (nRT) / (V - V_liq)
            double pressureNumerator = totalGasMoles * _universalGasConstantTimesTemp;
            double gasPressure = pressureNumerator / effectiveUllageVolume;

            // Analytical Derivative Quotient Rule: P = u / v
            // u = M * C1  (where C1 = WeightedInvMolarMass * RT)
            // v = V_t - M * C2 (where C2 = WeightedInvDensity)

            double gasConstantFactor = WeightedInverseMolarMass * _universalGasConstantTimesTemp;

            // Result simplifies to: dP/dM = (C1 * V_tank) / (V_gas)^2
            double gasPressureDerivative = (gasConstantFactor * TankVolume) / (effectiveUllageVolume * effectiveUllageVolume);

            return (gasPressure, gasPressureDerivative);
        }
    }
}