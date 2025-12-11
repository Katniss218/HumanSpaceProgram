using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System.Linq;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class MultiInletConsumerTests
    {
        private const double DT = 0.02;

        private static FlowPipe CreateAndAddPipe( FlowNetworkBuilder builder, IResourceConsumer from, Vector3 fromLocation, IResourceConsumer to, Vector3 toLocation, double length, float area = 0.1f )
        {
            var portA = new FlowPipe.Port( from, fromLocation, area );
            var portB = new FlowPipe.Port( to, toLocation, area );
            var pipe = new FlowPipe( portA, portB, length, area );
            builder.TryAddFlowObj( new object(), pipe );
            return pipe;
        }

        [Test, Description( "Verifies that when two identical pipes feed two identical consumers, they draw roughly equal mass from their respective tanks." )]
        public void TwoConsumers_BothFedCorrectly_ConsumeRoughlyEqualMass()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = Vector3.zero;
            double fuelMass = 800;
            double loxMass = 1141;

            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, fuelMass );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Lox, loxMass );
            var fuelFeed = new EngineFeedSystem();
            var loxFeed = new EngineFeedSystem();

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

            for( int i = 0; i < 100; i++ )
            {
                fuelFeed.TargetPressure = 10e5;
                loxFeed.TargetPressure = 10e5;
                fuelFeed.ExpectedDensity = TestSubstances.Kerosene.GetDensityAtSTP();
                loxFeed.ExpectedDensity = TestSubstances.Lox.GetDensityAtSTP();

                snapshot.Step( (float)DT );

                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );
                fuelTank.ApplyFlows( DT );
                loxTank.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.ActualMassFlow_LastStep * DT;
                totalLoxConsumed += loxFeed.ActualMassFlow_LastStep * DT;
            }

            // Assert
            Assert.Greater( totalFuelConsumed, 0, "Fuel should have been consumed." );
            Assert.Greater( totalLoxConsumed, 0, "LOX should have been consumed." );
            Assert.Less( fuelTank.Contents.GetMass(), fuelMass, "Fuel tank should be draining." );
            Assert.Less( loxTank.Contents.GetMass(), loxMass, "LOX tank should be draining." );

            // Ratio of consumed mass should be roughly related to the ratio of their densities and pipe conductances.
            // Since everything is symmetrical, they should consume similar amounts of mass.
            Assert.That( totalLoxConsumed / totalFuelConsumed, Is.EqualTo( 1.0 ).Within( 20 ).Percent, "Consumed mass ratio should be close to 1:1 for symmetrical setup." );
        }

        [Test, Description( "Verifies that if one propellant line is starved (empty tank), the solver correctly provides zero flow for that line while still allowing flow from the other." )]
        public void TwoConsumers_OneLineStarved_FlowsOnlyFromFullTank()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, 800 );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 0, 100, 0 ) ); // Empty LOX tank
            var fuelFeed = new EngineFeedSystem();
            var loxFeed = new EngineFeedSystem();

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            CreateAndAddPipe( builder, fuelTank, Vector3.zero, fuelFeed, Vector3.zero, 1.0 );
            CreateAndAddPipe( builder, loxTank, Vector3.zero, loxFeed, Vector3.zero, 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            double totalFuelConsumed = 0;
            double totalLoxConsumed = 0;

            for( int i = 0; i < 50; i++ )
            {
                fuelFeed.TargetPressure = 10e5;
                loxFeed.TargetPressure = 10e5;
                fuelFeed.ExpectedDensity = TestSubstances.Kerosene.GetDensityAtSTP();
                loxFeed.ExpectedDensity = TestSubstances.Lox.GetDensityAtSTP();

                snapshot.Step( (float)DT );

                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );
                fuelTank.ApplyFlows( DT );
                loxTank.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.ActualMassFlow_LastStep * DT;
                totalLoxConsumed += loxFeed.ActualMassFlow_LastStep * DT;
            }

            // Assert
            Assert.That( totalLoxConsumed, Is.EqualTo( 0.0 ), "No LOX should have been consumed from an empty tank." );
            Assert.That( totalFuelConsumed, Is.GreaterThan( 0.0 ), "Fuel should still flow even if the other line is starved." );
            Assert.That( fuelTank.Contents.GetMass(), Is.LessThan( 800 ), "Fuel tank should have drained." );
        }

        [Test, Description( "Verifies that a restriction in one pipe (high length) results in a significantly lower mass flow rate through that line compared to an unrestricted line." )]
        public void TwoConsumers_OneLineRestricted_FlowsAtLowerRate()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, 800 );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 0, 100, 0 ), TestSubstances.Lox, 1141 );
            var fuelFeed = new EngineFeedSystem();
            var loxFeed = new EngineFeedSystem();

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            CreateAndAddPipe( builder, fuelTank, Vector3.zero, fuelFeed, Vector3.zero, 1.0 );     // Normal fuel pipe
            CreateAndAddPipe( builder, loxTank, Vector3.zero, loxFeed, Vector3.zero, 1000.0 ); // Very long, restricted LOX pipe

            var snapshot = builder.BuildSnapshot();

            // Act
            double totalFuelConsumed = 0;
            double totalLoxConsumed = 0;

            for( int i = 0; i < 100; i++ )
            {
                fuelFeed.TargetPressure = 10e5;
                loxFeed.TargetPressure = 10e5;
                fuelFeed.ExpectedDensity = TestSubstances.Kerosene.GetDensityAtSTP();
                loxFeed.ExpectedDensity = TestSubstances.Lox.GetDensityAtSTP();

                snapshot.Step( (float)DT );

                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );
                fuelTank.ApplyFlows( DT );
                loxTank.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.ActualMassFlow_LastStep * DT;
                totalLoxConsumed += loxFeed.ActualMassFlow_LastStep * DT;
            }

            // Assert
            Assert.Greater( totalFuelConsumed, 0, "Fuel should have flowed." );
            Assert.Greater( totalLoxConsumed, 0, "Some LOX should have flowed, even if restricted." );
            Assert.Greater( totalFuelConsumed, totalLoxConsumed * 10, "Fuel flow should be significantly higher than the restricted LOX flow." );
        }
    }
}