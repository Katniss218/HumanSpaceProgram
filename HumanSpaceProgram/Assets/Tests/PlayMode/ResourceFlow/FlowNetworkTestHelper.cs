using HSP.ResourceFlow;
using HSP.Time;
using UnityEngine;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    public static class FlowNetworkTestHelper
    {
        public static (GameObject manager, TimeManager timeManager, AssertMonoBehaviour assertMonoBeh) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );
            var assertMonoBeh = manager.AddComponent<AssertMonoBehaviour>();

            return (manager, timeManager, assertMonoBeh);
        }
    }

    public sealed class MockFlowTankWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public FlowTank Tank { get; set; }
        public ResourceInlet[] Inlets { get; set; }

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            c.TryAddFlowObj( this, Tank );
            foreach( var inlet in Inlets )
            {
                FlowPipe.Port flowInlet = new FlowPipe.Port( (IResourceConsumer)Tank, inlet.LocalPosition );
                c.TryAddFlowObj( inlet, flowInlet );
            }

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            return false;
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            if( !Tank.Outflow.IsEmpty() )
            {
                Tank.Contents.Add( Tank.Outflow, -snapshot.deltaTime );
            }
            if( !Tank.Inflow.IsEmpty() )
            {
                Tank.Contents.Add( Tank.Inflow, snapshot.deltaTime );
            }
            Tank.InvalidateFluids();
        }
    }

    public sealed class MockFlowPipeWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea { get; set; } = 0.1f;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null )
                return BuildFlowResult.Finished;
            if( ToInlet == null )
                return BuildFlowResult.Finished;

            // only add if valve open, etc.

            // inlets are part of the tank which is built by the builder.
            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea, conductance: 1 );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            return false;
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
        }
    }

    /*
    public class MockFlowTankWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public FlowTank Tank { get; }
        public Transform Transform { get; }

        public Vector3 FluidAcceleration { get => Tank.FluidAcceleration; set => Tank.FluidAcceleration = value; }
        public Vector3 FluidAngularVelocity { get => Tank.FluidAngularVelocity; set => Tank.FluidAngularVelocity = value; }
        public ISubstanceStateCollection Inflow { get => Tank.Inflow; set => Tank.Inflow = value; }
        public ISubstanceStateCollection Outflow { get => Tank.Outflow; set => Tank.Outflow = value; }

        public FluidState Sample( Vector3 position, double holeArea )
        {
            Vector3 localPosition = Transform.InverseTransformPoint( position );
            return Tank.Sample( localPosition, holeArea );
        }

        public IReadonlySubstanceStateCollection SampleSubstances( Vector3 position, double flowRate, double dt )
        {
            Vector3 localPosition = Transform.InverseTransformPoint( position );
            return Tank.SampleSubstances( localPosition, flowRate, dt );
        }
    }

    public class TestNetworkBuilderComponent : MonoBehaviour, IBuildsFlowNetwork
    {
        public List<MockFlowTankWrapper> Wrappers = new();
        public List<(int fromTank, int fromPort, int toTank, int toPort, double conductance, double headAdded)> Pipes = new();

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder builder )
        {
            foreach( var wrapper in Wrappers )
            {
                builder.TryAddFlowObj( wrapper, wrapper.Tank );
            }

            foreach( var p in Pipes )
            {
                var fromWrapper = Wrappers[p.fromTank];
                var toWrapper = Wrappers[p.toTank];

                var fromPortLocalPos = fromWrapper.Tank.InletNodes.Keys.ToList()[p.fromPort].pos;
                var toPortLocalPos = toWrapper.Tank.InletNodes.Keys.ToList()[p.toPort].pos;

                var fromPortWorldPos = fromWrapper.Transform.TransformPoint( fromPortLocalPos );
                var toPortWorldPos = toWrapper.Transform.TransformPoint( toPortLocalPos );

                var portFrom = new FlowPipe.Port( (IResourceConsumer)fromWrapper, fromPortWorldPos );
                var portTo = new FlowPipe.Port( (IResourceConsumer)toWrapper, toPortWorldPos );
                var pipe = new FlowPipe( portFrom, portTo, 0.1, p.conductance, p.headAdded );

                builder.TryAddFlowObj( this, pipe );
            }
            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot ) => true; // Keep it simple for tests
        public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
    }*/
}
