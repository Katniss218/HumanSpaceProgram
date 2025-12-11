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
            public readonly float area;
            public readonly IResourceProducer Producer;
            public readonly IResourceConsumer Consumer;

            public Port( IResourceProducer producer, Vector3 pos, float area )
            {
                Producer = producer;
                Consumer = producer as IResourceConsumer;
                this.pos = pos;
                this.area = area;
            }

            public Port( IResourceConsumer consumer, Vector3 pos, float area )
            {
                Producer = consumer as IResourceProducer;
                Consumer = consumer;
                this.pos = pos;
                this.area = area;
            }
        }

        public Port FromInlet { get; }
        public Port ToInlet { get; }

        public double Length { get; }
        public double Area { get; }
        public double Diameter { get; }

        /// <summary>
        /// Mass flow conductance, in [kg*s/m^2].
        /// This is a linearized coefficient, where Mass Flow Rate (kg/s) = Conductance * Potential Difference (J/kg).
        /// This value is dynamically calculated and updated by the solver each frame.
        /// </summary>
        public double MassFlowConductance { get; set; }

        /// <summary>
        /// Pump added head potential, positive when pushing from <see cref="FromInlet"/> to <see cref="ToInlet"/>, in [J/kg]
        /// </summary>
        public double HeadAdded { get; set; }

        // --- State for solver ---
        internal double MassFlowRateLastStep { get; set; }
        internal double DensityLastStep { get; set; }
        internal double ViscosityLastStep { get; set; }
        internal double ConductanceLastStep { get; set; }


        public FlowPipe( Port fromInlet, Port toInlet, double length, double area )
        {
            FromInlet = fromInlet;
            ToInlet = toInlet;
            Length = Math.Max( length, 0.001 ); // Min length 1mm
            Area = area;
            Diameter = Math.Sqrt( 4 * area / Math.PI );
            MassFlowConductance = 0; // Initial value, will be calculated by solver.
            MassFlowRateLastStep = 0;
            DensityLastStep = 0;
            ViscosityLastStep = 0;
            ConductanceLastStep = 0;
        }

        public double ComputeMassFlowRate( double potentialFrom, double potentialTo )
        {
            double flowrate = MassFlowConductance * (potentialFrom - potentialTo + HeadAdded);
            if( !double.IsFinite( flowrate ) )
                flowrate = 0f;

            return flowrate;
        }

        /// <summary>
        /// Samples the actual flow that would occur through this pipe for the given mass flow (uses tanks' accelerations to determine solids flows).
        /// </summary>
        /// <param name="signedMassFlow">Mass flow rate (positive is flow from FromInlet to ToInlet), in [kg/s].</param>
        /// <param name="dt"">Timestep used for scaling the flowing resources, in [s].</param>
        public ISampledSubstanceStateCollection SampleFlowResources( double signedMassFlow, double dt )
        {
            if( Math.Abs( signedMassFlow ) < 1e-9 )
                return PooledReadonlySubstanceStateCollection.Get();

            IResourceConsumer consumer;
            IResourceProducer producer;
            Vector3 samplePos;
            if( signedMassFlow < 0 )
            {
                producer = ToInlet.Producer;
                consumer = FromInlet.Consumer;
                samplePos = ToInlet.pos;
            }
            else
            {
                producer = FromInlet.Producer;
                consumer = ToInlet.Consumer;
                samplePos = FromInlet.pos;
            }

            if( producer == null || consumer == null )
                return PooledReadonlySubstanceStateCollection.Get();

            double massToTransfer = Math.Abs( signedMassFlow ) * dt;
            return producer.SampleSubstances( samplePos, massToTransfer );
        }
    }
}
