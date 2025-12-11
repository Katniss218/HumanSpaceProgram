using HSP.ResourceFlow;
using NUnit.Framework;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowPipeTests
    {
        [Test]
        public void ComputeFlowRate___PositivePotentialDifference___FlowsForward()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 0.5;
            double potentialFrom = 100.0;
            double potentialTo = 50.0;

            // Act
            double flowRate = pipe.ComputeMassFlowRate( potentialFrom, potentialTo );

            // Assert
            Assert.That( flowRate, Is.GreaterThan( 0 ) );
            Assert.That( flowRate, Is.EqualTo( 0.5 * (100.0 - 50.0) ) );
        }

        [Test]
        public void ComputeFlowRate___NegativePotentialDifference___FlowsBackward()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 0.5;
            double potentialFrom = 50.0;
            double potentialTo = 100.0;

            // Act
            double flowRate = pipe.ComputeMassFlowRate( potentialFrom, potentialTo );

            // Assert
            Assert.That( flowRate, Is.LessThan( 0 ) );
            Assert.That( flowRate, Is.EqualTo( 0.5 * (50.0 - 100.0) ) );
        }

        [Test]
        public void ComputeFlowRate___ZeroPotentialDifference___NoFlow()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 0.5;
            double potentialFrom = 100.0;
            double potentialTo = 100.0;

            // Act
            double flowRate = pipe.ComputeMassFlowRate( potentialFrom, potentialTo );

            // Assert
            Assert.That( flowRate, Is.EqualTo( 0 ) );
        }

        [Test]
        public void ComputeFlowRate___WithPositiveHeadAdded___IncreasesForwardFlow()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 0.5;
            pipe.HeadAdded = 20.0;
            double potentialFrom = 100.0;
            double potentialTo = 100.0; // No natural flow

            // Act
            double flowRate = pipe.ComputeMassFlowRate( potentialFrom, potentialTo );

            // Assert
            Assert.That( flowRate, Is.GreaterThan( 0 ) );
            Assert.That( flowRate, Is.EqualTo( 0.5 * (100.0 - 100.0 + 20.0) ) );
        }

        [Test]
        public void ComputeFlowRate___WithNegativeHeadAdded___InducesBackwardFlow()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 0.5;
            pipe.HeadAdded = -30.0;
            double potentialFrom = 100.0;
            double potentialTo = 100.0; // No natural flow

            // Act
            double flowRate = pipe.ComputeMassFlowRate( potentialFrom, potentialTo );

            // Assert
            Assert.That( flowRate, Is.LessThan( 0 ) );
            Assert.That( flowRate, Is.EqualTo( 0.5 * (100.0 - 100.0 - 30.0) ) );
        }
    }
}
