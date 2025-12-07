using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class MultiManifoldEngineIntegrationTests
    {
        private const double DT = 0.02;

        private static FlowPipe CreateAndAddPipe( FlowNetworkBuilder builder, IResourceConsumer from, Vector3 fromLocation, IResourceConsumer to, Vector3 toLocation, double conductance )
        {
            var portA = new FlowPipe.Port( from, fromLocation, 0.1f );
            var portB = new FlowPipe.Port( to, toLocation, 0.1f );
            var pipe = new FlowPipe( portA, portB, conductance );
            builder.TryAddFlowObj( new object(), pipe );
            return pipe;
        }

        [Test]
        public void EngineWithTwoInlets_BothFedCorrectly_ConsumesCorrectRatio()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -9.81f, 0 );
            double fuelMass = 800;
            double loxMass = 1141;
            double mixtureRatio = loxMass / fuelMass; // LOX / Fuel

            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, fuelMass );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Lox, loxMass );
            var fuelFeed = new EngineFeedSystem( 0.01 );
            var loxFeed = new EngineFeedSystem( 0.01 );

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            CreateAndAddPipe( builder, fuelTank, new Vector3( 0, 99, 0 ), fuelFeed, new Vector3( 0, 1, 0 ), 1.0 );
            CreateAndAddPipe( builder, loxTank, new Vector3( 0, 99, 0 ), loxFeed, new Vector3( 0, 1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            double totalFuelConsumed = 0;
            double totalLoxConsumed = 0;

            // Simulate engine parameters
            const double targetTotalMassFlow = 150.0; // kg/s
            const double chamberPressureToMassFlowRatio = 50e5 / 150.0; // Pa / (kg/s)
            const double NominalPressureDelta = 1e6; // 10 bar, from FRocketEngine

            for( int i = 0; i < 100; i++ ) // Simulate for 2 seconds
            {
                // --- Drive the EngineFeedSystems (simulating FRocketEngine's logic) ---
                fuelFeed.IsOutflowEnabled = loxFeed.IsOutflowEnabled = true;

                // Demand is based on target total flow and mixture ratio
                double fuelMassDemand = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassDemand = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));
                fuelFeed.Demand = fuelMassDemand / TestSubstances.Kerosene.ReferenceDensity; // Volumetric demand
                loxFeed.Demand = loxMassDemand / TestSubstances.Lox.ReferenceDensity;

                // Chamber pressure is an emergent property of the *last* step's consumption
                double totalMassConsumedLastStep = fuelFeed.MassConsumedLastStep + loxFeed.MassConsumedLastStep;
                double currentChamberPressure;
                if( totalMassConsumedLastStep > 1e-6 )
                {
                    currentChamberPressure = (totalMassConsumedLastStep / DT) * chamberPressureToMassFlowRatio;
                }
                else
                {
                    currentChamberPressure = 1e5; // Initial priming pressure
                }
                fuelFeed.ChamberPressure = currentChamberPressure;
                loxFeed.ChamberPressure = currentChamberPressure;

                // Injector conductance is a physical property, calculated to achieve the target mixture ratio at a nominal pressure drop.
                double fuelMassFlowShare = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassFlowShare = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));

                fuelFeed.InjectorConductance = fuelMassFlowShare / Math.Sqrt( NominalPressureDelta );
                loxFeed.InjectorConductance = loxMassFlowShare / Math.Sqrt( NominalPressureDelta );

                // Use a high pump pressure to ensure manifolds stay pressurized
                fuelFeed.PumpPressureRise = loxFeed.PumpPressureRise = 60e5;

                // --- Simulate ---
                snapshot.Step( (float)DT );

                // Manually apply consumption from feed system
                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.MassConsumedLastStep;
                totalLoxConsumed += loxFeed.MassConsumedLastStep;
            }

            // Assert
            Assert.Greater( totalFuelConsumed, 0, "Fuel should have been consumed." );
            Assert.Greater( totalLoxConsumed, 0, "LOX should have been consumed." );
            Assert.Less( fuelTank.Contents.GetMass(), fuelMass, "Fuel tank should be draining." );
            Assert.Less( loxTank.Contents.GetMass(), loxMass, "LOX tank should be draining." );

            double consumedRatio = totalLoxConsumed / totalFuelConsumed;
            Assert.That( consumedRatio, Is.EqualTo( mixtureRatio ).Within( 10 ).Percent, "Consumed propellant ratio should be close to ideal mixture ratio." );
        }

        [Test]
        public void EngineWithTwoInlets_OneLineStarved_ConsumesAlmostNothing()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -9.81f, 0 );
            double fuelMass = 800;
            double mixtureRatio = 1141.0 / 800.0;

            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, fuelMass );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Lox, 0 ); // Empty LOX tank
            var fuelFeed = new EngineFeedSystem( 0.01 );
            var loxFeed = new EngineFeedSystem( 0.01 );

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            CreateAndAddPipe( builder, fuelTank, new Vector3( 0, 99, 0 ), fuelFeed, new Vector3( 0, 1, 0 ), 1.0 );
            CreateAndAddPipe( builder, loxTank, new Vector3( 0, 99, 0 ), loxFeed, new Vector3( 0, 1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            double totalFuelConsumed = 0;
            double totalLoxConsumed = 0;

            const double targetTotalMassFlow = 150.0;
            const double chamberPressureToMassFlowRatio = 50e5 / 150.0;
            const double NominalPressureDelta = 1e6; // 10 bar, from FRocketEngine

            for( int i = 0; i < 50; i++ ) // Simulate for 1 second
            {
                // --- Drive the EngineFeedSystems (simulating FRocketEngine's logic) ---
                fuelFeed.IsOutflowEnabled = loxFeed.IsOutflowEnabled = true;

                // Demand is based on target total flow and mixture ratio
                double fuelMassDemand = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassDemand = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));
                fuelFeed.Demand = fuelMassDemand / TestSubstances.Kerosene.ReferenceDensity; // Volumetric demand
                loxFeed.Demand = loxMassDemand / TestSubstances.Lox.ReferenceDensity;

                // Chamber pressure is an emergent property of the *last* step's consumption
                double totalMassConsumedLastStep = fuelFeed.MassConsumedLastStep + loxFeed.MassConsumedLastStep;
                double currentChamberPressure;
                if( totalMassConsumedLastStep > 1e-6 )
                {
                    currentChamberPressure = (totalMassConsumedLastStep / DT) * chamberPressureToMassFlowRatio;
                }
                else
                {
                    currentChamberPressure = 1e5; // Initial priming pressure
                }
                fuelFeed.ChamberPressure = currentChamberPressure;
                loxFeed.ChamberPressure = currentChamberPressure;

                // Injector conductance is a physical property, calculated to achieve the target mixture ratio at a nominal pressure drop.
                double fuelMassFlowShare = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassFlowShare = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));

                fuelFeed.InjectorConductance = fuelMassFlowShare / Math.Sqrt( NominalPressureDelta );
                loxFeed.InjectorConductance = loxMassFlowShare / Math.Sqrt( NominalPressureDelta );

                // Use a high pump pressure to ensure manifolds stay pressurized
                fuelFeed.PumpPressureRise = loxFeed.PumpPressureRise = 60e5;

                // --- Simulate ---
                snapshot.Step( (float)DT );

                // Manually apply consumption from feed system
                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.MassConsumedLastStep;
                totalLoxConsumed += loxFeed.MassConsumedLastStep;
            }

            // Assert
            Assert.That( totalLoxConsumed, Is.EqualTo( 0.0 ), "No LOX should have been consumed from an empty tank." );
            // A small amount of fuel might be consumed to prime the manifold initially, but it should be very little.
            Assert.That( totalFuelConsumed, Is.LessThan( 1.0 ), "Fuel consumption should be negligible when the other propellant is missing." );
            Assert.That( fuelTank.Contents.GetMass(), Is.EqualTo( fuelMass ).Within( 1.0 ), "Fuel tank should not have drained significantly." );
        }

        [Test]
        public void EngineWithTwoInlets_RestrictedLine_FlowsAtLowerRate()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -9.81f, 0 );
            double fuelMass = 800;
            double loxMass = 1141;
            double mixtureRatio = loxMass / fuelMass;

            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 10, 0 ), TestSubstances.Kerosene, fuelMass );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 10, 0 ), TestSubstances.Lox, loxMass );
            var fuelFeed = new EngineFeedSystem( 0.01 );
            var loxFeed = new EngineFeedSystem( 0.01 );

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            CreateAndAddPipe( builder, fuelTank, new Vector3( 0, 9, 0 ), fuelFeed, new Vector3( 0, 1, 0 ), 10 );
            CreateAndAddPipe( builder, loxTank, new Vector3( 0, 9, 0 ), loxFeed, new Vector3( 0, 1, 0 ), 0.0001 );

            var snapshot = builder.BuildSnapshot();

            // Act
            double totalFuelConsumed = 0;
            double totalLoxConsumed = 0;

            const double targetTotalMassFlow = 150.0;
            const double chamberPressureToMassFlowRatio = 50e5 / 150.0;
            const double NominalPressureDelta = 1e6; // 10 bar, from FRocketEngine

            for( int i = 0; i < 100; i++ )
            {
                // --- Drive the EngineFeedSystems (simulating FRocketEngine's logic) ---
                fuelFeed.IsOutflowEnabled = loxFeed.IsOutflowEnabled = true;

                // Demand is based on target total flow and mixture ratio
                double fuelMassDemand = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassDemand = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));
                fuelFeed.Demand = fuelMassDemand / TestSubstances.Kerosene.ReferenceDensity; // Volumetric demand
                loxFeed.Demand = loxMassDemand / TestSubstances.Lox.ReferenceDensity;

                // Chamber pressure is an emergent property of the *last* step's consumption
                double totalMassConsumedLastStep = fuelFeed.MassConsumedLastStep + loxFeed.MassConsumedLastStep;
                double currentChamberPressure;
                if( totalMassConsumedLastStep > 1e-6 )
                {
                    currentChamberPressure = (totalMassConsumedLastStep / DT) * chamberPressureToMassFlowRatio;
                }
                else
                {
                    currentChamberPressure = 1e5; // Initial priming pressure
                }
                fuelFeed.ChamberPressure = currentChamberPressure;
                loxFeed.ChamberPressure = currentChamberPressure;

                // Injector conductance is a physical property, calculated to achieve the target mixture ratio at a nominal pressure drop.
                double fuelMassFlowShare = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassFlowShare = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));

                fuelFeed.InjectorConductance = fuelMassFlowShare / Math.Sqrt( NominalPressureDelta );
                loxFeed.InjectorConductance = loxMassFlowShare / Math.Sqrt( NominalPressureDelta );

                // Use a high pump pressure to ensure manifolds stay pressurized
                fuelFeed.PumpPressureRise = loxFeed.PumpPressureRise = 6e4;

                // --- Simulate ---
                snapshot.Step( (float)DT );

                // Manually apply consumption from feed system
                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.MassConsumedLastStep;
                totalLoxConsumed += loxFeed.MassConsumedLastStep;
            }

            Debug.Log( totalFuelConsumed + " : " + totalLoxConsumed );
            // Assert
            Assert.Greater( totalFuelConsumed, 0, "Fuel should have flowed." );
            Assert.Greater( totalLoxConsumed, 0, "Some LOX should have flowed, even if restricted." );
            Assert.Greater( totalFuelConsumed, totalLoxConsumed, "Fuel flow should be significantly higher than the restricted LOX flow." );
        }
    }
}