using HSP.ResourceFlow;
using NUnit.Framework;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class PipeModifierTests
    {
        [Test]
        public void PumpModifier_Apply_AddsHeadToPipe()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.HeadAdded = 10.0; // Initial head
            var pump = new PumpModifier { HeadAdded = 50.0 };

            // Act
            pump.Apply( pipe );

            // Assert
            // PumpModifier uses +=
            Assert.That( pipe.HeadAdded, Is.EqualTo( 10.0 + 50.0 ) );
        }

        [Test]
        public void ValveModifier_Apply_ScalesPipeConductance()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 200.0;
            var valve = new ValveModifier { PercentOpen = 0.25 }; // 25% open

            // Act
            valve.Apply( pipe );

            // Assert
            // ValveModifier uses *=
            Assert.That( pipe.MassFlowConductance, Is.EqualTo( 200.0 * 0.25 ) );
        }

        [Test]
        public void MultipleModifiers_Apply_EffectsAreCumulative()
        {
            // Arrange
            var pipe = new FlowPipe( default, default, 1.0, 0.1 );
            pipe.MassFlowConductance = 100.0;
            // Assume head is reset to 0 before modifiers are applied, as per FResourceConnection_FlowPipe.SynchronizeState.
            pipe.HeadAdded = 0.0;

            var valve = new ValveModifier { PercentOpen = 0.5 };
            var pump1 = new PumpModifier { HeadAdded = 20.0 };
            var pump2 = new PumpModifier { HeadAdded = 30.0 };

            // Act
            valve.Apply( pipe );
            pump1.Apply( pipe );
            pump2.Apply( pipe );

            // Assert
            Assert.That( pipe.MassFlowConductance, Is.EqualTo( 100.0 * 0.5 ), "Valve should scale conductance." ); // 50.0
            Assert.That( pipe.HeadAdded, Is.EqualTo( 0.0 + 20.0 + 30.0 ), "Pump heads should be additive." ); // 50.0
        }
    }
}
