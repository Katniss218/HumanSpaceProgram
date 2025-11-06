using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FlowNetwork
    {
        public readonly List<FlowTank> Tanks = new List<FlowTank>();
        public readonly List<FlowPipe> Pipes = new List<FlowPipe>();

        /// <summary>
        /// Solves the flow network iteratively until convergence or max iterations.
        /// </summary>
        /// <param name="dt">Time step, in [s].</param>
        /// <param name="fluidAccelerationSceneSpace">Acceleration of fluid in scene space, in [m/s^2].</param>
        public void Solve( float dt, Vector3 fluidAccelerationSceneSpace )
        {
            const int MAX_ITERATIONS = 50;
            const float CONVERGENCE_THRESHOLD = 0.001f; // m^3/s

            // Update accelerations for all tanks
            foreach( var tank in Tanks )
            {
                // Acceleration should be set externally before calling Solve
                // tank.SetAcceleration( ... );
            }

            // Distribute fluids in tanks (stratification)
            foreach( var tank in Tanks )
            {
                // tank.DistributeFluids(); // Should be called externally
            }

            // Iterative flow solving
            float[] previousFlowRates = new float[Pipes.Count];
            float[] currentFlowRates = new float[Pipes.Count];

            for( int iteration = 0; iteration < MAX_ITERATIONS; iteration++ )
            {
                // Calculate flow rates for each pipe
                for( int i = 0; i < Pipes.Count; i++ )
                {
                    var pipe = Pipes[i];
                    float flowRate = pipe.ComputeFlowRate( fluidAccelerationSceneSpace );
                    currentFlowRates[i] = flowRate;
                }

                // Apply flows and clamp
                for( int i = 0; i < Pipes.Count; i++ )
                {
                    var pipe = Pipes[i];
                    float flowRate = currentFlowRates[i];

                    if( flowRate <= 0f )
                        continue;

                    // Sample the flow
                    var (flow, _) = pipe.SampleFlow( fluidAccelerationSceneSpace, dt );

                    if( flow.IsEmpty() )
                        continue;

                    // Clamp flow using consumer's ClampIn
                    if( pipe.ToInlet?.Consumer != null )
                    {
                        pipe.ToInlet.Consumer.ClampIn( flow, dt );
                    }

                    // Apply flow: remove from producer, add to consumer
                    if( pipe.FromInlet?.Producer != null && pipe.FromInlet.Producer.Outflow != null )
                    {
                        // Remove from producer (negative dt)
                        pipe.FromInlet.Producer.Outflow.Add( flow, -dt );
                    }

                    if( pipe.ToInlet?.Consumer != null && pipe.ToInlet.Consumer.Inflow != null )
                    {
                        // Add to consumer (positive dt)
                        pipe.ToInlet.Consumer.Inflow.Add( flow, dt );
                    }

                    // Update cached flow rates
                    pipe.FromInlet?.SetFlowRate( -flowRate ); // Negative for outflow
                    pipe.ToInlet?.SetFlowRate( flowRate ); // Positive for inflow
                }

                // Check for convergence
                bool converged = true;
                for( int i = 0; i < Pipes.Count; i++ )
                {
                    float change = Mathf.Abs( currentFlowRates[i] - previousFlowRates[i] );
                    if( change > CONVERGENCE_THRESHOLD )
                    {
                        converged = false;
                        break;
                    }
                }

                if( converged )
                    break;

                // Update previous flow rates for next iteration
                for( int i = 0; i < Pipes.Count; i++ )
                {
                    previousFlowRates[i] = currentFlowRates[i];
                }
            }
        }
    }
}