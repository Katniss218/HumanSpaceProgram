using System;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A lightweight, stack-allocated struct that caches the aggregate physical properties 
    /// of the mixture to allow for O(1) pressure calculations inside solver loops.
    /// </summary>
    internal struct MixtureProperties
    {
        public readonly double TotalMass;
        public double WeightedInvDensity;
        public double WeightedInvMolarMass;
        public readonly double AverageBulkModulus;
        public readonly double Temperature;
        public readonly double TankVolume;
        public readonly double ReferencePressure;

        private readonly double _RT;

        public MixtureProperties( IReadonlySubstanceStateCollection contents, FluidState state, double volume )
        {
            TotalMass = contents.GetMass();
            TankVolume = volume;
            Temperature = state.Temperature;
            ReferencePressure = state.Pressure;

            // Sanity check temperature
            if( Temperature <= 0.1 || Temperature > 100_000 )
                Temperature = 293.15;

            _RT = 8.314462618 * Temperature;

            WeightedInvDensity = 0;
            WeightedInvMolarMass = 0;
            AverageBulkModulus = 2.2e9; // Default fallback (Water-like)

            if( TotalMass <= 1e-12 )
                return;

            double sumBulkModWeighted = 0;
            double liquidMassSum = 0;

            // Iterate contents to gather coefficients
            // We use 'ref' to avoid copying this large struct during the loop
            if( contents is SubstanceStateCollection ssc )
            {
                for( int i = 0; i < ssc.Count; i++ )
                    ProcessItem( ssc[i], ref this, ref sumBulkModWeighted, ref liquidMassSum );
            }
            else
            {
                foreach( var item in contents )
                    ProcessItem( item, ref this, ref sumBulkModWeighted, ref liquidMassSum );
            }

            // Calculate the mass-weighted average Bulk Modulus for the liquid portion
            if( liquidMassSum > 1e-9 )
            {
                // Average K = Sum(Mass_i * K_i) / TotalLiquidMass
                AverageBulkModulus = sumBulkModWeighted / liquidMassSum;
            }
        }

        private static void ProcessItem( (ISubstance s, double m) item, ref MixtureProperties props, ref double sumBulkModWeighted, ref double liquidMassSum )
        {
            if( item.m <= 1e-12 )
                return;

            double w = item.m / props.TotalMass; // Mass fraction relative to total mixture

            if( item.s.Phase == SubstancePhase.Liquid )
            {
                // We use the ReferencePressure (Start of frame pressure) to look up density/modulus.
                // This linearizes the properties for the duration of the Newton-Raphson step.
                double rho = item.s.GetDensity( props.Temperature, props.ReferencePressure );
                double K = item.s.GetBulkModulus( props.Temperature, props.ReferencePressure );

                if( rho > 1e-9 )
                {
                    props.WeightedInvDensity += w / rho;

                    // Accumulate weighted bulk modulus
                    sumBulkModWeighted += item.m * K;
                    liquidMassSum += item.m;
                }
            }
            else if( item.s.Phase == SubstancePhase.Gas )
            {
                if( item.s.MolarMass > 1e-9 )
                    props.WeightedInvMolarMass += w / item.s.MolarMass;
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
            double liquidVol = currentMass * WeightedInvDensity;

            double gasVol = TankVolume - liquidVol;
            double effectiveGasVol = Math.Max( gasVol, 1e-6 );

            // 2. Check Overfill (Liquid Regime)
            // If gas volume is effectively zero, we are compressing liquid.
            if( gasVol <= 1e-6 )
            {
                double fillRatio = liquidVol / TankVolume;
                double strain = Math.Max( 0.0, fillRatio - 1.0 );

                // Pressure = Strain * BulkModulus
                double P = strain * AverageBulkModulus;

                // dP/dM = d(strain)/dM * K 
                // strain = (M * WeightedInvDensity / V) - 1
                // d(strain)/dM = WeightedInvDensity / V
                double dPdM = (WeightedInvDensity / TankVolume) * AverageBulkModulus;

                return (P, dPdM);
            }

            // 3. Gas/Mixed Regime
            // Moles_gas = Mass * Sum(w_i / M_i)
            double gasMoles = currentMass * WeightedInvMolarMass;

            if( gasMoles <= 1e-12 )
            {
                // Pure liquid underfilled. The ullage is vacuum, so ullage pressure is zero.
                // The stiffness in this regime comes from approaching the volume limit.
                // A simple model: stiffness is low when far from full, and ramps up to the liquid's
                // compressive stiffness as ullage disappears.
                double fillRatio = liquidVol / TankVolume;
                if( TankVolume <= 1e-6 ) fillRatio = 1.0;

                // Use a steep power function to make stiffness significant only when almost full.
                // A power of 20 makes stiffness at 99% fill about 80% of max, but at 90% it's only 12%.
                double stiffnessFactor = Math.Pow( Math.Max( 0, fillRatio ), 20 );

                // dP/dM_max = (K_avg / V_tank) * WeightedInvDensity
                double max_dPdM = (AverageBulkModulus / TankVolume) * WeightedInvDensity;

                return (1e-6, max_dPdM * stiffnessFactor);
            }


            // P = (nRT) / (V - V_liq)
            double numerator = gasMoles * _RT;
            double P_gas = numerator / effectiveGasVol;

            // Analytical Derivative Quotient Rule: P = u / v
            // u = M * C1  (where C1 = WeightedInvMolarMass * RT)
            // v = V_t - M * C2 (where C2 = WeightedInvDensity)

            double C1 = WeightedInvMolarMass * _RT;

            // Result simplifies to: dP/dM = (C1 * V_tank) / (V_gas)^2
            double dPdM_gas = (C1 * TankVolume) / (effectiveGasVol * effectiveGasVol);

            return (P_gas, dPdM_gas);
        }
    }


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
            void processSubstance( (ISubstance, double) item )
            {
                (ISubstance substance, double mass) = item;

                if( mass <= 0.0 )
                    return;

                // convert mass -> moles
                double moles = mass / substance.MolarMass;

                if( substance.Phase == SubstancePhase.Liquid )
                {
                    // density( T, P ) used to compute occupied volume by this liquid
                    double density = substance.GetDensity( temperature, currentState.Pressure );
                    liquidVolume += mass / density;
                }
                else if( substance.Phase == SubstancePhase.Gas )
                {
                    totalGasMoles += moles;
                }
            }

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
                double pressure = compressionStrain * BulkModulus;
                if( pressure < 0 )
                    pressure = 0;
                return pressure;
            }
            else
            {
                if( totalGasMoles <= 0.0 )
                {
                    return 1e-6;
                }

                // Ideas gas law.
                double pressure = (totalGasMoles * R * temperature) / effectiveGasVolume;
                if( pressure < 0 )
                    pressure = 0;
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
            void processSubstance( (ISubstance, double) item )
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

                    var gasSubstance = SubstancePhaseMap.GetPartnerPhase( s, SubstancePhase.Gas );
                    if( gasSubstance != null && liquidCount < MaxLiquids )
                    {
                        liquids[liquidCount++] = new LiquidEntry()
                        {
                            Liq = s,
                            Gas = gasSubstance,
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
            }

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

                    if( fullOfLiquid && dMoles > 0 )
                        dMoles = 0;
                    if( dMoles > 0 )
                        dMoles = Math.Min( dMoles, e.Moles );
                    else
                        dMoles = Math.Max( dMoles, -gasMolesThis );

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


        // Context struct to avoid closure allocation
        struct Context
        {
            public double Temperature;
            public double Pressure;
            public double TotalMass;
            public double LiquidVolume;
            public double SumGasMoles;
            public double Sum_w_liquid_over_rho; // Sum(w_i / rho_i) for liquids
            public double Sum_w_gas_over_MM;     // Sum(w_i / MM_i) for gases
        }

        /// <summary>
        /// Computes the pressure of a substance collection in a fixed volume, assuming no phase change,
        /// and also calculates the analytical derivative of pressure with respect to the total mass of the collection.
        /// </summary>
        /// <param name="contents">The collection of substances and their masses.</param>
        /// <param name="currentState">The current fluid state (used for temperature).</param>
        /// <param name="tankVolume">The total volume of the container in [m^3].</param>
        /// <returns>A tuple containing the calculated pressure [Pa] and the derivative ∂P/∂m [Pa/kg].</returns>
        public static (double pressure, double dPdM) ComputePressureAndDerivativeWrtMass( IReadonlySubstanceStateCollection contents, FluidState currentState, double tankVolume )
        {
            var props = new MixtureProperties( contents, currentState, tankVolume );
            return props.GetStateAtMass( contents.GetMass() );
        }
    }
}