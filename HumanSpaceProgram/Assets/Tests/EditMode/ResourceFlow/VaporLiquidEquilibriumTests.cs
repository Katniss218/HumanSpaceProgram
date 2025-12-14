using HSP.ResourceFlow;
using NUnit.Framework;
using System;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class VaporLiquidSolidEquilibriumTests
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

        #region GetPressureInVolume Tests

        [Test]
        public void GetPressureInVolume_GasOnly_MatchesIdealGasLaw()
        {
            double tankVolume = 2.0;
            double temperature = 300;
            double mass = 1.0;
            var contents = new SubstanceStateCollection { { _gasWater, mass } };
            var currentState = new FluidState( 0, temperature, 0 );

            double pressure = contents.GetPressureInVolume( tankVolume, currentState );

            double moles = mass / _gasWater.MolarMass;
            const double R = 8.314462618;
            double expectedPressure = (moles * R * temperature) / tankVolume;

            Assert.That( pressure, Is.EqualTo( expectedPressure ).Within( 1e-3 ) );
        }

        [Test]
        public void GetPressureInVolume_LiquidOnlyBelowCapacity_IsEffectivelyVacuum()
        {
            var contents = new SubstanceStateCollection { { _lqWater, 100 } };
            var currentState = new FluidState( 0, 300, 0 );

            double pressure = contents.GetPressureInVolume( 1.0, currentState );

            Assert.That( pressure, Is.EqualTo( 1e-6 ) );
        }

        [Test]
        public void GetPressureInVolume_LiquidOverfilled_CalculatesHighPressureViaBulkModulus()
        {
            double tankVolume = 1.0;
            double liquidDensity = _lqWater.GetDensity( 300, 101325 );
            double mass = liquidDensity * 1.1; // 10% overfill
            var contents = new SubstanceStateCollection { { _lqWater, mass } };
            var currentState = new FluidState( 0, 300, 0 );

            double pressure = contents.GetPressureInVolume( tankVolume, currentState );

            double fillRatio = (mass / liquidDensity) / tankVolume;
            double expectedPressure = (fillRatio - 1.0) * _lqWater.BulkModulus;

            Assert.That( pressure, Is.EqualTo( expectedPressure ).Within( 0.1 ).Percent );
        }

        [Test]
        public void GetPressureInVolume_MixedLiquidAndGas_UsesUllageVolume()
        {
            double tankVolume = 10.0;
            double temperature = 300;
            double liquidMass = 997; // ~1 m^3 of water
            double gasMass = 1.0;
            var contents = new SubstanceStateCollection { { _lqWater, liquidMass }, { _gasWater, gasMass } };
            var currentState = new FluidState( 0, temperature, 0 );

            double pressure = contents.GetPressureInVolume( tankVolume, currentState );

            double liquidVolume = liquidMass / _lqWater.GetDensity( temperature, 0 );
            double ullageVolume = tankVolume - liquidVolume;
            double gasMoles = gasMass / _gasWater.MolarMass;
            const double R = 8.314462618;
            double expectedPressure = (gasMoles * R * temperature) / ullageVolume;

            Assert.That( pressure, Is.EqualTo( expectedPressure ).Within( 1e-3 ) );
        }

        #endregion

        #region ComputeFlash_Stable VLE Tests

        [Test]
        public void ComputeFlash_Stable_EvaporationOccurs_WhenPartialPressureIsLow()
        {
            double tankVolume = 10.0;
            double temperature = 300; // Use a standard temperature
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            // Start with vacuum ullage, this is an unstable state that should cause boiling.
            var currentState = new FluidState( 0.01, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 1.0 );

            Assert.That( updatedContents[_lqWater], Is.LessThan( 1.0 ) );
            Assert.That( updatedContents.Contains( _gasWater ), Is.True );
            Assert.That( updatedContents[_gasWater], Is.GreaterThan( 0.0 ) );
            Assert.That( newState.Temperature, Is.LessThan( temperature ) );
        }

        [Test]
        public void ComputeFlash_Stable_CondensationOccurs_WhenPartialPressureIsHigh()
        {
            double tankVolume = 1.0;
            double temperature = 300;
            // Start with gas pressure far above vapor pressure for this temp (~3537 Pa)
            var contents = new SubstanceStateCollection { { _lqWater, 0.01 }, { _gasWater, 1.0 } };
            var currentState = new FluidState( 0, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 1.0 );

            Assert.That( updatedContents[_lqWater], Is.GreaterThan( 0.01 ) );
            Assert.That( updatedContents[_gasWater], Is.LessThan( 1.0 ) );
            Assert.That( newState.Temperature, Is.GreaterThan( temperature ) );
        }

        [Test]
        public void ComputeFlash_Stable_Equilibrium_ResultsInNoSignificantChange()
        {
            double tankVolume = 10.0;
            double temperature = 325;
            double liquidMass = 7;
            double vaporPressure = _lqWater.GetVaporPressure( temperature );
            double liquidVolume = liquidMass / _lqWater.GetDensity( temperature, vaporPressure );
            double ullageVolume = tankVolume - liquidVolume;
            double gasMass = _gasWater.GetMassForPressureInVolume( vaporPressure, ullageVolume, temperature );

            var contents = new SubstanceStateCollection()
            {
                { _lqWater, liquidMass },
                { _gasWater, gasMass }
            };
            var currentState = new FluidState( vaporPressure, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 0, 1.0 );

            Assert.That( updatedContents[_lqWater], Is.EqualTo( liquidMass ).Within( 1e-2 ) );
            Assert.That( updatedContents[_gasWater], Is.EqualTo( gasMass ).Within( 1e-2 ) );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-2 ) );
        }

        #endregion

        #region ComputeFlash_Stable SLE Tests

        [Test]
        public void ComputeFlash_Stable_MeltingOccurs_WhenAboveMeltingPointWithHeat()
        {
            double temperature = 273.15; // At melting point
            var contents = new SubstanceStateCollection { { _ice, 1.0 } };
            var currentState = new FluidState( 101325, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 1.0, 10000, 1.0 );

            Assert.That( updatedContents[_ice], Is.LessThan( 1.0 ), "Ice mass should decrease." );
            Assert.That( updatedContents.Contains( _lqWater ), Is.True, "Liquid water should be created." );
            Assert.That( updatedContents[_lqWater], Is.GreaterThan( 0.0 ), "Liquid water should be created." );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-3 ), "Temperature should remain at melting point during phase change." );
        }

        [Test]
        public void ComputeFlash_Stable_FreezingOccurs_WhenBelowFreezingPointWithCooling()
        {
            double temperature = 273.15; // At freezing point
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            var currentState = new FluidState( 101325, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 1.0, -10000, 1.0 );

            Assert.That( updatedContents[_lqWater], Is.LessThan( 1.0 ), "Liquid mass should decrease." );
            Assert.That( updatedContents.Contains( _ice ), Is.True, "Ice should be created." );
            Assert.That( updatedContents[_ice], Is.GreaterThan( 0.0 ), "Ice should be created." );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-3 ), "Temperature should remain at freezing point during phase change." );
        }

        [Test]
        public void ComputeFlash_Stable_HeatingSolid_RaisesTemperatureBelowMeltingPoint()
        {
            double temperature = 260.0; // Below freezing
            var contents = new SubstanceStateCollection { { _ice, 1.0 } };
            var currentState = new FluidState( 101325, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 1.0, 1000, 1.0 );

            Assert.That( updatedContents.Contains( _lqWater ), Is.False, "No melting should occur below melting point." );
            Assert.That( newState.Temperature, Is.GreaterThan( temperature ), "Temperature should rise." );
        }

        #endregion

        #region Conservation & Stability Tests

        [Test]
        public void ComputeFlash_Stable_MassIsConserved_DuringPhaseChange()
        {
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            var currentState = new FluidState( 0, 300, 0 );
            double initialTotalMass = contents.GetMass();

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 10.0, 10000, 1.0 );

            double finalTotalMass = updatedContents.GetMass();
            Assert.That( finalTotalMass, Is.EqualTo( initialTotalMass ).Within( 1e-9 ) );
        }

        [Test]
        public void ComputeFlash_Stable_HydraulicLock_PreventsEvaporation()
        {
            double tankVolume = 1.0;
            double liquidDensity = _lqWater.GetDensity( 300, 0 );
            var contents = new SubstanceStateCollection { { _lqWater, liquidDensity * tankVolume * 1.01 } };
            var currentState = new FluidState( 0, 300, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, 1000, 1.0 );

            Assert.That( updatedContents.Contains( _gasWater ), Is.False, "No gas should be created when tank is full." );
            Assert.That( updatedContents.GetMass(), Is.EqualTo( contents.GetMass() ).Within( 1e-9 ) );
        }

        [Test]
        public void ComputeFlash_Stable_EnergyIsConserved_WhenBoiling()
        {
            double temperature = 373.15; // Boiling point at ~1 atm
            double pressure = 101325;
            double heatInput = 10000; // Watts
            double deltaTime = 1.0;
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            var currentState = new FluidState( pressure, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 10.0, heatInput, deltaTime );

            double massBoiled = 1.0 - updatedContents[_lqWater];
            double latentHeatUsed = massBoiled * _lqWater.GetLatentHeatOfVaporization();

            Assert.That( updatedContents.Contains( _gasWater ), Is.True, "Gas should have been created." );
            Assert.That( massBoiled, Is.GreaterThan( 0 ), "Some liquid should have boiled." );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-3 ), "Temperature should remain at boiling point during partial boiling." );
            Assert.That( latentHeatUsed, Is.EqualTo( heatInput * deltaTime ).Within( 0.1 ).Percent, "Energy balance (Heat In = Latent Heat) must be maintained during boiling." );
        }

        [Test]
        public void ComputeFlash_Stable_EnergyIsConserved_WhenCondensing()
        {
            double temperature = 373.15; // Condensation point at ~1 atm
            double pressure = 101325;
            double heatInput = -10000; // Watts (cooling)
            double deltaTime = 1.0;
            var contents = new SubstanceStateCollection { { _gasWater, 1.0 } };
            var currentState = new FluidState( pressure, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 1.0, heatInput, deltaTime );

            double massCondensed = 1.0 - updatedContents[_gasWater];
            double latentHeatReleased = massCondensed * _lqWater.GetLatentHeatOfVaporization();

            Assert.That( updatedContents.Contains( _lqWater ), Is.True, "Liquid should have been created." );
            Assert.That( massCondensed, Is.GreaterThan( 0 ), "Some gas should have condensed." );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-3 ), "Temperature should remain at condensation point during partial condensation." );
            Assert.That( latentHeatReleased, Is.EqualTo( -heatInput * deltaTime ).Within( 0.1 ).Percent, "Energy balance (Heat Out = Latent Heat) must be maintained during condensation." );
        }

        [Test]
        public void ComputeFlash_Stable_EnergyIsConserved_WhenMelting()
        {
            double temperature = 273.15; // At melting point
            double heatInput = 10000; // Watts
            double deltaTime = 1.0;
            var contents = new SubstanceStateCollection { { _ice, 1100 } }; // Ensure overfilled to stop boiling.
            var currentState = new FluidState( 101325, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 1.0, heatInput, deltaTime );

            double massMelted = 1100.0 - updatedContents[_ice];
            double latentHeatUsed = massMelted * _ice.GetLatentHeatOfFusion();

            Assert.That( updatedContents.Contains( _lqWater ), Is.True, "Liquid water should be created." );
            //Assert.That( updatedContents.Contains( _gasWater ), Is.False, "No gas should be created." );
            Assert.That( massMelted, Is.GreaterThan( 0 ), "Some ice should have melted." );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-3 ), "Temperature should remain at melting point during partial melting." );
            Assert.That( latentHeatUsed, Is.EqualTo( heatInput * deltaTime ).Within( 0.1 ).Percent, "Energy balance (Heat In = Latent Heat) must be maintained during melting." );
        }

        [Test]
        public void ComputeFlash_Stable_EnergyIsConserved_WhenFreezing()
        {
            double temperature = 273.15; // At freezing point
            double heatInput = -10000; // Watts (cooling)
            double deltaTime = 1.0;
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            var currentState = new FluidState( 101325, temperature, 0 );

            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 1.0, heatInput, deltaTime );

            double massFrozen = updatedContents.Contains( _ice ) ? updatedContents[_ice] : 0.0;
            double latentHeatReleased = massFrozen * _lqWater.GetLatentHeatOfFusion();

            Assert.That( updatedContents.Contains( _ice ), Is.True, "Ice should be created." );
            Assert.That( massFrozen, Is.GreaterThan( 0 ), "Some liquid should have frozen." );
            Assert.That( newState.Temperature, Is.EqualTo( temperature ).Within( 1e-3 ), "Temperature should remain at freezing point during partial freezing." );
            Assert.That( latentHeatReleased, Is.EqualTo( -heatInput * deltaTime ).Within( 0.1 ).Percent, "Energy balance (Heat Out = Latent Heat) must be maintained during freezing." );
        }

        [Test]
        public void ComputeFlash_Stable_UnstableStateWithNoHeat_MovesTowardEquilibrium()
        {
            // Arrange
            double temp = 323; // Not near any phase change for water
            var contents = new SubstanceStateCollection { { _lqWater, 1.0 } };
            var currentState = new FluidState( 101325, temp, 0 );

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, 10.0, 0.0, 1.0 );
            Debug.Log( newState );

            // Assert: The physically correct result is that the state changes.
            // Some liquid boils to create vapor pressure, and the temperature drops as a result.
            Assert.That( updatedContents[_lqWater], Is.LessThan( 1.0 ), "Some liquid should have boiled." );
            Assert.That( updatedContents.Contains( _gasWater ), Is.True, "Gas should have been created." );
            Assert.That( newState.Temperature, Is.LessThan( currentState.Temperature ), "Temperature should drop due to boiling." );
            double expectedPressure = _lqWater.GetVaporPressure( newState.Temperature );
            Assert.That( newState.Pressure, Is.EqualTo( expectedPressure ).Within( 10.0 ).Percent, "Pressure should be close to vapor pressure." );
        }

        #endregion

        #region Sensible Heat Tests

        [Test]
        public void ComputeFlash_Stable_HeatingLiquid_RaisesTemperature_NoPhaseChange()
        {
            // Arrange
            double temperature = 300.0; // Well between freezing and boiling
            double tankVolume = 1.0;
            // Overfill the tank to prevent boiling due to pressure.
            double liquidMass = _lqWater.GetDensity( temperature, 101325 ) * tankVolume * 1.001;
            var contents = new SubstanceStateCollection { { _lqWater, liquidMass } };
            var currentState = new FluidState( 101325, temperature, 0 );
            double heatInput = 1000.0; // W
            double deltaTime = 1.0; // s

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, heatInput, deltaTime );

            // Assert
            double totalHeatCapacity = liquidMass * _lqWater.GetSpecificHeatCapacity( temperature, currentState.Pressure );
            double expectedTempChange = (heatInput * deltaTime) / totalHeatCapacity;
            double expectedFinalTemp = temperature + expectedTempChange;

            Assert.That( updatedContents.Contains( _gasWater ), Is.False, "No gas should have been created in a full tank." );
            Assert.That( updatedContents.Contains( _ice ), Is.False, "No freezing should occur." );
            Assert.That( updatedContents[_lqWater], Is.EqualTo( liquidMass ).Within( 1e-9 ), "Liquid mass should remain constant." );
            Assert.That( newState.Temperature, Is.EqualTo( expectedFinalTemp ).Within( 1e-6 ), "Temperature should rise due to heating." );
        }

        [Test]
        public void ComputeFlash_Stable_CoolingLiquid_LowersTemperature_NoPhaseChange()
        {
            // Arrange
            double temperature = 325.0;
            double tankVolume = 1.0;
            // Overfill the tank to prevent boiling due to pressure.
            double liquidMass = _lqWater.GetDensity( temperature, 101325 ) * tankVolume * 1.001;
            var contents = new SubstanceStateCollection { { _lqWater, liquidMass } };
            var currentState = new FluidState( 101325, temperature, 0 );
            double heatInput = -1000.0; // Cooling
            double deltaTime = 1.0;

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, heatInput, deltaTime );

            // Assert
            double totalHeatCapacity = liquidMass * _lqWater.GetSpecificHeatCapacity( temperature, currentState.Pressure );
            double expectedTempChange = (heatInput * deltaTime) / totalHeatCapacity;
            double expectedFinalTemp = temperature + expectedTempChange;

            Assert.That( updatedContents.Contains( _gasWater ), Is.False, "No phase change should occur." );
            Assert.That( updatedContents.Contains( _ice ), Is.False, "No freezing should occur." );
            Assert.That( updatedContents[_lqWater], Is.EqualTo( liquidMass ).Within( 1e-9 ), "Liquid mass should remain constant." );
            Assert.That( newState.Temperature, Is.EqualTo( expectedFinalTemp ).Within( 1e-6 ), "Temperature should fall due to cooling." );
        }

        [Test]
        public void ComputeFlash_Stable_HeatingGas_RaisesTemperature_NoPhaseChange()
        {
            // Arrange
            double temperature = 400.0; // Well above boiling
            double mass = 1.0;
            double tankVolume = 1.0;
            var contents = new SubstanceStateCollection { { _gasWater, mass } };
            var currentState = new FluidState( contents.GetPressureInVolume( tankVolume, new FluidState( 0, temperature, 0 ) ), temperature, 0 );
            double heatInput = 1000.0;
            double deltaTime = 1.0;

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, heatInput, deltaTime );

            // Assert
            double specificHeat = _gasWater.GetSpecificHeatCapacity( temperature, currentState.Pressure );
            double expectedTempChange = (heatInput * deltaTime) / (mass * specificHeat);
            double expectedFinalTemp = temperature + expectedTempChange;

            Assert.That( newState.Temperature, Is.EqualTo( expectedFinalTemp ).Within( 1e-6 ), "Gas temperature should rise." );
            Assert.That( updatedContents.Contains( _lqWater ), Is.False, "No condensation should occur." );
            Assert.That( updatedContents[_gasWater], Is.EqualTo( mass ).Within( 1e-9 ), "Gas mass should remain constant." );
        }

        [Test]
        public void ComputeFlash_Stable_CoolingGas_LowersTemperature_NoPhaseChange()
        {
            // Arrange
            double temperature = 400.0; // Well above dew point for this pressure.
            double mass = 1.0;
            double tankVolume = 1.0;
            var contents = new SubstanceStateCollection { { _gasWater, mass } };
            var currentState = new FluidState( contents.GetPressureInVolume( tankVolume, new FluidState( 0, temperature, 0 ) ), temperature, 0 );
            double heatInput = -1000.0; // Cooling
            double deltaTime = 1.0;

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, heatInput, deltaTime );

            // Assert
            double specificHeat = _gasWater.GetSpecificHeatCapacity( temperature, currentState.Pressure );
            double expectedTempChange = (heatInput * deltaTime) / (mass * specificHeat);
            double expectedFinalTemp = temperature + expectedTempChange;

            Assert.That( newState.Temperature, Is.EqualTo( expectedFinalTemp ).Within( 1e-6 ), "Gas temperature should fall." );
            Assert.That( updatedContents.Contains( _lqWater ), Is.False, "No condensation should occur." );
            Assert.That( updatedContents[_gasWater], Is.EqualTo( mass ).Within( 1e-9 ), "Gas mass should remain constant." );
        }

        [Test]
        public void ComputeFlash_Stable_LowHeatInput_UsesEarlyExitPathForSensibleHeat()
        {
            // Arrange
            double temperature = 200.0;
            double tankVolume = 1.0;
            // A full tank of subcooled liquid is not near a phase boundary and has no VLE imbalance.
            // This state should take the early exit path for a small heat input.
            double liquidMass = _lqWater.GetDensity( temperature, 101325 ) * tankVolume;
            var contents = new SubstanceStateCollection { { _lqWater, liquidMass } };
            var currentState = new FluidState( 101325, temperature, 0 );
            double heatInput = 1.0; // Very low heat
            double deltaTime = 1.0;

            // Act
            (var updatedContents, var newState) = VaporLiquidEquilibrium.ComputeFlash( contents, currentState, tankVolume, heatInput, deltaTime );

            // Assert
            // The early exit path should be taken.
            double totalHeatCapacity = liquidMass * _lqWater.GetSpecificHeatCapacity( temperature, currentState.Pressure );
            double expectedTempChange = (heatInput * deltaTime) / totalHeatCapacity;
            double expectedFinalTemp = temperature + expectedTempChange;

            Assert.That( newState.Temperature, Is.EqualTo( expectedFinalTemp ).Within( 1e-6 ), "Temperature should change correctly via early exit path." );
            // In the early exit path, no mass transfer occurs.
            Assert.That( updatedContents.Contains( _gasWater ), Is.False, "No phase change should occur on early exit." );
            Assert.That( updatedContents[_lqWater], Is.EqualTo( liquidMass ).Within( 1e-9 ), "Liquid mass should be constant on early exit." );
        }

        #endregion
    }
}