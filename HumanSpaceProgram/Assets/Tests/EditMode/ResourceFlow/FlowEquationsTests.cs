using HSP.ResourceFlow;
using NUnit.Framework;
using System;

namespace HSP_Tests_EditMode.ResourceFlow
{
    public class FlowEquationsTests
    {
        private const double TOLERANCE = 1e-6;

        // Properties for water at 20C
        private const double WATER_VISCOSITY = 0.001002; // Pa*s
        private const double WATER_DENSITY = 998.2;   // kg/m^3

        // Properties for air at 20C
        private const double AIR_DENSITY = 1.204;
        private const double AIR_VISCOSITY = 1.81e-5;
        private const double AIR_SPEED_OF_SOUND = 343;

        [Test]
        public void GetReynoldsNumber_LaminarFlow_IsCorrect()
        {
            // Arrange: Slow flow of water in a small pipe
            double massFlowRate = 0.01; // kg/s
            double diameter = 0.01;     // m (1 cm)

            // Act
            double re = FlowEquations.GetReynoldsNumber( massFlowRate, diameter, WATER_VISCOSITY );

            // Assert
            // Re = (4 * 0.01) / (PI * 0.01 * 0.001002) = 1270.6
            double expectedRe = (4.0 * massFlowRate) / (Math.PI * diameter * WATER_VISCOSITY);
            Assert.That( re, Is.EqualTo( expectedRe ).Within( TOLERANCE ) );
            Assert.That( re, Is.LessThan( 2300 ), "Flow should be laminar." );
        }

        [Test]
        public void GetReynoldsNumber_TurbulentFlow_IsCorrect()
        {
            // Arrange: Fast flow of water in a larger pipe
            double massFlowRate = 5.0; // kg/s
            double diameter = 0.05;    // m (5 cm)

            // Act
            double re = FlowEquations.GetReynoldsNumber( massFlowRate, diameter, WATER_VISCOSITY );

            // Assert
            // Re = (4 * 5.0) / (PI * 0.05 * 0.001002) = 127068
            double expectedRe = (4.0 * massFlowRate) / (Math.PI * diameter * WATER_VISCOSITY);
            Assert.That( re, Is.EqualTo( expectedRe ).Within( TOLERANCE ) );
            Assert.That( re, Is.GreaterThan( 4000 ), "Flow should be turbulent." );
        }

        [Test]
        public void GetDarcyFrictionFactor_Blasius_IsCorrect()
        {
            // Arrange
            double reynoldsNumber = 10000;

            // Act
            double f = FlowEquations.GetDarcyFrictionFactor( reynoldsNumber );

            // Assert
            // f = 0.3164 * 10000^-0.25 = 0.3164 * 0.1 = 0.03164
            double expectedF = 0.3164 * Math.Pow( reynoldsNumber, -0.25 );
            Assert.That( f, Is.EqualTo( expectedF ).Within( TOLERANCE ) );
        }

        [Test]
        public void GetMassConductance_Laminar_IsCorrect()
        {
            // Arrange
            double area = 0.01;
            double length = 10.0;

            // Act
            double conductance = FlowEquations.GetLaminarMassConductance( WATER_DENSITY, area, length, WATER_VISCOSITY );

            // Assert
            // C = (rho^2 * A^2) / (8 * pi * mu * L)
            double expectedC = (WATER_DENSITY * WATER_DENSITY * area * area) / (8 * Math.PI * WATER_VISCOSITY * length);
            Assert.That( conductance, Is.EqualTo( expectedC ).Within( TOLERANCE ) );
        }

        [Test]
        public void GetMassConductance_Turbulent_IsCorrect()
        {
            // Arrange
            double area = 0.01;
            double diameter = Math.Sqrt( 4 * area / Math.PI );
            double length = 10.0;
            double frictionFactor = 0.03;
            double lastMassFlowRate = 10.0; // kg/s

            // Act
            double conductance = FlowEquations.GetTurbulentMassConductance( WATER_DENSITY, area, diameter, length, frictionFactor, lastMassFlowRate );

            // Assert
            // C = (2 * rho^2 * A^2 * D) / (f * L * |m_dot|)
            double expectedC = (2.0 * WATER_DENSITY * WATER_DENSITY * area * area * diameter) / (frictionFactor * length * lastMassFlowRate);
            Assert.That( conductance, Is.EqualTo( expectedC ).Within( TOLERANCE ) );
        }

        [Test]
        public void GetMassConductance_Turbulent_WithZeroFlow_ReturnsHighConductance()
        {
            // Arrange
            double area = 0.01;
            double diameter = Math.Sqrt( 4 * area / Math.PI );
            double length = 10.0;
            double frictionFactor = 0.03;
            double lastMassFlowRate = 0.0;

            // Act
            double conductance = FlowEquations.GetTurbulentMassConductance( WATER_DENSITY, area, diameter, length, frictionFactor, lastMassFlowRate );

            // Assert
            // Should return a large number to allow flow to start
            Assert.That( conductance, Is.EqualTo( 1e9 ) );
        }

        [Test]
        public void GetMaxMassFlow_Choked_IsCorrect()
        {
            // Arrange
            double density = AIR_DENSITY;
            double area = 0.01;
            double speedOfSound = AIR_SPEED_OF_SOUND;

            // Act
            double maxFlow = FlowEquations.GetChokedMassFlow( density, area, speedOfSound );

            // Assert
            // m_max = rho * A * c
            double expectedMaxFlow = density * area * speedOfSound;
            Assert.That( maxFlow, Is.EqualTo( expectedMaxFlow ).Within( TOLERANCE ) );
        }
    }
}