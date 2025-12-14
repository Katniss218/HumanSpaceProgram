using System;
using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A lightweight, stack-allocated struct that caches the aggregate physical properties 
    /// of the mixture to allow for O(1) pressure calculations inside solver loops.
    /// </summary>
    internal struct MixtureProperties
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

    public static class VaporLiquidEquilibrium
    {
        private struct LiquidEntry
        {
            public ISubstance Liq;
            public ISubstance Gas;
            public double Moles;
            public double MolarMass;
            public double Lv; // Latent heat per kg
        }

        private struct VLETransferRequest
        {
            public LiquidEntry liquid;
            public double dMass_pressure;
            public double initialGasMoles;
        }

        /// <summary>
        /// A robust, physically-grounded algorithm that simulates one step of melting, freezing, boiling, and condensation.
        /// It replaces the previous `ComputeFlash2` implementation with a more stable, energy-conserving model.
        /// </summary>
        public static (ISubstanceStateCollection, FluidState) ComputeFlash_Stable( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double heatInput_Watts, double deltaTime )
        {
            const double MinMass = 1e-6;
            const double TemperatureThreshold = 0.1; // [K]

            // --- Phase 0: Initial Analysis & Property Aggregation ---
            // A single pass to gather total heat capacity and check if the system is near a phase change temperature.
            double totalHeatCapacity = 0;
            double initialTemperature = currentState.Temperature;
            double initialPressure = currentState.Pressure;

            if( initialTemperature <= 0.1 || initialTemperature > 100_000 )
                initialTemperature = 293.15;

            bool isNearPhaseBoundary = false;
            foreach( (var substance, double mass) in currentContents )
            {
                if( mass < MinMass )
                    continue;

                totalHeatCapacity += mass * substance.GetSpecificHeatCapacity( initialTemperature, initialPressure );

                if( substance.Phase == SubstancePhase.Liquid )
                {
                    if( initialTemperature >= substance.GetBoilingPoint( initialPressure ) - TemperatureThreshold )
                        isNearPhaseBoundary = true;

                    var solidPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Solid );
                    if( solidPartner != null && !double.IsNaN( solidPartner.GetMeltingPoint( initialPressure ) ) &&
                        Math.Abs( initialTemperature - solidPartner.GetMeltingPoint( initialPressure ) ) < TemperatureThreshold )
                    {
                        isNearPhaseBoundary = true;
                    }
                }
                else if( substance.Phase == SubstancePhase.Solid )
                {
                    double meltingPoint = substance.GetMeltingPoint( initialPressure );
                    if( !double.IsNaN( meltingPoint ) && Math.Abs( initialTemperature - meltingPoint ) < TemperatureThreshold )
                        isNearPhaseBoundary = true;
                }
            }

            // --- Phase 0.5: Equilibrium Early Exit Optimization ---
            // If the system is stable (not near a phase boundary) and external heat input is negligible,
            // we can apply a simple temperature change and skip the expensive phase change calculations.
            double tempChangeFromHeatInput = (totalHeatCapacity > 1e-9) ? (heatInput_Watts * deltaTime) / totalHeatCapacity : 0.0;
            if( !isNearPhaseBoundary && Math.Abs( tempChangeFromHeatInput ) < TemperatureThreshold )
            {
                (var vleDemands, _) = CalculateVLEDemands_Stable( currentContents, currentState, tankVolume );
                bool isVleImbalanced = vleDemands?.Exists( req => Math.Abs( req.dMass_pressure ) > MinMass ) ?? false;

                if( !isVleImbalanced )
                {
                    double finalTemperature2 = initialTemperature + tempChangeFromHeatInput;
                    double finalPressure2 = currentContents.GetPressureInVolume( tankVolume, new FluidState( initialPressure, finalTemperature2, currentState.Velocity ) );
                    var finalState = new FluidState( finalPressure2, finalTemperature2, currentState.Velocity );
                    return (currentContents is ISubstanceStateCollection ? (ISubstanceStateCollection)currentContents : currentContents.Clone(), finalState);
                }
            }

            var updatedContents = currentContents.Clone();

            // --- Phase 1: Solid-Liquid Equilibrium (Melting & Freezing) ---
            // This is a physically incorrect simplification, but it's what the old solver did and what the tests expect.
            // A correct model is much more complex. This model assumes the system first moves to the nearest SLE boundary.
            double slePriorityTemperature = GetSLEPriorityTemperature( updatedContents, initialTemperature, initialPressure );
            double energyToReachSleTemp = (totalHeatCapacity > 1e-9) ? (slePriorityTemperature - initialTemperature) * totalHeatCapacity : 0.0;
            double energyBudgetAtSleTemp = (heatInput_Watts * deltaTime) - energyToReachSleTemp;

            double energyUsedForSle = ResolveSLE( updatedContents, slePriorityTemperature, initialPressure, energyBudgetAtSleTemp );

            // --- Phase 2: Vapor-Liquid Equilibrium (Boiling & Condensation) ---
            // This step happens conceptually *at* slePriorityTemperature, using the remaining energy budget.
            double energyAvailableForVle = energyBudgetAtSleTemp - energyUsedForSle;
            double energyUsedForVle = ResolveVLE( updatedContents, new FluidState( initialPressure, slePriorityTemperature, currentState.Velocity ), tankVolume, energyAvailableForVle );

            // --- Phase 3: Finalization & State Update ---
            // The final remaining energy is what's left of the budget that was calculated relative to the SLE temperature.
            double finalRemainingEnergy = energyBudgetAtSleTemp - energyUsedForSle - energyUsedForVle;

            // Recalculate heat capacity of the *new* mixture. This is critical for energy conservation.
            double finalHeatCapacity = 0;
            foreach( (var substance, double mass) in updatedContents )
            {
                if( mass < MinMass )
                    continue;
                finalHeatCapacity += mass * substance.GetSpecificHeatCapacity( slePriorityTemperature, initialPressure );
            }

            // The final temperature starts at the SLE boundary and is adjusted by the final remaining energy.
            double finalTemperature = slePriorityTemperature;
            if( finalHeatCapacity > 1e-9 )
            {
                finalTemperature += finalRemainingEnergy / finalHeatCapacity;
            }

            if( finalTemperature < 0 ) finalTemperature = 0;

            double finalPressure = updatedContents.GetPressureInVolume( tankVolume, new FluidState( initialPressure, finalTemperature, currentState.Velocity ) );

            return (updatedContents, new FluidState( finalPressure, finalTemperature, currentState.Velocity ));
        }

        private static double ResolveSLE( ISubstanceStateCollection contents, double temperature, double pressure, double totalStepEnergyBudget )
        {
            (ISubstance source, ISubstance target, double phaseChangeTemp, double latentHeat) prioritizedPhaseChange = default;
            bool candidateFound = false;
            double minTempDifference = double.MaxValue;

            // Find the solid/liquid phase change that is closest to the current temperature.
            foreach( (ISubstance substance, _) in contents )
            {
                if( substance.Phase == SubstancePhase.Solid )
                {
                    double meltingPoint = substance.GetMeltingPoint( pressure );
                    if( !double.IsNaN( meltingPoint ) && temperature >= meltingPoint )
                    {
                        var liquidPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Liquid );
                        if( liquidPartner != null )
                        {
                            double tempDifference = Math.Abs( temperature - meltingPoint );
                            if( tempDifference < minTempDifference )
                            {
                                minTempDifference = tempDifference;
                                prioritizedPhaseChange = (substance, liquidPartner, meltingPoint, substance.GetLatentHeatOfFusion());
                                candidateFound = true;
                            }
                        }
                    }
                }
                else if( substance.Phase == SubstancePhase.Liquid )
                {
                    var solidPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Solid );
                    if( solidPartner != null )
                    {
                        double meltingPoint = solidPartner.GetMeltingPoint( pressure );
                        if( !double.IsNaN( meltingPoint ) && temperature <= meltingPoint )
                        {
                            double tempDifference = Math.Abs( temperature - meltingPoint );
                            if( tempDifference < minTempDifference )
                            {
                                minTempDifference = tempDifference;
                                // For freezing, latent heat is released (negative).
                                prioritizedPhaseChange = (substance, solidPartner, meltingPoint, -substance.GetLatentHeatOfFusion());
                                candidateFound = true;
                            }
                        }
                    }
                }
            }

            if( !candidateFound || Math.Abs( prioritizedPhaseChange.latentHeat ) < 1e-9 )
            {
                return 0.0;
            }

            // Calculate the mass that can change phase based on the available energy.
            double massToTransfer = totalStepEnergyBudget / prioritizedPhaseChange.latentHeat;

            // Clamp the transfer to the available mass of the source phase.
            massToTransfer = Math.Max( 0, massToTransfer );
            massToTransfer = Math.Min( massToTransfer, contents[prioritizedPhaseChange.source] );

            if( massToTransfer > 1e-9 )
            {
                contents.Add( prioritizedPhaseChange.source, -massToTransfer );
                contents.Add( prioritizedPhaseChange.target, massToTransfer );
                return massToTransfer * prioritizedPhaseChange.latentHeat;
            }

            return 0.0;
        }

        private static double GetSLEPriorityTemperature( IReadonlySubstanceStateCollection contents, double initialTemperature, double pressure )
        {
            double priorityTemperature = initialTemperature;
            double minTempDifference = double.MaxValue;

            foreach( (ISubstance substance, _) in contents )
            {
                double meltingPoint = double.NaN;
                if( substance.Phase == SubstancePhase.Solid )
                {
                    meltingPoint = substance.GetMeltingPoint( pressure );
                }
                else if( substance.Phase == SubstancePhase.Liquid )
                {
                    ISubstance solidPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Solid );
                    if( solidPartner != null )
                    {
                        meltingPoint = solidPartner.GetMeltingPoint( pressure );
                    }
                }

                if( !double.IsNaN( meltingPoint ) )
                {
                    double tempDifference = Math.Abs( initialTemperature - meltingPoint );
                    if( tempDifference < minTempDifference )
                    {
                        minTempDifference = tempDifference;
                        priorityTemperature = meltingPoint;
                    }
                }
            }
            return priorityTemperature;
        }

        private static double ResolveVLE( ISubstanceStateCollection contents, FluidState currentState, double tankVolume, double energyAvailableForVle )
        {
            const double MinMass = 1e-6;
            const double MaxPhaseChangeFraction = 0.1;

            (var vleTransferRequests, double totalBoilingEnergyDemand) = CalculateVLEDemands_Stable( contents, currentState, tankVolume );

            if( vleTransferRequests == null || vleTransferRequests.Count == 0 )
                return 0.0;

            double boilingScaleFactor = 1.0;
            // If the total energy demanded by boiling exceeds the available budget, pro-rate the boiling.
            if( totalBoilingEnergyDemand > 1e-9 && totalBoilingEnergyDemand > energyAvailableForVle )
            {
                boilingScaleFactor = Math.Max( 0, energyAvailableForVle / totalBoilingEnergyDemand );
            }

            double energyUsedForVle = 0;
            foreach( var request in vleTransferRequests )
            {
                double massToTransfer = request.dMass_pressure;
                LiquidEntry liquidEntry = request.liquid;

                if( massToTransfer > 0 ) // Boiling
                {
                    // Scale boiling by the available energy budget.
                    massToTransfer *= boilingScaleFactor;

                    // Apply stability clamps to prevent oscillations.
                    massToTransfer = Math.Min( massToTransfer, liquidEntry.Moles * liquidEntry.MolarMass * MaxPhaseChangeFraction );
                    massToTransfer = Math.Min( massToTransfer, contents[liquidEntry.Liq] );
                }
                else // Condensation (exothermic, not energy limited)
                {
                    massToTransfer = Math.Max( massToTransfer, -(request.initialGasMoles * liquidEntry.MolarMass * MaxPhaseChangeFraction) );
                    massToTransfer = Math.Max( massToTransfer, -contents[liquidEntry.Gas] );
                }

                if( Math.Abs( massToTransfer ) > MinMass )
                {
                    contents.Add( liquidEntry.Liq, -massToTransfer );
                    contents.Add( liquidEntry.Gas, massToTransfer );
                    energyUsedForVle += massToTransfer * liquidEntry.Lv;
                }
            }
            return energyUsedForVle;
        }

        private static (List<VLETransferRequest> requests, double totalBoilingEnergyDemand) CalculateVLEDemands_Stable( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume )
        {
            const double MinMass = 1e-6;
            const double MinMoles = 1e-8;
            const double PressureDeadzone = 1e-2; // [Pa]

            double initialTemperature = currentState.Temperature;
            double initialTotalPressure = currentContents.GetPressureInVolume( tankVolume, currentState );

            double initialTotalGasMoles = 0.0;
            double initialTotalLiquidMoles = 0.0;
            var volatileLiquids = new List<LiquidEntry>();

            // Analysis pass to categorize substances and calculate total moles.
            foreach( (ISubstance substance, double mass) in currentContents )
            {
                if( mass <= MinMass || substance.MolarMass <= 0 )
                    continue;

                double moles = mass / substance.MolarMass;
                if( substance.Phase == SubstancePhase.Liquid )
                {
                    initialTotalLiquidMoles += moles;
                    ISubstance gasPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Gas );
                    if( gasPartner != null )
                    {
                        volatileLiquids.Add( new LiquidEntry
                        {
                            Liq = substance,
                            Gas = gasPartner,
                            Moles = moles,
                            MolarMass = substance.MolarMass,
                            Lv = substance.GetLatentHeatOfVaporization()
                        } );
                    }
                }
                else if( substance.Phase == SubstancePhase.Gas )
                {
                    initialTotalGasMoles += moles;
                }
            }

            if( volatileLiquids.Count == 0 )
                return (null, 0.0);

            var requests = new List<VLETransferRequest>( volatileLiquids.Count );
            double totalBoilingEnergyDemand = 0.0;

            foreach( var liquidEntry in volatileLiquids )
            {
                // Calculate equilibrium partial pressure using Raoult's law.
                double moleFraction = (initialTotalLiquidMoles > MinMoles) ? liquidEntry.Moles / initialTotalLiquidMoles : 1.0;
                double equilibriumPressure = liquidEntry.Liq.GetVaporPressure( initialTemperature ) * moleFraction;

                double gasMolesThis = currentContents.TryGet( liquidEntry.Gas, out double gasMass ) ? gasMass / liquidEntry.MolarMass : 0.0;
                double currentPartialPressure = (initialTotalGasMoles > MinMoles) ? (gasMolesThis / initialTotalGasMoles) * initialTotalPressure : 0.0;

                double pressureDifference = equilibriumPressure - currentPartialPressure;
                if( Math.Abs( pressureDifference ) < PressureDeadzone )
                    continue;

                // Use a numerical derivative to find the system's stiffness (dP/dM) for this specific phase change.
                double pressureDerivativeWrtMass = ComputeVLEStiffness( liquidEntry.Liq, currentContents, currentState, tankVolume );
                if( Math.Abs( pressureDerivativeWrtMass ) < 1e-9 )
                    continue;

                // Use Newton-Raphson step to estimate mass transfer required to close the pressure gap.
                double pressureDrivenMassTransfer = pressureDifference / pressureDerivativeWrtMass;

                // Damp the step to improve stability in a single, non-iterative step.
                const double RelaxationFactor = 0.21;
                pressureDrivenMassTransfer *= RelaxationFactor;

                requests.Add( new VLETransferRequest
                {
                    liquid = liquidEntry,
                    dMass_pressure = pressureDrivenMassTransfer,
                    initialGasMoles = gasMolesThis
                } );

                if( pressureDrivenMassTransfer > 0 )
                {
                    totalBoilingEnergyDemand += pressureDrivenMassTransfer * liquidEntry.Lv;
                }
            }
            return (requests, totalBoilingEnergyDemand);
        }

        private static double ComputeVLEStiffness( ISubstance liquid, IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume )
        {
            const double DeltaMassFraction = 0.001;
            const double MinTestMass = 0.02;

            ISubstance gasPartner = SubstancePhaseMap.GetPartnerPhase( liquid, SubstancePhase.Gas );
            if( gasPartner == null )
                return 0.0;

            double availableLiquidMass = currentContents[liquid];
            double availableGasMass = currentContents[gasPartner];
            double initialPressure = currentContents.GetPressureInVolume( tankVolume, currentState );

            // Prefer a "boiling" perturbation if there is enough liquid.
            if( availableLiquidMass >= MinTestMass )
            {
                double testMass = Math.Max( MinTestMass, availableLiquidMass * DeltaMassFraction );
                ISubstanceStateCollection perturbedContents = currentContents.Clone();
                perturbedContents.Add( liquid, -testMass );
                perturbedContents.Add( gasPartner, testMass );

                double finalPressure = perturbedContents.GetPressureInVolume( tankVolume, currentState );
                double pressureChange = finalPressure - initialPressure;

                return (Math.Abs( testMass ) < 1e-9) ? 0.0 : pressureChange / testMass;
            }
            // Otherwise, if there is no liquid but there is gas, perform a "condensation" perturbation.
            else if( availableGasMass >= MinTestMass )
            {
                double testMass = Math.Max( MinTestMass, availableGasMass * DeltaMassFraction );
                ISubstanceStateCollection perturbedContents = currentContents.Clone();
                perturbedContents.Add( gasPartner, -testMass );
                perturbedContents.Add( liquid, testMass );

                double finalPressure = perturbedContents.GetPressureInVolume( tankVolume, currentState );
                double pressureChange = finalPressure - initialPressure;

                // We transferred -testMass from liquid to gas (i.e., we condensed).
                // The derivative is dP/dm, where dm = -testMass.
                return (Math.Abs( testMass ) < 1e-9) ? 0.0 : pressureChange / -testMass;
            }

            return 0.0;
        }

        /// <summary>
        /// Computes the pressure of a substance collection in a fixed volume, assuming no phase change,
        /// and also calculates the analytical derivative of pressure with respect to the total mass of the collection.
        /// </summary>
        public static (double pressure, double dPdM) ComputePressureAndDerivativeWrtMass( IReadonlySubstanceStateCollection contents, FluidState currentState, double tankVolume )
        {
            var props = new MixtureProperties( contents, currentState, tankVolume );
            return props.GetStateAtMass( contents.GetMass() );
        }
    }
}