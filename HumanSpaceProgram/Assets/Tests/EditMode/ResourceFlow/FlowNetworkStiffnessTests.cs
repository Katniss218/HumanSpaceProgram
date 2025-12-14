using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkStiffnessTests
    {
        private const double DT = 0.02;
        private static readonly Vector3 GRAVITY = new Vector3( 0, -9.81f, 0 );

        private Substance _lqO2;
        private Substance _gasO2;

        [SetUp]
        public void SetUp()
        {
            _lqO2 = new Substance( "lq_o2" )
            {
                Phase = SubstancePhase.Liquid,
                MolarMass = 0.0319988,
                ReferenceDensity = 1141,
                BulkModulus = 0.95e9,
                // Antoine Coeffs for O2, T in K, P in Pa: log10(P) = A - B / (T + C) -> P = 10^(A - B/(T+C))
                // For Oxygen, using values for bar, then converting to Pa:
                // log10(P_bar) = 3.9932 - 340.59 / (T - 6.33)
                // P_Pa = 100000 * 10^(...)
                // We must use coeffs that work with Pa directly. log10(P_Pa) = log10(P_bar * 100000) = log10(P_bar) + 5
                // So, A' = A + 5 = 8.9932
                AntoineCoeffs = new double[] { 8.9932, 340.59, -6.33 },
                LatentHeatVaporization = 2.13e5, // J/kg
                SpecificHeatCoeffs = new double[] { 1660 } // J/(kg*K)
            };
            _gasO2 = new Substance( "gas_o2" )
            {
                Phase = SubstancePhase.Gas,
                MolarMass = 0.0319988,
                SpecificHeatCoeffs = new double[] { 918 } // Cp for O2 gas
            };

            SubstancePhaseMap.Clear();
            SubstancePhaseMap.RegisterPhasePartner( _lqO2, SubstancePhase.Gas, _gasO2 );
            SubstancePhaseMap.RegisterPhasePartner( _gasO2, SubstancePhase.Liquid, _lqO2 );
        }

        [TearDown]
        public void TearDown()
        {
            SubstancePhaseMap.Clear();
        }

        [Test, Description( "Simulates draining a cryogenic tank to ensure the system remains stable without pressure spikes or temperature increases, validating the fix for stiff systems." )]
        public void CryogenicTankDrain_StaysStable_AndDoesNotOscillate()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            // A tank partially filled with LOX slightly above its boiling point (~90.2K at 1 atm)
            // This creates a highly volatile, stiff system.
            double initialTemp = 300; // K
            double tankVolume = 2.0; // m^3
            double initialLiquidMass = _lqO2.ReferenceDensity * (tankVolume * 0.9); // 90% full of liquid

            var tankA = FlowNetworkTestHelper.CreateTestTank( tankVolume, GRAVITY, new Vector3( 0, 10, 0 ), _lqO2, initialLiquidMass );
            // Initialize tank state and let it reach equilibrium before the test.
            tankA.FluidState = new FluidState( 101325, initialTemp, 0 );
            (var initialContents, var initialFState) = VaporLiquidEquilibrium.ComputeFlash( tankA.Contents, tankA.FluidState, tankA.Volume, 1.0, 1 );
            tankA.Contents = initialContents;
            tankA.FluidState = initialFState;

            // A lower, empty tank to drain into.
            var tankB = FlowNetworkTestHelper.CreateTestTank( tankVolume, GRAVITY, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 9, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0f, 0.01f );

            var snapshot = builder.BuildSnapshot();

            var pressureHistory = new List<double>();
            var temperatureHistory = new List<double>();
            double initialTotalMass = tankA.Contents.GetMass();

            // Act
            int steps = 500;
            for( int i = 0; i < steps; i++ )
            {
                // Run the full simulation step
                snapshot.Step( (float)DT );

                // Record state for analysis
                pressureHistory.Add( tankA.FluidState.Pressure );
                temperatureHistory.Add( tankA.FluidState.Temperature );
            }

            // Assert
            // 1. Mass Conservation
            double finalTotalMass = tankA.Contents.GetMass() + tankB.Contents.GetMass();
            Assert.That( finalTotalMass, Is.EqualTo( initialTotalMass ).Within( 1e-6 ), "Total mass must be conserved." );

            // 2. Flow occurred
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 1.0 ), "Mass should have drained into Tank B." );

            // 3. Pressure Stability: No wild spikes
            for( int i = 1; i < pressureHistory.Count; i++ )
            {
                double p_prev = pressureHistory[i - 1];
                double p_curr = pressureHistory[i];
                // Allow a reasonable fluctuation, but fail if pressure more than doubles in one step (indicative of a spike).
                if( p_prev > 1.0 ) // Avoid division by zero
                {
                    Assert.Less( p_curr / p_prev, 2.0, $"Pressure spiked unexpectedly at step {i}. From {p_prev:F0} Pa to {p_curr:F0} Pa." );
                }
            }

            // 4. Temperature Stability: Temperature should only decrease or stay the same due to boil-off.
            for( int i = 1; i < temperatureHistory.Count; i++ )
            {
                // Adding a small tolerance for floating point inaccuracies.
                Assert.LessOrEqual( temperatureHistory[i], temperatureHistory[i - 1] + 1e-5, $"Temperature incorrectly increased at step {i}. From {temperatureHistory[i - 1]:F4} K to {temperatureHistory[i]:F4} K." );
            }
        }
    }
}
