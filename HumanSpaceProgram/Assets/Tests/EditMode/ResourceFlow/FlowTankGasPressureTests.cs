using HSP.ResourceFlow;
using NUnit.Framework;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowTankGasPressureTests
    {
        private FlowTank _tank;

        [SetUp]
        public void SetUp()
        {
            _tank = new FlowTank( 1.0 ); // 1 m^3
            _tank.FluidState = new FluidState( 0, 300, 0 );
        }

        [Test]
        public void ApplyFlows_AddGasToEmptyTank_IncreasesPressure()
        {
            // Arrange
            double addedMass = 1.2; // ~1 atm in 1m^3 at 300K
            _tank.Inflow.Add( TestSubstances.Air, addedMass );

            // Act
            _tank.ApplySolveResults( 0.02 );

            // Assert
            double expectedMoles = addedMass / TestSubstances.Air.MolarMass;
            const double R = 8.314462618;
            double expectedPressure = (expectedMoles * R * 300) / 1.0;

            Assert.That( _tank.FluidState.Pressure, Is.EqualTo( expectedPressure ).Within( 0.1 ).Percent );
            Assert.AreEqual( addedMass, _tank.Contents.GetMass(), 1e-9 );
        }

        [Test]
        public void ApplyFlows_RemoveGasFromPressurizedTank_DecreasesPressure()
        {
            // Arrange
            double initialMass = 1.2;
            _tank.Contents.Add( TestSubstances.Air, initialMass );
            _tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( _tank.Contents, _tank.FluidState, _tank.Volume ), 300, 0 );

            double removedMass = 0.6;
            _tank.Outflow.Add( TestSubstances.Air, removedMass );

            // Act
            _tank.ApplySolveResults( 0.02 );

            // Assert
            double finalMass = initialMass - removedMass;
            double expectedMoles = finalMass / TestSubstances.Air.MolarMass;
            const double R = 8.314462618;
            double expectedPressure = (expectedMoles * R * 300) / 1.0;

            Assert.That( _tank.FluidState.Pressure, Is.EqualTo( expectedPressure ).Within( 0.1 ).Percent );
            Assert.AreEqual( finalMass, _tank.Contents.GetMass(), 1e-9 );
        }

        [Test]
        public void ApplyFlows_AddLiquidToGasTank_IncreasesPressureByReducingUllage()
        {
            // Arrange
            double gasMass = 1.2;
            _tank.Contents.Add( TestSubstances.Air, gasMass );
            _tank.FluidState = new FluidState( VaporLiquidEquilibrium.ComputePressureOnly( _tank.Contents, _tank.FluidState, _tank.Volume ), 300, 0 );
            double initialPressure = _tank.FluidState.Pressure;

            double liquidMass = 500; // 0.5 m^3 of water
            _tank.Inflow.Add( TestSubstances.Water, liquidMass );

            // Act
            _tank.ApplySolveResults( 0.02 );

            // Assert
            double liquidVolume = liquidMass / TestSubstances.Water.GetDensity( 300, _tank.FluidState.Pressure );
            double ullageVolume = _tank.Volume - liquidVolume;

            double expectedMoles = gasMass / TestSubstances.Air.MolarMass;
            const double R = 8.314462618;
            double expectedPressure = (expectedMoles * R * 300) / ullageVolume;

            Assert.Greater( _tank.FluidState.Pressure, initialPressure );
            Assert.That( _tank.FluidState.Pressure, Is.EqualTo( expectedPressure ).Within( 0.1 ).Percent );
            Assert.AreEqual( gasMass + liquidMass, _tank.Contents.GetMass(), 1e-9 );
        }
    }
}
