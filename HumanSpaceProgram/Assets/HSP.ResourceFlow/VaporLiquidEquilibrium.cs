using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Provides a high-performance, physically-grounded VLE/SLE (Vapor-Liquid & Solid-Liquid Equilibrium) solver
    /// based on the "Isothermal Inverse" model. This approach first calculates the total energy required to
    /// reach a perfect phase equilibrium at the current temperature, then scales the process based on the
    /// actual heat energy available.
    /// </summary>
    /// <remarks>
    /// This model is designed to be faster, more stable, and more physically accurate than iterative or sequential
    /// energy-budgeting models, as it handles all phase changes concurrently.
    /// </remarks>
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
            public LiquidEntry Liquid;
            public double MassToTransfer; // Positive for boiling, negative for condensation
        }


        /// <summary>
        /// The forward-solving method that uses the inverse isothermal model. It computes one step of melting,
        /// freezing, boiling, and condensation based on a given heat input.
        /// </summary>
        public static (ISubstanceStateCollection, FluidState) ComputeFlash( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double heatInput_Watts, double deltaTime )
        {
            (double requiredHeat_Joules, ISubstanceStateCollection equilibriumContents) = ComputeIsothermalHeatRequirement( currentContents, currentState, tankVolume, deltaTime );

            double externalEnergy_Joules = heatInput_Watts * deltaTime;

            double finalTemperature;
            ISubstanceStateCollection finalContents;

#warning TODO - Handle case where the temperature would drop below absolute 0 due to requiring too much energy than what is available.
            // Case 1: System is already in equilibrium. Any heat input just changes temperature.
            if( Math.Abs( requiredHeat_Joules ) < 1e-6 )
            {
                double heatCapacity = CalculateTotalHeatCapacity( currentContents, currentState.Temperature, currentState.Pressure );
                double delta_T = (heatCapacity > 1e-9) ? externalEnergy_Joules / heatCapacity : 0.0;
                finalTemperature = currentState.Temperature + delta_T;
                finalContents = currentContents.Clone();
            }
            // Case 2: External energy opposes the required phase change (e.g., cooling a system that wants to melt).
            // No phase change occurs, only sensible heat change.
            else if( Math.Sign( externalEnergy_Joules ) != 0 && Math.Sign( requiredHeat_Joules ) != 0 && Math.Sign( externalEnergy_Joules ) != Math.Sign( requiredHeat_Joules ) )
            {
                finalContents = currentContents.Clone();
                double heatCapacity = CalculateTotalHeatCapacity( currentContents, currentState.Temperature, currentState.Pressure );
                double delta_T = (heatCapacity > 1e-9) ? externalEnergy_Joules / heatCapacity : 0.0;
                finalTemperature = currentState.Temperature + delta_T;
            }
            // Case 3: Partial phase change, limited by external energy. Temperature remains constant.
            // This is for non-zero external energy that is insufficient to complete the change.
            else if( Math.Abs( externalEnergy_Joules ) > 1e-6 && Math.Abs( externalEnergy_Joules ) < Math.Abs( requiredHeat_Joules ) )
            {
                double scale = externalEnergy_Joules / requiredHeat_Joules;
                finalContents = IReadonlySubstanceStateCollection.Lerp( currentContents, equilibriumContents, scale );
                finalTemperature = currentState.Temperature;
            }
            // Case 4: Full phase change (driven by sufficient external energy OR self-driven with no external energy).
            // Temperature changes based on the net energy balance.
            else
            {
                finalContents = equilibriumContents;
                double sensibleHeatBudget = externalEnergy_Joules - requiredHeat_Joules;
                double finalHeatCapacity = CalculateTotalHeatCapacity( finalContents, currentState.Temperature, currentState.Pressure );
                double delta_T = (finalHeatCapacity > 1e-9) ? sensibleHeatBudget / finalHeatCapacity : 0.0;
                finalTemperature = currentState.Temperature + delta_T;
            }

            double finalPressure = finalContents.GetPressureInVolume( tankVolume, new FluidState( 0, finalTemperature, 0 ) );
            var finalState = new FluidState( finalPressure, finalTemperature, currentState.Velocity );
            return (finalContents, finalState);
        }

        /// <summary>
        /// Analytically calculates the total heat energy (in Joules) that would be absorbed or released
        /// to bring the system to perfect phase equilibrium at its current temperature. This is the "inverse" solver.
        /// </summary>
        public static (double requiredHeat_Joules, ISubstanceStateCollection finalContents) ComputeIsothermalHeatRequirement( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double deltaTime )
        {
            var finalContents = currentContents.Clone();

            // --- Phase 1: Isothermal Solid-Liquid Equilibrium (SLE) ---
            double heatForSLE = ResolveSLE_Isothermal( finalContents, currentState.Temperature, currentState.Pressure );

            // --- Phase 2: Isothermal Vapor-Liquid Equilibrium (VLE) ---
            double heatForVLE = ResolveVLE_Isothermal( finalContents, currentState, tankVolume );

            // --- Phase 3: Finalization ---
            double totalLatentHeat = heatForSLE + heatForVLE;
            return (totalLatentHeat, finalContents);
        }

        private static double ResolveSLE_Isothermal( ISubstanceStateCollection contents, double temperature, double pressure )
        {
            double heatForSLE = 0.0;
            var substancesToProcess = new List<ISubstance>();
            foreach( (var s, _) in contents )
            {
                substancesToProcess.Add( s );
            }

            foreach( ISubstance s in substancesToProcess )
            {
                double mass = contents[s];
                if( mass <= 1e-9 )
                    continue;

                if( s.Phase == SubstancePhase.Solid && temperature >= s.GetMeltingPoint( pressure ) )
                {
                    var liquidPartner = SubstancePhaseMap.GetPartnerPhase( s, SubstancePhase.Liquid );
                    if( liquidPartner != null )
                    {
                        heatForSLE += mass * s.GetLatentHeatOfFusion();
                        contents.Add( s, -mass );
                        contents.Add( liquidPartner, mass );
                    }
                }
                else if( s.Phase == SubstancePhase.Liquid )
                {
                    var solidPartner = SubstancePhaseMap.GetPartnerPhase( s, SubstancePhase.Solid );
                    if( solidPartner != null && temperature <= solidPartner.GetMeltingPoint( pressure ) )
                    {
                        heatForSLE -= mass * s.GetLatentHeatOfFusion(); // Freezing releases heat
                        contents.Add( s, -mass );
                        contents.Add( solidPartner, mass );
                    }
                }
            }

            return heatForSLE;
        }

        private static double ResolveVLE_Isothermal( ISubstanceStateCollection contents, FluidState currentState, double tankVolume )
        {
            const double MinMoles = 1e-8;
            const double PressureDeadzone = 2.0; // [Pa]

            double initialTemperature = currentState.Temperature;
            double initialTotalPressure = contents.GetPressureInVolume( tankVolume, currentState );

            var volatilePairs = new Dictionary<string, LiquidEntry>();

            // Populate pairs from both liquids and gases present to handle boiling and condensation.
            foreach( (var substance, _) in contents )
            {
                if( substance.MolarMass <= 0 ) continue;

                if( substance.Phase == SubstancePhase.Liquid )
                {
                    var gasPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Gas );
                    if( gasPartner != null && !volatilePairs.ContainsKey( substance.ID ) )
                    {
                        volatilePairs[substance.ID] = new LiquidEntry { Liq = substance, Gas = gasPartner, MolarMass = substance.MolarMass, Lv = substance.GetLatentHeatOfVaporization() };
                    }
                }
                else if( substance.Phase == SubstancePhase.Gas )
                {
                    var liquidPartner = SubstancePhaseMap.GetPartnerPhase( substance, SubstancePhase.Liquid );
                    if( liquidPartner != null && !volatilePairs.ContainsKey( liquidPartner.ID ) )
                    {
                        volatilePairs[liquidPartner.ID] = new LiquidEntry { Liq = liquidPartner, Gas = substance, MolarMass = liquidPartner.MolarMass, Lv = liquidPartner.GetLatentHeatOfVaporization() };
                    }
                }
            }

            if( volatilePairs.Count == 0 )
                return 0.0;

            var requests = new List<VLETransferRequest>();
            double initialTotalGasMoles = contents.GetTotalMolesOfPhases( SubstancePhase.Gas );
            double initialTotalLiquidMoles = contents.GetTotalMolesOfPhases( SubstancePhase.Liquid );

            // Pre-calculate ullage volume before the loop for optimization and correctness.
            double liquidVolume = contents.GetVolumeOfPhases( initialTemperature, initialTotalPressure, SubstancePhase.Liquid, SubstancePhase.Solid );
            double ullageVolume = tankVolume - liquidVolume;

            // First pass: calculate all desired transfers without modifying contents to avoid order-dependency.
            foreach( var liquidEntry in volatilePairs.Values )
            {
                double liquidMoles = contents.TryGet( liquidEntry.Liq, out var liqMass ) ? liqMass / liquidEntry.MolarMass : 0.0;
                double moleFraction = (initialTotalLiquidMoles > MinMoles) ? liquidMoles / initialTotalLiquidMoles : 1.0;
                double equilibriumPressure = liquidEntry.Liq.GetVaporPressure( initialTemperature ) * moleFraction;

                double gasMolesThis = contents.TryGet( liquidEntry.Gas, out double gasMass ) ? gasMass / liquidEntry.MolarMass : 0.0;
                double currentPartialPressure = (initialTotalGasMoles > MinMoles) ? (gasMolesThis / initialTotalGasMoles) * initialTotalPressure : 0.0;

                double pressureDifference = equilibriumPressure - currentPartialPressure;
                if( Math.Abs( pressureDifference ) < PressureDeadzone )
                    continue;

                double pressureDerivativeWrtMass = ComputeVLEStiffness( liquidEntry.Liq, contents, currentState, tankVolume, ullageVolume );
                if( Math.Abs( pressureDerivativeWrtMass ) < 1e-9 )
                    continue;

                double massToTransfer = pressureDifference / pressureDerivativeWrtMass;
                requests.Add( new VLETransferRequest { Liquid = liquidEntry, MassToTransfer = massToTransfer } );
            }

            // Second pass: apply clamped transfers and calculate total heat.
            double heatForVLE = 0.0;
            foreach( var request in requests )
            {
                double massToTransfer = request.MassToTransfer;

                // Clamp mass transfer to the available mass of the source phase.
                if( massToTransfer > 0 ) // Boiling
                {
                    massToTransfer = Math.Min( massToTransfer, contents[request.Liquid.Liq] );
                }
                else // Condensation
                {
                    massToTransfer = Math.Max( massToTransfer, -contents[request.Liquid.Gas] );
                }

                if( Math.Abs( massToTransfer ) > 1e-9 )
                {
                    contents.Add( request.Liquid.Liq, -massToTransfer );
                    contents.Add( request.Liquid.Gas, massToTransfer );
                    heatForVLE += massToTransfer * request.Liquid.Lv;
                }
            }
            return heatForVLE;
        }

        private static double ComputeVLEStiffness( ISubstance liquid, IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double ullageVolume )
        {
            double temperature = currentState.Temperature;

            // If there is significant ullage, use the faster, more stable analytic derivative.
            if( ullageVolume > 1e-6 )
            {
                ISubstance gasPartner = SubstancePhaseMap.GetPartnerPhase( liquid, SubstancePhase.Gas );
                if( gasPartner == null ) return 0.0;

                // We need the current pressure for the formula. GetPressureInVolume is the source of truth.
                double currentPressure = currentContents.GetPressureInVolume( tankVolume, currentState );

                double R_s_i = gasPartner.SpecificGasConstant;
                double rho_li = liquid.GetDensity( temperature, currentPressure );

                if( rho_li > 1e-9 )
                {
                    // dP/dm = (1 / V_ullage) * ( R_s_i * T - P / rho_li )
                    return (1 / ullageVolume) * (R_s_i * temperature - currentPressure / rho_li);
                }
            }

            // --- Fallback to numerical derivative for liquid-full or edge cases ---
            const double DeltaMassFraction = 0.001;
            const double MinTestMass = 0.001;

            ISubstance gasPartner_num = SubstancePhaseMap.GetPartnerPhase( liquid, SubstancePhase.Gas );
            if( gasPartner_num == null )
                return 0.0;

            (double initialPressure, _) = ComputePressureAndDerivativeWrtMass( currentContents, currentState, tankVolume );

            double availableLiquidMass = currentContents[liquid];
            double availableGasMass = currentContents[gasPartner_num];

            if( availableLiquidMass >= MinTestMass )
            {
                double testMass = Math.Max( MinTestMass, availableLiquidMass * DeltaMassFraction );
                using var perturbedContents = PooledReadonlySubstanceStateCollection.Get();
                perturbedContents.Add( currentContents );
                perturbedContents.Add( liquid, -testMass );
                perturbedContents.Add( gasPartner_num, testMass );

                (double finalPressure, _) = ComputePressureAndDerivativeWrtMass( perturbedContents, currentState, tankVolume );
                return (finalPressure - initialPressure) / testMass;
            }
            else if( availableGasMass >= MinTestMass )
            {
                double testMass = Math.Max( MinTestMass, availableGasMass * DeltaMassFraction );
                using var perturbedContents = PooledReadonlySubstanceStateCollection.Get();
                perturbedContents.Add( currentContents );
                perturbedContents.Add( gasPartner_num, -testMass );
                perturbedContents.Add( liquid, testMass );

                (double finalPressure, _) = ComputePressureAndDerivativeWrtMass( perturbedContents, currentState, tankVolume );
                // dm = -testMass for boiling, so dP/dm = (finalP - initialP) / -testMass
                return (finalPressure - initialPressure) / -testMass;
            }

            return 0.0;
        }

        public static (double pressure, double dPdM) ComputePressureAndDerivativeWrtMass( IReadonlySubstanceStateCollection contents, FluidState currentState, double tankVolume )
        {
            var props = new MixtureProperties( contents, currentState, tankVolume );
            return props.GetStateAtMass( contents.GetMass() );
        }

        private static double CalculateTotalHeatCapacity( IReadonlySubstanceStateCollection contents, double temperature, double pressure )
        {
            double totalHeatCapacity = 0;
            foreach( (var substance, double mass) in contents )
            {
                if( mass > 1e-9 )
                {
                    totalHeatCapacity += mass * substance.GetSpecificHeatCapacity( temperature, pressure );
                }
            }
            return totalHeatCapacity;
        }
    }
}