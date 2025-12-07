using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class EngineFeedSystemTests
    {
        private EngineFeedSystem _feedSystem;

        private const double MANIFOLD_VOLUME = 0.01; // 10 liters.

        [SetUp]
        public void SetUp()
        {
            _feedSystem = new EngineFeedSystem( MANIFOLD_VOLUME );
        }

        [Test]
        public void Sample___InternalPressure_AndMass___IsCorrect()
        {
            // Arrange
            const double targetManifoldPressure = 2e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            // Act
            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();

            // Assert
            Assert.That( _feedSystem.ManifoldPressure, Is.EqualTo( targetManifoldPressure ).Within( 1e-9 ) );
            Assert.That( _feedSystem.Contents.GetMass(), Is.EqualTo( requiredMass ).Within( 1e-9 ) );
        }

        [Test]
        public void Sample___InternalPressure___CreatesBackPressure()
        {
            // Arrange
            const double targetManifoldPressure = 2e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            // Act
            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();

            _feedSystem.IsOutflowEnabled = true;
            _feedSystem.PumpPressureRise = 0;
            _feedSystem.ChamberPressure = 50e5; // 50 bar
            var state = _feedSystem.Sample( Vector3.zero, 0.01 );

            // Assert
            // Inlet pressure = Manifold (2e5) = 2e5 Pa
            // Potential = P / rho
            double inletPressure = targetManifoldPressure;
            double densityAtInletPressure = _feedSystem.Contents.GetAverageDensity( FluidState.STP.Temperature, inletPressure );
            double expectedPotential = inletPressure / densityAtInletPressure;
            Assert.That( state.FluidSurfacePotential, Is.EqualTo( expectedPotential ).Within( 1e-9 ) );
        }

        [Test]
        public void Sample___Empty___BackPressureIsZero()
        {
            _feedSystem.IsOutflowEnabled = false;
            var state = _feedSystem.Sample( Vector3.zero, 0.01 );
            Assert.That( state.FluidSurfacePotential, Is.EqualTo( 0.0 ) );
        }

        [Test]
        public void Sample___InternalPressure_Disabled___CreatesBackPressure()
        {
            // Arrange
            const double targetManifoldPressure = 2e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            // Act
            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();
            var state = _feedSystem.Sample( Vector3.zero, 0.01 );

            // Assert
            // Inlet pressure = Manifold (2e5) = 2e5 Pa
            // Potential = P / rho
            double inletPressure = targetManifoldPressure;
            double densityAtInletPressure = _feedSystem.Contents.GetAverageDensity( FluidState.STP.Temperature, inletPressure );
            double expectedPotential = inletPressure / densityAtInletPressure;
            Assert.That( state.FluidSurfacePotential, Is.EqualTo( expectedPotential ).Within( 1e-9 ) );
        }

        [Test]
        public void Sample___InternalPressure_AndPump___CreatesBackPressure()
        {
            // Arrange
            const double targetManifoldPressure = 2e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            // Act
            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();

            _feedSystem.IsOutflowEnabled = true;
            _feedSystem.PumpPressureRise = 1e5; // 1 bar pump
            _feedSystem.ChamberPressure = 50e5; // 50 bar
            var state = _feedSystem.Sample( Vector3.zero, 0.01 );

            // Assert
            // Inlet pressure = Manifold (2e5) - Pump (1e5) = 1e5 Pa
            // Potential = P / rho
            double inletPressure = targetManifoldPressure - _feedSystem.PumpPressureRise;
            double densityAtInletPressure = _feedSystem.Contents.GetAverageDensity( FluidState.STP.Temperature, inletPressure );
            double expectedPotential = inletPressure / densityAtInletPressure;
            Assert.That( state.FluidSurfacePotential, Is.EqualTo( expectedPotential ).Within( 1e-9 ) );
        }

        [Test]
        public void ApplyFlows___WithInflowAndNoOutflow___PressureIncreases()
        {
            // Arrange
            _feedSystem.IsOutflowEnabled = false;
            double inflowMass = 8.0;
            _feedSystem.Inflow.Add( TestSubstances.Air, inflowMass );

            // Act
            _feedSystem.ApplyFlows( 0.02 );

            // Assert
            Assert.That( _feedSystem.MassConsumedLastStep, Is.EqualTo( 0 ) );

            double expectedPressure = TestSubstances.Air.GetPressure( inflowMass, MANIFOLD_VOLUME, FluidState.STP.Temperature );

            Assert.That( _feedSystem.ManifoldPressure, Is.EqualTo( expectedPressure ).Within( 1.0 ) );
        }

        [Test]
        public void ApplyFlows___WithInsufficientPressure___ConsumesNothing()
        {
            // Arrange: Prime manifold to a pressure BELOW chamber pressure
            const double targetManifoldPressure = 40e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();

            Assert.That( _feedSystem.ManifoldPressure, Is.EqualTo( targetManifoldPressure ).Within( 1.0 ), "Manifold did not prime correctly." );

            _feedSystem.IsOutflowEnabled = true;
            _feedSystem.ChamberPressure = 50e5; // 50 bar

            // Act
            _feedSystem.ApplyFlows( 0.02 );

            // Assert
            Assert.That( _feedSystem.MassConsumedLastStep, Is.EqualTo( 0.0 ) );
        }

        [Test]
        public void ApplyFlows___WithSufficientPressure_Unclamped___ConsumesExpectedMass()
        {
            // Arrange: Prime manifold to a pressure BELOW chamber pressure
            const double targetManifoldPressure = 60e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();

            Assert.That( _feedSystem.ManifoldPressure, Is.EqualTo( targetManifoldPressure ).Within( 1.0 ), "Manifold did not prime correctly." );

            _feedSystem.IsOutflowEnabled = true;
            _feedSystem.ChamberPressure = 50e5; // 50 bar
            _feedSystem.InjectorConductance = 0.0005; // Low conductance so the outflow is not clamped by the available mass.

            // Act
            _feedSystem.ApplyFlows( 0.02 );

            // Assert
            double pressureDropToChamber = targetManifoldPressure - _feedSystem.ChamberPressure;
            double expectedMassFlow = _feedSystem.InjectorConductance * Math.Sqrt( pressureDropToChamber );
            double expectedConsumption = expectedMassFlow * 0.02;
            Assert.That( _feedSystem.MassConsumedLastStep, Is.EqualTo( expectedConsumption ).Within( 1e-9 ) );
        }

        [Test]
        public void ApplyFlows___WithSufficientPressure_Clamped___ConsumesAvailableMass()
        {
            // Arrange: High pressure gradient but very little mass
            const double targetManifoldPressure = 60e5;
            double requiredMass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, targetManifoldPressure, FluidState.STP.Temperature );
            _feedSystem.IsOutflowEnabled = false;

            _feedSystem.Inflow.Add( TestSubstances.Air, requiredMass );
            _feedSystem.ApplyFlows( 0.01 );
            _feedSystem.Inflow.Clear();

            Assert.That( _feedSystem.ManifoldPressure, Is.EqualTo( targetManifoldPressure ).Within( 1.0 ), "Manifold did not prime correctly." );
            double availableMass = _feedSystem.Contents.GetMass();

            _feedSystem.IsOutflowEnabled = true;
            _feedSystem.ChamberPressure = 1e5;
            _feedSystem.InjectorConductance = 1.0; // High conductance to demand a lot of mass

            // Act
            _feedSystem.ApplyFlows( 0.02 );

            // Assert
            double theoreticalConsumption = _feedSystem.InjectorConductance * Math.Sqrt( targetManifoldPressure - _feedSystem.ChamberPressure ) * 0.02;
            Assert.That( requiredMass, Is.EqualTo( availableMass ) );
            Assert.That( theoreticalConsumption, Is.GreaterThan( availableMass ) );
            Assert.That( _feedSystem.MassConsumedLastStep, Is.EqualTo( availableMass ).Within( 1e-9 ) );
        }



        [Test]
        public void Simulation___ConstantInflow___ReachesEquilibrium()
        {
            // Arrange
            const double dt = 0.02;
            const double chamberPressure = 1e6;
            const double conductance = 0.1;
            const double inflowRate = 1.5; // kg/s constant inflow

            _feedSystem.IsOutflowEnabled = true;
            _feedSystem.ChamberPressure = chamberPressure;
            _feedSystem.InjectorConductance = conductance;

            // Act: Run until roughly stabilized (mass in ~ mass out)
            // Ideally, Pressure stabilizes when: Inflow = Conductance * Sqrt(P_man - P_chamb)
            // Therefore: (Inflow/C)^2 + P_chamb = P_man
            double theoreticalEquilibriumPressure = Math.Pow( inflowRate / conductance, 2 ) + chamberPressure;

            int ticks = 0;
            const int maxTicks = 1000;
            bool reachedEquilibrium = false;

            while( ticks < maxTicks )
            {
                // Add constant mass every tick
                _feedSystem.Inflow.Add( TestSubstances.Air, inflowRate * dt );
                _feedSystem.ApplyFlows( dt );
                _feedSystem.Inflow.Clear(); // Clear buffer for next tick

                // Check if outflow matches inflow
                double outflowRate = _feedSystem.MassConsumedLastStep / dt;
                Debug.Log( ticks + " Outflow Rate: " + outflowRate + " Inflow Rate: " + inflowRate );

                if( Math.Abs( outflowRate - inflowRate ) < 0.01 )
                {
                    reachedEquilibrium = true;
                    break;
                }
                ticks++;
            }

            // Assert
            Assert.That( reachedEquilibrium, Is.True, "System did not reach equilibrium within time limit." );
            Assert.That( _feedSystem.ManifoldPressure, Is.EqualTo( theoreticalEquilibriumPressure ).Within( 100.0 ),
                "Equilibrium pressure does not match theoretical calculation." );
        }


        [Test]
        public void Simulation___PumpSpinUp___IncreasesInletPotentialOverTime()
        {
            // Arrange
            const double dt = 0.02;
            const double staticPressure = 2e5;

            // Fill with static amount of air
            double mass = TestSubstances.Air.GetMass( MANIFOLD_VOLUME, staticPressure, FluidState.STP.Temperature );
            _feedSystem.Inflow.Add( TestSubstances.Air, mass );
            _feedSystem.ApplyFlows( 0 );
            _feedSystem.Inflow.Clear();
            _feedSystem.IsOutflowEnabled = false; // Closed system, just testing pump potential

            // Act
            double targetPumpPressure = 50e5; // 50 bar
            double currentPumpPressure = 0;

            for( int i = 0; i < 20; i++ )
            {
                // Ramp up pump
                currentPumpPressure += (targetPumpPressure / 20.0);
                _feedSystem.PumpPressureRise = currentPumpPressure;

                // Sample
                var state = _feedSystem.Sample( Vector3.zero, dt );

                // Assert
                // Remember: Sample() returns the "Suction" condition (Manifold P - Pump P)
                // or the "Discharge" condition? 
                // Based on previous tests: Inlet P = Manifold - PumpPressure.
                double expectedEffectivePressure = staticPressure - currentPumpPressure;

                // If pump is stronger than manifold pressure, we might get negative effective pressure 
                // (suction) depending on implementation. Assuming linear subtraction here based on your previous tests.

                double density = _feedSystem.Contents.GetAverageDensity( FluidState.STP.Temperature, expectedEffectivePressure );
                double expectedPotential = expectedEffectivePressure / density;

                Assert.That( state.FluidSurfacePotential, Is.EqualTo( expectedPotential ).Within( 1e-9 ),
                    $"Tick {i}: Fluid potential did not track pump pressure rise." );
            }
        }
    }
}
