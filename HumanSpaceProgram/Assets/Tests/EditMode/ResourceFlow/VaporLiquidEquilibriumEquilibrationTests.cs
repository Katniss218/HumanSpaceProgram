using HSP.ResourceFlow;
using NUnit.Framework;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class VaporLiquidEquilibriumEquilibrationTests
    {
        private Substance _lqWater;
        private Substance _gasWater;
        private Substance _ice;

        [SetUp]
        public void SetUp()
        {
            const double WATER_MOLAR_MASS = 0.01801528; // kg/mol

            _lqWater = new Substance( "lq_water" )
            {
                Phase = SubstancePhase.Liquid,
                MolarMass = WATER_MOLAR_MASS,
                ReferenceDensity = 997, // kg/m^3 at ~300K
                BulkModulus = 2.2e9,
                AntoineCoeffs = new double[] { 10.196, 1730.63, -39.724 }, // Pa at 300K -> ~3537
                LatentHeatVaporization = 2.26e6, // J/kg
                LatentHeatFusion = 3.34e5, // J/kg
                SpecificHeatCoeffs = new double[] { 4186 }, // J/(kg*K)
                MeltingPointSTP = 273.15
            };

            _gasWater = new Substance( "gas_water" )
            {
                Phase = SubstancePhase.Gas,
                MolarMass = WATER_MOLAR_MASS,
                SpecificHeatCoeffs = new double[] { 1941.5 }, // Cp for steam
            };

            _ice = new Substance( "ice" )
            {
                Phase = SubstancePhase.Solid,
                MolarMass = WATER_MOLAR_MASS,
                ReferenceDensity = 917,
                MeltingPointSTP = 273.15,
                LatentHeatFusion = 3.34e5,
                SpecificHeatCoeffs = new double[] { 2108 } // J/(kg*K) for ice
            };

            SubstancePhaseMap.Clear();
            SubstancePhaseMap.RegisterPhasePartner( _lqWater, SubstancePhase.Gas, _gasWater );
            SubstancePhaseMap.RegisterPhasePartner( _gasWater, SubstancePhase.Liquid, _lqWater );
            SubstancePhaseMap.RegisterPhasePartner( _lqWater, SubstancePhase.Solid, _ice );
            SubstancePhaseMap.RegisterPhasePartner( _ice, SubstancePhase.Liquid, _lqWater );
        }

        [TearDown]
        public void TearDown()
        {
            SubstancePhaseMap.Clear();
        }

        [Test, Description( "Verifies that a tank with only liquid and a vacuum ullage boils until it reaches a stable vapor pressure equilibrium." )]
        public void UnstableLiquid_Boils_AndReachesEquilibrium()
        {
            // Arrange
            double tankVolume = 10.0;
            double temperature = 300;
            double initialLiquidMass = 7;
            IReadonlySubstanceStateCollection contents = new SubstanceStateCollection { { _lqWater, initialLiquidMass } };
            var currentState = new FluidState( 0, temperature, 0 ); // Start with vacuum ullage

            // Act
            // Run simulation for enough steps to stabilize
            for( int i = 0; i < 100; i++ )
            {
                Debug.Log( currentState );
                (contents, currentState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 0.1 );
            }

            // Assert
            double finalLiquidMass = contents[_lqWater];
            double finalGasMass = contents[_gasWater];
            double finalTemp = currentState.Temperature;
            double finalPressure = currentState.Pressure;

            // 1. Phase change occurred correctly
            Assert.That( finalLiquidMass, Is.LessThan( initialLiquidMass ), "Liquid mass should decrease." );
            Assert.That( finalGasMass, Is.GreaterThan( 0 ), "Gas mass should increase." );
            Assert.That( finalTemp, Is.LessThan( temperature ), "Temperature should drop due to boiling." );

            // 2. Equilibrium was reached
            double expectedVaporPressure = _lqWater.GetVaporPressure( finalTemp );
            double finalPartialPressure = contents.GetPartialPressureOfVapor( _gasWater, finalPressure );
            Assert.That( finalPartialPressure, Is.EqualTo( expectedVaporPressure ).Within( 1.0 ).Percent, "Final partial pressure should match vapor pressure at final temperature." );

            // 3. Mass is conserved
            Assert.That( finalLiquidMass + finalGasMass, Is.EqualTo( initialLiquidMass ).Within( 1e-9 ), "Total mass of water must be conserved." );

            // 4. State is stable
            var (contentsAfter, stateAfter) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 0.1 );
            Assert.That( contentsAfter[_lqWater], Is.EqualTo( finalLiquidMass ).Within( 1e-6 ), "Liquid mass should be stable at equilibrium." );
            Assert.That( stateAfter.Temperature, Is.EqualTo( finalTemp ).Within( 1e-6 ), "Temperature should be stable at equilibrium." );
        }

        [Test, Description( "Verifies that a tank with supersaturated vapor condenses until it reaches a stable vapor pressure equilibrium." )]
        public void UnstableSupersaturatedVapor_Condenses_AndReachesEquilibrium()
        {
            // Arrange
            double tankVolume = 1.0;
            double temperature = 300;
            double initialGasMass = 0.5; // High mass of gas in a small volume
            IReadonlySubstanceStateCollection contents = new SubstanceStateCollection { { _gasWater, initialGasMass } };
            var currentState = new FluidState( contents.GetPressureInVolume( tankVolume, new FluidState( 0, temperature, 0 ) ), temperature, 0 );

            // Sanity check: initial pressure is much higher than vapor pressure
            Assert.That( currentState.Pressure, Is.GreaterThan( _lqWater.GetVaporPressure( temperature ) * 10 ) );

            // Act
            for( int i = 0; i < 100; i++ )
            {
                (contents, currentState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 0.1 );
            }

            // Assert
            double finalLiquidMass = contents.Contains( _lqWater ) ? contents[_lqWater] : 0;
            double finalGasMass = contents[_gasWater];
            double finalTemp = currentState.Temperature;
            double finalPressure = currentState.Pressure;

            // 1. Phase change occurred correctly
            Assert.That( finalLiquidMass, Is.GreaterThan( 0 ), "Liquid mass should increase." );
            Assert.That( finalGasMass, Is.LessThan( initialGasMass ), "Gas mass should decrease." );
            Assert.That( finalTemp, Is.GreaterThan( temperature ), "Temperature should rise due to condensation." );

            // 2. Equilibrium was reached
            double expectedVaporPressure = _lqWater.GetVaporPressure( finalTemp );
            double finalPartialPressure = contents.GetPartialPressureOfVapor( _gasWater, finalPressure );
            Assert.That( finalPartialPressure, Is.EqualTo( expectedVaporPressure ).Within( 1.0 ).Percent, "Final partial pressure should match vapor pressure at final temperature." );

            // 3. Mass is conserved
            Assert.That( finalLiquidMass + finalGasMass, Is.EqualTo( initialGasMass ).Within( 1e-9 ), "Total mass of water must be conserved." );

            // 4. State is stable
            var (contentsAfter, stateAfter) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 0.1 );
            Assert.That( contentsAfter[_gasWater], Is.EqualTo( finalGasMass ).Within( 1e-6 ), "Gas mass should be stable at equilibrium." );
            Assert.That( stateAfter.Temperature, Is.EqualTo( finalTemp ).Within( 1e-6 ), "Temperature should be stable at equilibrium." );
        }
    }

    // Helper extension method to calculate partial pressure
    internal static class VLETestHelpers
    {
        public static double GetPartialPressureOfVapor( this IReadonlySubstanceStateCollection contents, ISubstance vapor, double totalPressure )
        {
            double totalGasMoles = contents.GetTotalMolesOfPhases( SubstancePhase.Gas );
            if( totalGasMoles <= 1e-9 )
            {
                return 0.0;
            }

            double vaporMoles = contents.TryGet( vapor, out double mass ) ? mass / vapor.MolarMass : 0.0;
            return totalPressure * (vaporMoles / totalGasMoles);
        }
    }
}
