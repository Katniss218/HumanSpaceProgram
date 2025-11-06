using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FResourceConnector_FlowPipe : MonoBehaviour
    {
        public FlowPipe pipe;
    }

    /// <summary>
    /// A pipe that connects two FlowTanks (or other objects) via inlet nodes. Doesn't have any volume.
    /// </summary>
    public class FlowPipe
    {
        public FlowInlet FromInlet { get; private set; }
        public FlowInlet ToInlet { get; private set; }

        /// <summary>
        /// The minimum cross-sectional area of the pipe, in [m^2].
        /// </summary>
        public float CrossSectionArea { get; private set; } = 0.1f;

        public FlowPipe( FlowInlet from, FlowInlet to, float crossSectionArea )
        {
            FromInlet = from;
            ToInlet = to;
            CrossSectionArea = crossSectionArea;
        }

        // compute volumetric flow rate (m^3/s) using simplified Torricelli-like relation
        // Q = A * v; v = sqrt(2 * deltaP / rho)
        // we cap and handle negative deltaP
        public float ComputeFlowRate( Vector3 fluidAccelerationSceneSpace )
        {
            // positive flowrate => from FromInlet to ToInlet.
            if( FromInlet == null || ToInlet == null )
                return 0f;

            // Get pressure at FromInlet
            float pFrom = 0f;
            float densityFrom = 1000f; // default density

            if( FromInlet.node != null )
            {
                // Inlet is attached to a FlowTank node - need to get pressure from tank
                // For now, use Sample() if producer is available
                if( FromInlet.Producer != null )
                {
                    Vector3 localAccel = FromInlet.Producer.transform.InverseTransformVector( fluidAccelerationSceneSpace );
                    FluidState fromState = FromInlet.Producer.Sample( FromInlet.node.pos, localAccel, FromInlet.nominalArea );
                    pFrom = fromState.Pressure;

                    // Estimate density from producer's contents
                    if( FromInlet.Producer.Outflow != null && !FromInlet.Producer.Outflow.IsEmpty() )
                    {
                        densityFrom = FromInlet.Producer.Outflow.GetAverageDensity();
                    }
                }
            }
            else if( FromInlet.Producer != null )
            {
                // Standalone inlet
                Vector3 localAccel = FromInlet.Producer.transform.InverseTransformVector( fluidAccelerationSceneSpace );
                FluidState fromState = FromInlet.Producer.Sample( FromInlet.LocalPosition, localAccel, FromInlet.nominalArea );
                pFrom = fromState.Pressure;

                if( FromInlet.Producer.Outflow != null && !FromInlet.Producer.Outflow.IsEmpty() )
                {
                    densityFrom = FromInlet.Producer.Outflow.GetAverageDensity();
                }
            }

            // Get pressure at ToInlet
            float pTo = 0f;

            if( ToInlet.node != null )
            {
                if( ToInlet.Consumer != null )
                {
                    Vector3 localAccel = ToInlet.Consumer.transform.InverseTransformVector( fluidAccelerationSceneSpace );
                    FluidState toState = ToInlet.Consumer.Sample( ToInlet.node.pos, localAccel, ToInlet.nominalArea );
                    pTo = toState.Pressure;
                }
            }
            else if( ToInlet.Consumer != null )
            {
                // Standalone inlet
                Vector3 localAccel = ToInlet.Consumer.transform.InverseTransformVector( fluidAccelerationSceneSpace );
                FluidState toState = ToInlet.Consumer.Sample( ToInlet.LocalPosition, localAccel, ToInlet.nominalArea );
                pTo = toState.Pressure;
            }

            // Calculate pressure difference
            float deltaP = pFrom - pTo;

            if( deltaP <= 0f )
                return 0f; // No backflow in passive pipe

            if( densityFrom <= 0f )
                densityFrom = 1f;

            // Torricelli's law: v = sqrt(2 * deltaP / rho)
            float velocity = Mathf.Sqrt( 2f * deltaP / densityFrom );

            // Flow rate: Q = min(CrossSectionArea, FromInlet.nominalArea, ToInlet.nominalArea) * v
            float effectiveArea = Mathf.Min( CrossSectionArea, FromInlet.nominalArea, ToInlet.nominalArea );
            float q = effectiveArea * velocity;

            return q;
        }

        /// <summary>
        /// Samples the flow that would occur through this pipe.
        /// </summary>
        public (SubstanceStateCollection flow, FluidState fluidState) SampleFlow( Vector3 fluidAccelerationSceneSpace, float dt )
        {
            float flowRate = ComputeFlowRate( fluidAccelerationSceneSpace );

            if( flowRate <= 0f || FromInlet?.Producer == null )
            {
                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
            }

            // Get fluid composition from producer
            SubstanceStateCollection flow = FromInlet.Producer.Outflow?.Clone() ?? SubstanceStateCollection.Empty;

            if( flow.IsEmpty() )
            {
                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
            }

            // Scale to flow rate
            float currentVolume = flow.GetVolume();
            if( currentVolume > 0f )
            {
                flow.SetVolume( flowRate );
            }
            else
            {
                return (SubstanceStateCollection.Empty, FluidState.Vacuum);
            }

            // Calculate fluid state
            float density = flow.GetAverageDensity();
            float deltaP = 0f; // Will be calculated in ComputeFlowRate
            float velocity = flowRate > 0f && FromInlet.nominalArea > 0f ? flowRate / FromInlet.nominalArea : 0f;

            FluidState fluidState = new FluidState
            {
                Pressure = deltaP,
                Temperature = 273.15f, // TODO: get from producer
                Velocity = velocity
            };

            return (flow, fluidState);
        }
    }

    /*public class FlowPipePump : FlowPipe
    {

    }
    public class FlowPipeValve : FlowPipe
    {

    }
    public class FlowPipeCheckValve : FlowPipe
    {

    }
    public class FlowPipeReliefValve : FlowPipe
    {

    }*/
}