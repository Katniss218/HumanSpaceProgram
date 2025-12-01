using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericProducer : IResourceProducer
    {
        // TODO...
        public Vector3 FluidAcceleration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3 FluidAngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubstanceStateCollection Outflow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            throw new NotImplementedException();
        }

        public ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double flowRate, double dt )
        {
            throw new NotImplementedException();
        }
    }
}