using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowTankThermodynamicsTests
    {
        private const double DT = 0.02;

        private ISubstance _lqWater;
        private ISubstance _gasWater;

        [SetUp]
        public void SetUp()
        {
            const double WATER_MOLAR_MASS = 0.01801528;

            _lqWater = new Substance( "test_lq_water" )
            {
                Phase = SubstancePhase.Liquid,
                MolarMass = WATER_MOLAR_MASS,
                ReferenceDensity = 997,
                BulkModulus = 2.2e9,
                // Antoine Coeffs for water, T in Kelvin, P in Pa.
                // log10(P) = A - B / (T + C)
                // The default coeffs are { 10.196, 1730.63, -39.724 }, which gives ~3523 Pa at 300K.
                AntoineCoeffs = new double[] { 10.196, 1730.63, -39.724 },
                LatentHeatVaporization = 2.26e6,
                SpecificHeatCoeffs = new double[] { 4186 }
            };

            _gasWater = new Substance( "test_gas_water" )
            {
                Phase = SubstancePhase.Gas,
                MolarMass = WATER_MOLAR_MASS,
                // SpecificHeatCoeffs for Cp, Cv for steam is ~1480 J/kgK
                // R_specific is 461.5 J/kgK. Cp = Cv + R = 1480 + 461.5 = 1941.5
                SpecificHeatCoeffs = new double[] { 1941.5 }
            };

            SubstancePhaseMap.Clear();
            SubstancePhaseMap.RegisterPhasePartner( _lqWater, SubstancePhase.Gas, _gasWater );
            SubstancePhaseMap.RegisterPhasePartner( _gasWater, SubstancePhase.Liquid, _lqWater );
        }

        [TearDown]
        public void TearDown()
        {
            SubstancePhaseMap.Clear();
        }

        [Test]
        public void BoilingToVacuum_CausesEvaporationAndCooling()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            double initialTemp = 300;
            double initialMass = 10.0;

            // Tank with liquid, connected to a large vacuum tank
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero, _lqWater, initialMass );
            tankA.FluidState = new FluidState( 0, initialTemp, 0 ); // Set temperature first
            tankA.FluidState = new FluidState( tankA.Contents.GetPressureInVolume( tankA.Volume, tankA.FluidState ), initialTemp, 0 );
            double initialPressure = tankA.FluidState.Pressure;
            var tankB_vacuum = FlowNetworkTestHelper.CreateTestTank( 1000.0, Vector3.zero, new Vector3( 5, 0, 0 ) ); // Large, empty tank

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB_vacuum );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, Vector3.right, tankB_vacuum, new Vector3( 4, 0, 0 ), 1.0f );

            using var snapshot = builder.BuildSnapshot();

            // Act
            // Simulate for a few steps to allow phase change and flow to start
            for( int i = 0; i < 20; i++ )
            {
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );
            }

            // Assert
            double finalLiquidMass = tankA.Contents[_lqWater];
            double finalGasMass = tankA.Contents[_gasWater];
            double finalTemp = tankA.FluidState.Temperature;
            double finalPressure = tankA.FluidState.Pressure;

            Assert.That( finalLiquidMass, Is.LessThan( initialMass ), "Liquid mass should decrease due to boiling." );
            Assert.That( finalGasMass, Is.GreaterThan( 0 ), "Gas should be produced by boiling." );
            Assert.That( finalTemp, Is.LessThan( initialTemp ), "Temperature should drop due to latent heat of vaporization." );
            Assert.That( finalPressure, Is.GreaterThan( initialPressure ), "Pressure should rise as gas is produced." );
            // Check if it's approaching vapor pressure
            Assert.That( finalPressure, Is.EqualTo( _lqWater.GetVaporPressure( finalTemp ) ).Within( 25 ).Percent );
        }

        [Test]
        public void DrainingCryogenic_CausesContinuousCooling()
        {
            // Arrange: Define LOX and its gaseous partner GOX.
            var lqO2 = new Substance( "lq_o2" )
            {
                Phase = SubstancePhase.Liquid,
                MolarMass = 0.0319988,
                ReferenceDensity = 1141,
                BulkModulus = 0.95e9,
                // Antoine for O2, T in K, P in Pa: log10(P) = 8.99028 - 341.278 / (T - 6.133)
                AntoineCoeffs = new double[] { 8.99028, 341.278, -6.133 },
                LatentHeatVaporization = 2.13e5,
                SpecificHeatCoeffs = new double[] { 1660 }
            };
            var gasO2 = new Substance( "gas_o2" )
            {
                Phase = SubstancePhase.Gas,
                MolarMass = 0.0319988,
                SpecificHeatCoeffs = new double[] { 918 } // Cp
            };
            SubstancePhaseMap.RegisterPhasePartner( lqO2, SubstancePhase.Gas, gasO2 );
            SubstancePhaseMap.RegisterPhasePartner( gasO2, SubstancePhase.Liquid, lqO2 );

            double initialTemp = 100; // Above boiling point of LOX (~90K)
            var builder = new FlowNetworkBuilder();

            // Tank A is high, partially filled with cryo liquid, creating ullage space
            double loxMass = lqO2.ReferenceDensity * 0.8; // Fill half of the 1m^3 tank.
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 2, 0 ), lqO2, loxMass );
            tankA.FluidState = new FluidState( 0, initialTemp, 0 );
            tankA.FluidState = new FluidState( tankA.Contents.GetPressureInVolume( tankA.Volume, tankA.FluidState ), initialTemp, 0 );

            // Tank B is low, to drain into
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 1, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0f, 0.03f );
            using var snapshot = builder.BuildSnapshot();

            // Act & Assert
            double temp_t0 = tankA.FluidState.Temperature;

            // Simulate for a bit
            for( int i = 0; i < 20; i++ )
            {
                Debug.Log( tankA.Contents.GetMass() + " : " + tankA.FluidState );
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );
            }
            double temp_t1 = tankA.FluidState.Temperature;

            // Simulate some more
            for( int i = 0; i < 20; i++ )
            {
                Debug.Log( tankA.Contents.GetMass() + " : " + tankA.FluidState );
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );
            }
            double temp_t2 = tankA.FluidState.Temperature;

            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 0 ), "Mass should be draining from tank A." );
            Assert.That( temp_t1, Is.LessThan( temp_t0 ), "Temperature should drop after some time." );
            Assert.That( temp_t2, Is.LessThan( temp_t1 ), "Temperature should continue to drop as draining and boiling continue." );
        }
    }
}