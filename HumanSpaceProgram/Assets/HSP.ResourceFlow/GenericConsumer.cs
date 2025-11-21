using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericConsumer : IResourceConsumer
    {
        // TODO...
        public Vector3 FluidAcceleration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3 FluidAngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubstanceStateCollection Inflow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            throw new NotImplementedException();
        }
    }
}