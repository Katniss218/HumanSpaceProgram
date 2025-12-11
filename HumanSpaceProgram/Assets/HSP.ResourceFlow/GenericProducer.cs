using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericProducer : IResourceProducer
    {
        public Vector3 FluidAcceleration { get; set; }
        public Vector3 FluidAngularVelocity { get; set; }
        public ISubstanceStateCollection Outflow { get; set; } = new SubstanceStateCollection();

        public void ApplyFlows( double deltaTime )
        {
            // This is a proxy object. The owning component is responsible for providing Outflow.
        }

        public double GetAvailableOutflowVolume()
        {
            return double.PositiveInfinity;
        }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            // A generic producer is assumed to be at some defined state, for now STP.
            return FluidState.STP;
        }

        public ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double mass )
        {
            // A generic producer needs to have its contents supplied by the owning component.
            // By default, it produces nothing.
            return PooledReadonlySubstanceStateCollection.Get();
        }
    }
}