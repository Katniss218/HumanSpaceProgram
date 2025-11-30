using HSP.ResourceFlow;
using NUnit.Framework;
using System;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class VaporLiquidEquilibriumTests
    {
        private VLETestSubstance _lqWater;
        private VLETestSubstance _gasWater;
        private VLETestSubstance _lqEthanol;
        private VLETestSubstance _gasEthanol;
        private VLETestSubstance _nonVolatileLiquid;

        // Helper class for creating testable substances with specific thermodynamic properties.
        private class VLETestSubstance : ISubstance
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }
            public Color DisplayColor { get; set; }
            public string[] Tags { get; set; }
            public SubstancePhase Phase { get; set; }
            public double MolarMass { get; set; }
            public double SpecificGasConstant { get; set; }
            public double? FlashPoint { get; set; }
            public double BulkModulus { get; set; } = 2.2e9;

            // Funcs for curves
            public Func<double, double> VaporPressureCurve { get; set; } = temp => 0.0;
            public Func<double> LatentHeatCurve { get; set; } = () => 0.0;
            public Func<double, double, double> SpecificHeatCapacityCurve { get; set; } = ( temp, press ) => 1.0;
            public Func<double, double, double> DensityCurve { get; set; } = ( temp, press ) => 1000.0;

            public double GetBoilingPoint( double pressure ) => throw new NotImplementedException();
            public double GetDensity( double temperature, double pressure ) => DensityCurve( temperature, pressure );
            public double GetLatentHeatOfFusion() => throw new NotImplementedException();
            public double GetLatentHeatOfVaporization() => LatentHeatCurve();
            public double GetPressure( double temperature, double density ) => (SpecificGasConstant > 0) ? density * SpecificGasConstant * temperature : 0.0;
            public double GetSpecificHeatCapacity( double temperature, double pressure ) => SpecificHeatCapacityCurve( temperature, pressure );
            public double GetSpeedOfSound( double temperature, double pressure ) => throw new NotImplementedException();
            public double GetThermalConductivity( double temperature, double pressure ) => throw new NotImplementedException();
            public double GetVaporPressure( double temperature ) => VaporPressureCurve( temperature );
            public double GetViscosity( double temperature, double pressure ) => throw new NotImplementedException();
        }

        [SetUp]
        public void SetUp()
        {
            const double WATER_MOLAR_MASS = 0.01801528; // kg/mol
            const double ETHANOL_MOLAR_MASS = 0.04607;  // kg/mol

            _lqWater = new VLETestSubstance
            {
                ID = "lq_water",
                Phase = SubstancePhase.Liquid,
                MolarMass = WATER_MOLAR_MASS,
                DensityCurve = ( t, p ) => 997, // kg/m^3 at ~300K
                VaporPressureCurve = temp => 3537, // Pa at 300K
                LatentHeatCurve = () => 2.26e6, // J/kg
                SpecificHeatCapacityCurve = ( t, p ) => 4186 // J/(kg*K)
            };

            _gasWater = new VLETestSubstance
            {
                ID = "gas_water",
                Phase = SubstancePhase.Gas,
                MolarMass = WATER_MOLAR_MASS,
                SpecificGasConstant = 461.5, // J/(kg*K)
                DensityCurve = ( t, p ) => p / (461.5 * t),
                SpecificHeatCapacityCurve = ( t, p ) => 1480 // Cv for steam
            };

            _lqEthanol = new VLETestSubstance
            {
                ID = "lq_ethanol",
                Phase = SubstancePhase.Liquid,
                MolarMass = ETHANOL_MOLAR_MASS,
                DensityCurve = ( t, p ) => 789,
                VaporPressureCurve = temp => 7880, // More volatile than water at 300K
                LatentHeatCurve = () => 8.41e5,
                SpecificHeatCapacityCurve = ( t, p ) => 2440
            };

            _gasEthanol = new VLETestSubstance
            {
                ID = "gas_ethanol",
                Phase = SubstancePhase.Gas,
                MolarMass = ETHANOL_MOLAR_MASS,
                SpecificGasConstant = 180.5,
                DensityCurve = ( t, p ) => p / (180.5 * t),
                SpecificHeatCapacityCurve = ( t, p ) => 1430 // Cv
            };

            _nonVolatileLiquid = new VLETestSubstance
            {
                ID = "heavy_oil",
                Phase = SubstancePhase.Liquid,
                MolarMass = 0.3,
                DensityCurve = ( t, p ) => 900,
                VaporPressureCurve = temp => 0 // Non-volatile
            };

            SubstancePhaseMap.Clear();
            SubstancePhaseMap.RegisterPhasePartner( _lqWater, SubstancePhase.Gas, _gasWater );
            SubstancePhaseMap.RegisterPhasePartner( _gasWater, SubstancePhase.Liquid, _lqWater );
            SubstancePhaseMap.RegisterPhasePartner( _lqEthanol, SubstancePhase.Gas, _gasEthanol );
            SubstancePhaseMap.RegisterPhasePartner( _gasEthanol, SubstancePhase.Liquid, _lqEthanol );
        }

        [TearDown]
        public void TearDown()
        {
            SubstancePhaseMap.Clear();
        }

        [Test]
        public void ComputePressureOnly_GasOnly_MatchesIdealGasLaw()
        {
            // Arrange
            double tankVolume = 2.0; // m^3
            double temperature = 300; // K
            double mass = 1.0; // kg
            var contents = new SubstanceStateCollection { { _gasWater, mass } };
            var currentState = new FluidState( 0, temperature, 0 );

            // Act
            double pressure = VaporLiquidEquilibrium.ComputePressureOnly( contents, currentState, tankVolume );

            // Assert
            double moles = mass / _gasWater.MolarMass;
            double R = 8.314462618;
            double expectedPressure = (moles * R * temperature) / tankVolume;

            Assert.AreEqual( expectedPressure, pressure, 1e-3 );
        }

        [Test]
        public void ComputePressureOnly_LiquidOnlyBelowCapacity_IsEffectivelyVacuum()
        {
            // Arrange
            var contents = new SubstanceStateCollection { { _lqWater, 100 } }; // 100kg of water
            var currentState = new FluidState( 0, 300, 0 );

            // Act
            double pressure = VaporLiquidEquilibrium.ComputePressureOnly( contents, currentState, 1.0 );

            // Assert
            Assert.AreEqual( 1e-6, pressure );
        }

        [Test]
        public void ComputePressureOnly_LiquidOverfilled_CalculatesHighPressureViaBulkModulus()
        {
            // Arrange
            double tankVolume = 1.0; // m^3
            double liquidDensity = _lqWater.GetDensity( 300, 101325 );
            double mass = liquidDensity * 1.1; // 10% overfill
            var contents = new SubstanceStateCollection { { _lqWater, mass } };
            var currentState = new FluidState( 0, 300, 0 );

            // Act
            double pressure = VaporLiquidEquilibrium.ComputePressureOnly( contents, currentState, tankVolume );

            // Assert
            double fillRatio = (mass / liquidDensity) / tankVolume;
            double expectedPressure = (fillRatio - 1.0) * 2.2e9; // 2.2e9 is the hardcoded bulk modulus
            Assert.AreEqual( expectedPressure, pressure, 1e-3 );
        }

        [Test]
        public void ComputePressureOnly_MixedLiquidAndGas_UsesUllageVolume()
        {
            // Arrange
            double tankVolume = 10.0; // m^3
            double temperature = 300; // K
            double liquidMass = 997; // ~1 m^3 of water
            double gasMass = 1.0; // kg of steam
            var contents = new SubstanceStateCollection { { _lqWater, liquidMass }, { _gasWater, gasMass } };
            var currentState = new FluidState( 0, temperature, 0 );

            // Act
            double pressure = VaporLiquidEquilibrium.ComputePressureOnly( contents, currentState, tankVolume );

            // Assert
            double liquidVolume = liquidMass / _lqWater.GetDensity( temperature, 0 );
            double ullageVolume = tankVolume - liquidVolume;
            double gasMoles = gasMass / _gasWater.MolarMass;
            double R = 8.314462618;
            double expectedPressure = (gasMoles * R * temperature) / ullageVolume;

            Assert.AreEqual( expectedPressure, pressure, 1e-3 );
        }

        [Test]
        public void ComputeFlash2_EvaporationOccurs_WhenPartialPressureIsLow()
        {
            // Arrange
            double tankVolume = 10.0;
            double temperature = 300;
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } }; // Only liquid, so partial pressure of gas is 0
            var currentState = new FluidState( 0, temperature, 0 );

            // Vapor pressure of water at 300K is ~3537 Pa. Since partial pressure is 0, evaporation should occur.

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, tankVolume, 1.0 );

            // Assert
            Assert.Less( updatedContents[_lqWater], 1.0, "Liquid mass should decrease due to evaporation." );
            Assert.Greater( updatedContents[_gasWater], 0.0, "Gas mass should increase due to evaporation." );
            Assert.Less( newState.Temperature, temperature, "Temperature should drop due to latent heat of vaporization." );
        }

        [Test]
        public void ComputeFlash2_CondensationOccurs_WhenPartialPressureIsHigh()
        {
            // Arrange
            double tankVolume = 1.0;
            double temperature = 300;
            // High mass of gas in a small volume to create high partial pressure
            var contents = new SubstanceStateCollection { { _lqWater, 0.01 }, { _gasWater, 1.0 } };
            var currentState = new FluidState( 0, temperature, 0 );

            // Vapor pressure at 300K is ~3537 Pa. The initial pressure from the gas will be much higher, so condensation should occur.

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, tankVolume, 1.0 );

            // Assert
            Assert.Greater( updatedContents[_lqWater], 0.01, "Liquid mass should increase due to condensation." );
            Assert.Less( updatedContents[_gasWater], 1.0, "Gas mass should decrease due to condensation." );
            Assert.Greater( newState.Temperature, temperature, "Temperature should rise due to latent heat of condensation." );
        }

        [Test]
        public void ComputeFlash2_MassIsConserved_DuringPhaseChange()
        {
            // Arrange
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            var currentState = new FluidState( 0, 300, 0 );
            double initialTotalMass = contents.GetMass();

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, 10.0, 1.0 );

            // Assert
            double finalTotalMass = updatedContents.GetMass();
            Assert.AreEqual( initialTotalMass, finalTotalMass, 1e-9, "Total mass of substance should be conserved across phases." );
        }

        [Test]
        public void ComputeFlash2_HydraulicLock_PreventsEvaporation()
        {
            // Arrange
            double tankVolume = 1.0;
            double liquidDensity = _lqWater.GetDensity( 300, 0 );
            // Fill tank exactly with liquid
            var contents = new SubstanceStateCollection { { _lqWater, liquidDensity * tankVolume } };
            var currentState = new FluidState( 0, 300, 0 );

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, tankVolume, 1.0 );

            // Assert
            Assert.IsFalse( updatedContents.Contains( _gasWater ), "No gas should be created when tank is full of liquid." );
            Assert.AreEqual( contents.GetMass(), updatedContents.GetMass(), 1e-9, "Liquid mass should not change." );
        }

        [Test]
        public void ComputeFlash2_Equilibrium_ResultsInNoSignificantChange()
        {
            // Arrange
            double tankVolume = 10.0;
            double temperature = 300;
            // Create a state that is already at equilibrium
            // Partial pressure of gas = Vapor pressure of liquid at this temp
            double vaporPressure = _lqWater.GetVaporPressure( temperature ); // 3537 Pa
            double R = 8.314462618;
            double requiredMoles = (vaporPressure * tankVolume) / (R * temperature);
            double requiredMass = requiredMoles * _gasWater.MolarMass;

            var contents = new SubstanceStateCollection { { _lqWater, 1.0 }, { _gasWater, requiredMass } };
            var currentState = new FluidState( vaporPressure, temperature, 0 );

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, tankVolume, 1.0 );

            // Assert
            Assert.AreEqual( 1.0, updatedContents[_lqWater], 1e-2, "Liquid mass should not change significantly at equilibrium." );
            Assert.AreEqual( requiredMass, updatedContents[_gasWater], 1e-2, "Gas mass should not change significantly at equilibrium." );
            Assert.AreEqual( temperature, newState.Temperature, 1e-2, "Temperature should not change significantly at equilibrium." );
        }

        [Test]
        public void ComputeFlash2_MultipleVolatiles_BothEvaporate()
        {
            // Arrange
            double tankVolume = 10.0;
            double temperature = 300;
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 }, { _lqEthanol, 1.0 } };
            var currentState = new FluidState( 0, temperature, 0 );

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, tankVolume, 1.0 );

            // Assert
            Assert.Less( updatedContents[_lqWater], 1.0, "Water should evaporate." );
            Assert.Less( updatedContents[_lqEthanol], 1.0, "Ethanol should evaporate." );
            Assert.Greater( updatedContents[_gasWater], 0.0, "Water vapor should be produced." );
            Assert.Greater( updatedContents[_gasEthanol], 0.0, "Ethanol vapor should be produced." );
            Assert.Less( newState.Temperature, temperature );
        }

        [Test]
        public void ComputeFlash2_InvalidMolarMass_Zero_IsStable()
        {
            // Arrange
            var badSubstance = new VLETestSubstance { ID = "bad", Phase = SubstancePhase.Liquid, MolarMass = 0.0 };
            SubstancePhaseMap.RegisterPhasePartner( badSubstance, SubstancePhase.Gas, badSubstance ); // Map to self to avoid nullref

            var contents = new SubstanceStateCollection { { _lqWater, 1.0 }, { badSubstance, 1.0 } };
            var currentState = new FluidState( 0, 300, 0 );

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash2( contents, currentState, 10.0, 1.0 );

            // Assert
            Assert.IsFalse( double.IsNaN( newState.Pressure ), "Pressure should not be NaN." );
            Assert.IsFalse( double.IsNaN( newState.Temperature ), "Temperature should not be NaN." );
            Assert.IsFalse( double.IsNaN( updatedContents[_lqWater] ), "Water mass should not be NaN." );
            Assert.IsFalse( double.IsNaN( updatedContents[badSubstance] ), "Bad substance mass should not be NaN." );

            // The bad substance should effectively be ignored by the VLE calculation
            Assert.AreEqual( 1.0, updatedContents[badSubstance], 1e-9 );
        }
    }
}