using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.ResourceFlow
{
    public static class VaporLiquidEquilibrium
    {
        public static double ComputePressureOnly( IReadonlySubstanceStateCollection contents, FluidState currentState, double tankVolume )
        {
            const double MinVolume = 1e-6;
            const double R = 8.314462618;           // universal gas constant [J/(mol·K)]
            const double BulkModulus = 2.2e9;       // bulk modulus used in original method [Pa]

            // Sanity-check temperature as in ComputeFlash
            double temperature = currentState.Temperature;
            if( temperature <= 0.1 || temperature > 100_000 )
                temperature = 293.15;

            double liquidVolume = 0.0;
            double totalGasMoles = 0.0;

            // Sum up gas moles and liquid occupied volume
            Action<(ISubstance, double)> processSubstance = ( item ) =>
            {
                (ISubstance substance, double mass) = item;

                if( mass <= 0.0 )
                    return;

                // convert mass -> moles
                double moles = mass / substance.MolarMass;

                if( substance.Phase == SubstancePhase.Liquid )
                {
                    // density( T, P ) used to compute occupied volume by this liquid
                    // note: we pass currentState.Pressure because that's what ComputeFlash used
                    double density = substance.GetDensity( temperature, currentState.Pressure );
                    // guard against degenerate density
                    if( density <= 0.0 ) density = 1.0;

                    liquidVolume += mass / density;
                }
                else if( substance.Phase == SubstancePhase.Gas )
                {
                    totalGasMoles += moles;
                }
            };

            // OPTIMIZATION: Use for loop to avoid enumerator allocation on SubstanceStateCollection
            if( contents is SubstanceStateCollection ssc )
            {
                for( int i = 0; i < ssc.Count; i++ )
                {
                    processSubstance( ssc[i] );
                }
            }
            else
            {
                foreach( (ISubstance substance, double mass) in contents )
                {
                    processSubstance( (substance, mass) );
                }
            }


            // Compute available gas volume
            double gasVolume = tankVolume - liquidVolume;
            bool isFullOfLiquid = gasVolume <= MinVolume;
            double effectiveGasVolume = Math.Max( gasVolume, MinVolume );

            if( isFullOfLiquid )
            {
                // Bulk-modulus approximation identical to ComputeFlash
                double fillRatio = liquidVolume / tankVolume;
                double compressionStrain = Math.Max( 0.0, fillRatio - 1.0 );
                return compressionStrain * BulkModulus;
            }
            else
            {
                if( totalGasMoles <= 0.0 )
                {
                    return 1e-6;
                }

                // Ideas gas law.
                double pressure = (totalGasMoles * R * temperature) / effectiveGasVolume;
                return pressure;
            }
        }

        struct LiquidEntry
        {
            public ISubstance Liq;
            public ISubstance Gas;
            public double Moles;
            public double MolarMass;
            public double Lv; // Latent heat per kg
        }

        public static (ISubstanceStateCollection, FluidState) ComputeFlash2( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double deltaTime )
        {
            const double MinVolume = 1e-6;
            const double RelaxationFactor = 0.21;
            const double R = 8.314462618;
            const double MinMass = 1e-6;
            const double MinMoles = 1e-8;
            const double PressureDeadZone = 1e-2; // Pa. Pressure differences smaller than this are ignored.

            // Clone immediately — this may be the only main allocation in the function.
            var updatedContents = (SubstanceStateCollection)currentContents.Clone();

            double T = currentState.Temperature;
            if( T <= 0.1 || T > 100_000 )
                T = 293.15;

            double totalLiquidMoles = 0;
            double totalGasMoles = 0;
            double liquidVolume = 0;
            double totalHeatCap = 0;
            double netGasMoleChange = 0;
            double latentHeatAbsorbed = 0;

            const int MaxLiquids = 16;
            var liquids = new LiquidEntry[MaxLiquids];
            int liquidCount = 0;


            // First pass
            Action<(ISubstance, double)> processSubstance = ( item ) =>
            {
                (var s, double mass) = item;

                if( mass <= MinMass )
                    return;
                double moles = mass / s.MolarMass;

                if( s.Phase == SubstancePhase.Liquid )
                {
                    totalLiquidMoles += moles;
                    liquidVolume += mass / s.GetDensity( T, currentState.Pressure );
                    totalHeatCap += mass * s.GetSpecificHeatCapacity( T, currentState.Pressure );

                    var gas = SubstancePhaseMap.GetPartnerPhase( s, SubstancePhase.Gas );
                    if( gas != null && liquidCount < MaxLiquids )
                    {
                        liquids[liquidCount++] = new LiquidEntry
                        {
                            Liq = s,
                            Gas = gas,
                            Moles = moles,
                            MolarMass = s.MolarMass,
                            Lv = s.GetLatentHeatOfVaporization()
                        };
                    }
                }
                else if( s.Phase == SubstancePhase.Gas )
                {
                    totalGasMoles += moles;
                    double cp = s.GetSpecificHeatCapacity( T, currentState.Pressure );
                    totalHeatCap += mass * (cp - s.SpecificGasConstant);
                }
            };

            // OPTIMIZATION: Use for loop to avoid enumerator allocation on SubstanceStateCollection
            if( currentContents is SubstanceStateCollection ssc )
            {
                for( int i = 0; i < ssc.Count; i++ )
                {
                    processSubstance( ssc[i] );
                }
            }
            else
            {
                foreach( (var s, double mass) in currentContents )
                {
                    processSubstance( (s, mass) );
                }
            }

            // OPTIMIZATION: Pre-allocate capacity to avoid re-allocations when adding new gas species.
            if( updatedContents != null )
            {
                updatedContents.EnsureCapacity( updatedContents.Count + liquidCount );
            }

            if( totalHeatCap < 1e-5 )
                totalHeatCap = 1.0;

            double gasVol = tankVolume - liquidVolume;
            bool fullOfLiquid = gasVol <= MinVolume;
            double Vg = Math.Max( gasVol, MinVolume );

            // Flash pass
            if( totalLiquidMoles > 1e-9 && liquidCount > 0 )
            {
                // Pre-calculate total gas pressure for correct partial pressure calculation.
                double pTotalGases = (totalGasMoles * R * T) / Vg;
                double invLiq = 1.0 / totalLiquidMoles;

                for( int i = 0; i < liquidCount; i++ )
                {
                    ref LiquidEntry e = ref liquids[i];

                    double vp = e.Liq.GetVaporPressure( T );
                    double x = e.Moles * invLiq;
                    double pEq = x * vp;

                    double gasMolesThis = 0.0;
                    if( currentContents.TryGet( e.Gas, out double gasMass ) )
                        gasMolesThis = gasMass / e.Gas.MolarMass;

                    // Correct partial pressure calculation based on mole fraction and total pressure.
                    // P_partial = (moles_gas / total_moles_gas) * P_total_gas
                    // Note: This is mathematically equivalent to (moles_gas * R * T) / Vg but makes the physics clearer.
                    double pNow = (totalGasMoles > MinMoles) ? (gasMolesThis / totalGasMoles) * pTotalGases : 0.0;
                    double dp = pEq - pNow;

                    // If the pressure difference is negligible, the system is in equilibrium. Do nothing.
                    if( Math.Abs( dp ) < PressureDeadZone )
                        continue;

                    double dMoles = (dp * Vg) / (R * T) * RelaxationFactor;

                    if( fullOfLiquid && dMoles > 0 ) dMoles = 0;
                    if( dMoles > 0 ) dMoles = Math.Min( dMoles, e.Moles );
                    else dMoles = Math.Max( dMoles, -gasMolesThis );

                    if( Math.Abs( dMoles ) > MinMoles )
                    {
                        double dMass = dMoles * e.MolarMass;
                        netGasMoleChange += dMoles;
                        latentHeatAbsorbed += dMass * e.Lv;

                        // The calculated dMass is the amount to transfer this step, it should not be scaled by time.
                        updatedContents.Add( e.Liq, -dMass );
                        updatedContents.Add( e.Gas, dMass );
                    }
                }
            }

            // Temperature update
            double dT = -latentHeatAbsorbed / totalHeatCap;
            double Tfinal = T + dT;

            // Pressure
            double Pfinal;
            if( fullOfLiquid )
            {
                double overfill = Math.Max( 0, liquidVolume / tankVolume - 1.0 );
                Pfinal = 101325 + overfill * 2.2e9;
            }
            else
            {
                double finalGasMoles = totalGasMoles + netGasMoleChange;
                Pfinal = (finalGasMoles * R * Tfinal) / Vg;
            }

            var newState = new FluidState( Pfinal, Tfinal, currentState.Velocity );
            return (updatedContents, newState);
        }
    }
}