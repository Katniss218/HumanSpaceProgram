using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A pipe that connects two FlowTanks (or other objects) via inlet nodes. Doesn't have any volume.
    /// </summary>
    public class FlowPipe
    {
        /// <summary>
        /// Represents an inlet/outlet.
        /// </summary>
        public readonly struct Port
        {
            /// <summary>
            /// The position of the inlet, in simulation space.
            /// </summary>
            public readonly Vector3 pos;
            public readonly IResourceProducer Producer;
            public readonly IResourceConsumer Consumer;

            public Port( IResourceProducer producer, Vector3 pos )
            {
                Producer = producer;
                Consumer = producer as IResourceConsumer;
                this.pos = pos;
            }

            public Port( IResourceConsumer consumer, Vector3 pos )
            {
                Producer = consumer as IResourceProducer;
                Consumer = consumer;
                this.pos = pos;
            }
        }

        public Port FromInlet { get; }

        public Port ToInlet { get; }

        /// <summary>
        /// The minimum cross-sectional area of the pipe, in [m^2].
        /// </summary>
        public double CrossSectionArea { get; }

        /// <summary>
        /// Volumetric conductance, in [m^3/(s*(J/kg))]
        /// </summary>
        public double Conductance { get; set; }

        /// <summary>
        /// Pump added head potential, positive when pushing from <see cref="FromInlet"/> to <see cref="ToInlet"/>, in [J/kg]
        /// </summary>
        public double HeadAdded { get; set; }

        public FlowPipe( Port fromInlet, Port toInlet, double crossSectionArea, double conductance = 1.0, double headAdded = 0.0 )
        {
            FromInlet = fromInlet;
            ToInlet = toInlet;
            CrossSectionArea = crossSectionArea;
            Conductance = conductance;
            HeadAdded = headAdded;
        }

        public double ComputeFlowRate( double potentialFrom, double potentialTo )
        {
            double flowrate = Conductance * (potentialFrom - potentialTo + HeadAdded);
            if( !double.IsFinite( flowrate ) )
                flowrate = 0f;

            return flowrate;
        }

        /// <summary>
        /// Samples the actual flow that would occur through this pipe for the given flowrate (uses tanks' accelerations to determine solids flows).
        /// </summary>
        /// <param name="signedFlowRate">Volumetric flow rate (positive is flow from FromInlet to ToInlet), in [m^3/s].</param>
        /// <param name="dt"">Timestep used for scaling the flowing resources, in [s].</param>
        public IReadonlySubstanceStateCollection SampleFlowResources( double signedFlowRate, double dt )
        {
            if( signedFlowRate == 0f )
                return SubstanceStateCollection.Empty;

            IResourceConsumer consumer;
            IResourceProducer producer;
            if( signedFlowRate < 0 )
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
            IReadonlySubstanceStateCollection flow = producer.SampleSubstances( FromInlet.pos, Math.Abs( signedFlowRate ), dt );


            return flow;
        }
    }
}