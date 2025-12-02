using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericConsumer : IResourceConsumer
    {
        public Vector3 FluidAcceleration { get; set; }
        public Vector3 FluidAngularVelocity { get; set; }
        public ISubstanceStateCollection Inflow { get; set; } = new SubstanceStateCollection();

        public double Demand { get; set; }

        public void ApplyFlows( double deltaTime )
        {
            // This is a proxy object. The owning component is responsible for handling Inflow.
        }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            // A generic consumer acts like a vacuum, pulling resources towards it.
            return FluidState.Vacuum;
        }
    }
}