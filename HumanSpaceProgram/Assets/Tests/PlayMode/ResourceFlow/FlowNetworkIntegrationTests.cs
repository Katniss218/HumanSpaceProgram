using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using UnityEngine;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    [TestFixture]
    public class DirectFlowNetworkIntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TwoTanks_EqualizeLevels()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( TestSubstances.Water, 1000 ); // Full

            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            var ownerA = new object();
            var ownerB = new object();
            var ownerPipe = new object();

            builder.TryAddFlowObj( ownerA, tankA );
            builder.TryAddFlowObj( ownerB, tankB );

            var portA = new FlowPipe.Port( (IResourceConsumer)tankA, new Vector3( 0, -1, 0 ), 0.1f );
            var portB = new FlowPipe.Port( (IResourceConsumer)tankB, new Vector3( 0, -1, 0 ), 0.1f );
            var pipe = new FlowPipe( portA, portB, 1.0, 0.1f ); // Use length and area

            builder.TryAddFlowObj( ownerPipe, pipe );

            var snapshot = builder.BuildSnapshot();

            // Act
            float simulationTime = 10f;
            float fixedDeltaTime = 0.02f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( fixedDeltaTime );
                tankA.ApplySolveResults( fixedDeltaTime ); // Apply flows to update tank state
                tankB.ApplySolveResults( fixedDeltaTime );
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            Assert.That( massA, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank A should have half the mass." );
            Assert.That( massB, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank B should have half the mass." );
            Assert.That( massA + massB, Is.EqualTo( 1000.0 ).Within( 1e-9 ), "Mass should be conserved." );
        }
    }
}