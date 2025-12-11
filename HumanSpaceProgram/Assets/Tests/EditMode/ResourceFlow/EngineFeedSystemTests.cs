using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class EngineFeedSystemTests
    {
        private EngineFeedSystem _feedSystem;

        [SetUp]
        public void SetUp()
        {
            _feedSystem = new EngineFeedSystem();
        }

        [Test]
        public void Sample_WithTargetPressure_ReturnsCorrectNegativePotential()
        {
            // Arrange
            _feedSystem.TargetPressure = 200000; // 2 bar
            _feedSystem.ExpectedDensity = 800;   // Kerosene-like

            double expectedPotential = -(_feedSystem.TargetPressure / _feedSystem.ExpectedDensity); // -200000 / 800 = -250 J/kg

            // Act
            FluidState result = _feedSystem.Sample( Vector3.zero, 0.1 );

            // Assert
            Assert.AreEqual( expectedPotential, result.FluidSurfacePotential, 1e-9, "The calculated suction potential is incorrect." );
            Assert.AreEqual( 0, result.Pressure, "Sampled pressure should be zero for a pure consumer." );
        }

        [Test]
        public void Sample_WithZeroTargetPressure_ReturnsZeroPotential()
        {
            // Arrange
            _feedSystem.TargetPressure = 0;
            _feedSystem.ExpectedDensity = 800;

            // Act
            FluidState result = _feedSystem.Sample( Vector3.zero, 0.1 );

            // Assert
            Assert.AreEqual( 0, result.FluidSurfacePotential, 1e-9, "With zero target pressure, potential should be zero." );
        }

        [Test]
        public void Sample_WithZeroDensity_ReturnsZeroPotentialToPreventDivisionByZero()
        {
            // Arrange
            _feedSystem.TargetPressure = 200000;
            _feedSystem.ExpectedDensity = 0;

            // Act
            FluidState result = _feedSystem.Sample( Vector3.zero, 0.1 );

            // Assert
            Assert.AreEqual( 0, result.FluidSurfacePotential, 1e-9, "With zero density, potential should be zero to prevent division by zero." );
        }
    }
}