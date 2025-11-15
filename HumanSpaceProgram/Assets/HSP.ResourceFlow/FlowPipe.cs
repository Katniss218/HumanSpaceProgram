using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A pipe that connects two FlowTanks (or other objects) via inlet nodes. Doesn't have any volume.
    /// </summary>
    public class FlowPipe
    {
        public readonly struct Port
        {
            public readonly Vector3 pos;
            public readonly IResourceProducer Producer;
            public readonly IResourceConsumer Consumer;

            public Port( IResourceProducer producer, Vector3 pos )
            {
                Producer = producer;
                Consumer = null;
                this.pos = pos;
            }

            public Port( IResourceConsumer consumer, Vector3 pos )
            {
                Consumer = consumer;
                Producer = null;
                this.pos = pos;
            }
        }

        public Port FromInlet { get; private set; }
        public Port ToInlet { get; private set; }

        /// <summary>
        /// Volumetric conductance, in [m^3/(s*Pa)]
        /// </summary>
        public float Conductance = 1e-6f; // small default
        /// headAdded is positive when pushing from A->B (Pa)
        public float HeadAdded = 0f; // pump added head (Pa) from A->B

        /// <summary>
        /// The minimum cross-sectional area of the pipe, in [m^2].
        /// </summary>
        public float CrossSectionArea { get; private set; } = 0.1f;

        public FlowPipe( Port from, Port to, float crossSectionArea )
        {
            FromInlet = from;
            ToInlet = to;
            CrossSectionArea = crossSectionArea;
        }

        public float ComputeFlowRate( float pA, float pB )
        {
            float flowrate = Conductance * (pA - pB + HeadAdded);
            if( !float.IsFinite( flowrate ) )
                flowrate = 0f;

            return flowrate;
        }

        /// <summary>
        /// Samples the actual flow that would occur through this pipe for the given flowrate (uses tanks' accelerations to determine solids flows).
        /// </summary>
        public IReadonlySubstanceStateCollection SampleFlowResources( float flowRate, float dt )
        {
            if( flowRate == 0f )
                return SubstanceStateCollection.Empty;

            IResourceConsumer consumer;
            IResourceProducer producer;
            // Positive flowRate means from FromInlet to ToInlet.
            if( flowRate < 0 )
            {
                producer = ToInlet.Producer;
                consumer = FromInlet.Consumer;
            }
            else
            {
                producer = FromInlet.Producer;
                consumer = ToInlet.Consumer;
            }
            // TODO - bidirectional flow later (diffusion)

            // Do not clamp. instead we'll use compressibility to determine whether the resources can fit / what flowrate is achievable until equilibrium hits.
            IReadonlySubstanceStateCollection flow = producer.SampleSubstances( FromInlet.pos, Math.Abs( flowRate ), dt );


            return flow;
        }
    }
}