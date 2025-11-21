using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.ResourceFlow
{
    public static class VaporLiquidEquilibrium
    {
        [Obsolete( "untested" )]
        public static (ISubstanceStateCollection, FluidState) ComputeFlash( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double deltaTime )
        {
            const double MinVolume = 1e-6;
            const double RelaxationFactor = 0.3;
            const double R = 8.314462618;

            double initialTemperature = currentState.Temperature;

            // 1. Pre-pass: Aggregate system properties (Moles, Volume, Heat Capacity)
            // We need to map Substances to their current Moles for quick lookup during the flash calculation
            var moleMap = new Dictionary<ISubstance, double>();

            double totalLiquidMoles = 0.0;
            double totalGasMoles = 0.0;
            double liquidVolume = 0.0;
            double totalHeatCapacity = 0.0;

            // Sanity check temperature
            if( initialTemperature <= 0.1 || initialTemperature > 100_000 )
                initialTemperature = 293.15;

            // Iterate the generic collection (ISubstance, Mass)
            foreach( (ISubstance substance, double mass) in currentContents )
            {
                if( mass <= 1e-9 ) continue;

                double moles = mass / substance.MolarMass;
                moleMap[substance] = moles; // Store for later lookup

                // Determine phase behavior based on substance properties
                // Assumption: ISubstance has a 'Phase' property. 
                // If not, you might need to check: if (substance == SubstancePhaseMap.GetPartnerPhase(substance, SubstancePhase.Liquid))
                if( substance.Phase == SubstancePhase.Liquid )
                {
                    totalLiquidMoles += moles;
                    double density = substance.GetDensity( initialTemperature, currentState.Pressure );
                    liquidVolume += mass / density;

                    double cp = substance.GetSpecificHeatCapacity( initialTemperature, currentState.Pressure );
                    totalHeatCapacity += mass * cp;
                }
                else if( substance.Phase == SubstancePhase.Gas )
                {
                    totalGasMoles += moles;

                    double cp = substance.GetSpecificHeatCapacity( initialTemperature, currentState.Pressure );
                    double cv = cp - substance.SpecificGasConstant; // Ideal gas relation
                    totalHeatCapacity += mass * cv;
                }
            }

            // Prevent division by zero in temp calculation
            if( totalHeatCapacity < 1e-5 ) totalHeatCapacity = 1.0;

            // Geometric constraints
            double gasVolume = tankVolume - liquidVolume;
            bool isFullOfLiquid = gasVolume <= MinVolume;
            double effectiveGasVolume = Math.Max( gasVolume, MinVolume );

            double totalLatentHeatAbsorbed = 0.0;

            // Store planned transfers: (SubstanceToChange, MassDelta)
            var massTransfers = new List<(ISubstance, double)>();

            // 2. Flash Calculation Pass
            // We drive the equilibrium from the Liquid side. 
            // (Iterating over liquids ensures we have valid Vapor Pressure data)
            foreach( (ISubstance liquidSubstance, double liquidMoles) in moleMap )
            {
                // Only process liquids here
                if( liquidSubstance.Phase != SubstancePhase.Liquid ) continue;

                // API Call: Find the corresponding Gas substance
                ISubstance gasVariant = SubstancePhaseMap.GetPartnerPhase( liquidSubstance, SubstancePhase.Gas );

                // Retrieve current gas moles (if any exists in the container)
                double gasMoles = 0.0;
                if( gasVariant != null && moleMap.TryGetValue( gasVariant, out double existingGasMoles ) )
                {
                    gasMoles = existingGasMoles;
                }

                // --- Equilibrium Logic ---

                double vaporPressure = liquidSubstance.GetVaporPressure( initialTemperature );

                // Raoult's Law: Partial Pressure of component = Mole Fraction * Vapor Pressure
                double liquidMoleFraction = totalLiquidMoles > 1e-9 ? liquidMoles / totalLiquidMoles : 0.0;
                double equilibriumPartialPressure = liquidMoleFraction * vaporPressure;

                // Dalton's Law: Current Partial Pressure = (n_gas * R * T) / V_gas
                double currentPartialPressure = (gasMoles * R * initialTemperature) / effectiveGasVolume;

                double pressureDifference = equilibriumPartialPressure - currentPartialPressure;

                // Calculate transfer in Moles
                double unrestrictedTransferMoles = (pressureDifference * effectiveGasVolume) / (R * initialTemperature) * RelaxationFactor;
                double actualTransferMoles = unrestrictedTransferMoles;

                // Hydraulic Lock: If full of liquid, we cannot evaporate (volume constrained), but we can condense.
                if( isFullOfLiquid && actualTransferMoles > 0 )
                    actualTransferMoles = 0.0;

                // Mass Balance Limits
                if( actualTransferMoles > 0 ) // Evaporation (Liquid -> Gas)
                {
                    actualTransferMoles = Math.Min( actualTransferMoles, liquidMoles );
                }
                else // Condensation (Gas -> Liquid)
                {
                    actualTransferMoles = Math.Max( actualTransferMoles, -gasMoles );
                }

                // 3. Queue Updates
                if( Math.Abs( actualTransferMoles ) > 1e-12 && gasVariant != null )
                {
                    double molarMass = liquidSubstance.MolarMass;
                    double transferredMass = actualTransferMoles * molarMass;

                    // Liquid loses mass, Gas gains mass (sign of transferredMass respects this direction)
                    massTransfers.Add( (liquidSubstance, -transferredMass) );
                    massTransfers.Add( (gasVariant, transferredMass) );

                    // Latent Heat (Energy Balance)
                    // Evaporation (positive transfer) absorbs heat -> system cools
                    double latentHeatPerKg = liquidSubstance.GetLatentHeatOfVaporization( initialTemperature );
                    totalLatentHeatAbsorbed += transferredMass * latentHeatPerKg;
                }
            }

            // 4. Apply Updates
            var updatedContents = currentContents.Clone();

            foreach( var (substance, massDelta) in massTransfers )
            {
                // Assuming ISubstanceStateCollection has an Add method that handles mass accumulation
                // Add(substance, -5) subtracts 5 mass.
                updatedContents.Add( substance, massDelta, deltaTime );
            }

            // 5. Final State Calculation
            double temperatureChange = -totalLatentHeatAbsorbed / totalHeatCapacity;
            double finalTemperature = initialTemperature + temperatureChange;

            double finalPressure;
            if( isFullOfLiquid )
            {
                // Bulk Modulus approximation for hydraulic pressure
                double fillRatio = liquidVolume / tankVolume;
                double compressionStrain = Math.Max( 0.0, fillRatio - 1.0 );
                const double bulkModulus = 2.2e9;
                finalPressure = 101325 + compressionStrain * bulkModulus;
            }
            else
            {
                // Recalculate pressure with new gas totals and new temperature
                // Note: We need the *new* total gas moles.
                double finalTotalGasMoles = totalGasMoles + massTransfers
                    .Where( t => t.Item1.Phase == SubstancePhase.Gas )
                    .Sum( t => t.Item2 / t.Item1.MolarMass ); // Convert mass delta back to moles

                finalPressure = (finalTotalGasMoles * R * finalTemperature) / effectiveGasVolume;
            }

            var newState = new FluidState( finalPressure, finalTemperature, currentState.Velocity );
            return (updatedContents, newState);
        }



        //

        struct LiquidEntry
        {
            public ISubstance Liq;
            public ISubstance Gas;
            public double Moles;
            public double MolarMass;
            public double Lv; // Latent heat per kg
        }

        [Obsolete( "untested" )]
        public static (ISubstanceStateCollection, FluidState) ComputeFlash2( IReadonlySubstanceStateCollection currentContents, FluidState currentState, double tankVolume, double deltaTime )
        {
            const double MinVolume = 1e-6;
            const double RelaxationFactor = 0.3;
            const double R = 8.314462618;
            const double MinMass = 1e-9;
            const double MinMoles = 1e-12;

            // Clone immediately — only one allocation in entire function
            var updatedContents = currentContents.Clone();

            double T = currentState.Temperature;
            if( T <= 0.1 || T > 100_000 ) T = 293.15;

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
            foreach( (var s, double mass) in currentContents )
            {
                if( mass <= MinMass ) continue;
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
                            Lv = s.GetLatentHeatOfVaporization( T )
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

            if( totalHeatCap < 1e-5 ) totalHeatCap = 1.0;

            double gasVol = tankVolume - liquidVolume;
            bool fullOfLiquid = gasVol <= MinVolume;
            double Vg = Math.Max( gasVol, MinVolume );

            // Flash pass
            if( totalLiquidMoles > 1e-9 && liquidCount > 0 )
            {
                double invLiq = 1.0 / totalLiquidMoles;

                for( int i = 0; i < liquidCount; i++ )
                {
                    ref LiquidEntry e = ref liquids[i];

                    double vp = e.Liq.GetVaporPressure( T );
                    double x = e.Moles * invLiq;
                    double pEq = x * vp;

                    double gasMolesThis = 0.0;
                    if( currentContents.TryGetMass( e.Gas, out double gm ) ) // gets [kg] of gas in the tank
                        gasMolesThis = gm / e.Gas.MolarMass;

                    double pNow = (gasMolesThis * R * T) / Vg;
                    double dp = pEq - pNow;

                    double dMoles = (dp * Vg) / (R * T) * RelaxationFactor;

                    if( fullOfLiquid && dMoles > 0 ) dMoles = 0;
                    if( dMoles > 0 ) dMoles = Math.Min( dMoles, e.Moles );
                    else dMoles = Math.Max( dMoles, -gasMolesThis );

                    if( Math.Abs( dMoles ) > MinMoles )
                    {
                        double dMass = dMoles * e.MolarMass;
                        netGasMoleChange += dMoles;
                        latentHeatAbsorbed += dMass * e.Lv;

                        updatedContents.Add( e.Liq, -dMass, deltaTime );
                        updatedContents.Add( e.Gas, dMass, deltaTime );
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