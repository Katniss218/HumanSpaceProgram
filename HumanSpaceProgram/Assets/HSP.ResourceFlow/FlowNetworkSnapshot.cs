using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a snapshot of the fluid network at a given time.
    /// </summary>
    public sealed class FlowNetworkSnapshot
    {
        public static FlowNetworkSnapshot GetNetworkSnapshot( GameObject obj )
        {
            // return the flow network for this object and its children.

            // iterate over all descendants and collect their pipes. collect the tanks that these pipes connect to.
            // - No point solving tanks that don't matter.
            // - Also don't solve pipes that are closed.

            // iterate over gameobjects that implement this.
            if( obj == null )
                return null;

            FlowNetworkBuilder builder = new FlowNetworkBuilder();

            IBuildsFlowNetwork[] applyTo = obj.GetComponentsInChildren<IBuildsFlowNetwork>( true );

            List<IBuildsFlowNetwork> retryList = null;
            List<int> removeRetryIndices = null;
            foreach( var comp in applyTo )
            {
                var result = comp.BuildFlowNetwork( builder );
                if( result == BuildFlowResult.Retry )
                {
                    retryList ??= new();
                    retryList.Add( comp );
                }
            }

            // retry until nothing changes.
            while( retryList != null )
            {
                for( int i = 0; i < retryList.Count; i++ )
                {
                    var comp = retryList[i];
                    var result = comp.BuildFlowNetwork( builder );
                    if( result != BuildFlowResult.Retry )
                    {
                        removeRetryIndices ??= new();
                        removeRetryIndices.Add( i );
                    }
                }
                if( removeRetryIndices != null )
                {
                    if( removeRetryIndices.Count == 0 )
                        throw new Exception( "FlowNetworkBuilder: retry loop did not make progress." );

                    // remove in reverse order.
                    for( int i = removeRetryIndices.Count - 1; i >= 0; i-- )
                    {
                        int indexToRemove = removeRetryIndices[i];
                        retryList.RemoveAt( indexToRemove );
                    }
                    removeRetryIndices.Clear();
                }
            }

            IResourceConsumer[] consumers = builder.Consumers.ToArray();
            IResourceProducer[] producers = builder.Producers.ToArray();
            FlowPipe[] pipes = builder.Pipes.ToArray();
            // pipes are already added.

            // figure out which tanks are connected to which pipes.
            // FlowPipe already contains the connectivity info.
            int[][] consumersAndPipes = new int[consumers.Length][];
            int[][] producersAndPipes = new int[producers.Length][];
            List<int> tempPipeArray = new();
            for( int i = 0; i < consumers.Length; i++ )
            {
                IResourceConsumer consumer = consumers[i];
                for( int j = 0; j < pipes.Length; j++ )
                {
                    FlowPipe pipe = pipes[j];
                    if( pipe.ToInlet.Consumer == consumer )
                    {
                        tempPipeArray.Add( j );
                    }
                }
                consumersAndPipes[i] = tempPipeArray.ToArray();
                tempPipeArray.Clear();
            }
            for( int i = 0; i < producers.Length; i++ )
            {
                IResourceProducer producer = producers[i];
                for( int j = 0; j < pipes.Length; j++ )
                {
                    FlowPipe pipe = pipes[j];
                    if( pipe.FromInlet.Producer == producer )
                    {
                        tempPipeArray.Add( j );
                    }
                }
                producersAndPipes[i] = tempPipeArray.ToArray();
                tempPipeArray.Clear();
            }

            return new FlowNetworkSnapshot( obj, builder.Owner, applyTo, producers, producersAndPipes, consumers, consumersAndPipes, pipes );
        }

        // In general, only parts of the 'real' full network that need solving/evaluating will/should be included here.
        // E.g. tanks that actually are connected to something through an open pipe.
        private readonly IBuildsFlowNetwork[] applyTo;
        public readonly GameObject RootObject;
        private readonly FlowPipe[] Pipes;

        private readonly IResourceProducer[] Producers;
        private readonly int[][] ProducersAndPipes; // Lists indices to the Pipes array for each producer (which producer is connected to which pipes).
        private readonly IResourceConsumer[] Consumers;
        private readonly int[][] ConsumersAndPipes;
        private readonly IReadOnlyDictionary<object, object> _owner;

        public FlowNetworkSnapshot( GameObject rootObject, IReadOnlyDictionary<object, object> owner, IBuildsFlowNetwork[] applyTo, IResourceProducer[] producers, int[][] producersAndPipes, IResourceConsumer[] consumers, int[][] consumersAndPipes, FlowPipe[] pipes )
        {
            RootObject = rootObject;
            this.applyTo = applyTo;
            Producers = producers;
            ProducersAndPipes = producersAndPipes;
            Consumers = consumers;
            ConsumersAndPipes = consumersAndPipes;
            Pipes = pipes;
        }

        public bool TryGetFlowObj<T>( object obj, out T flowObj )
        {
            if( _owner.TryGetValue( obj, out var rawOwner ) && rawOwner is T typedOwner )
            {
                flowObj = typedOwner;
                return true;
            }
            flowObj = default;
            return false;
        }


        public bool IsValid()
        {
            // invalid if the 'real' objects moved/connections have been made, fluid was changed, etc.
            // each 'real' object needs to validate whether the simulation snapshot is still valid for itself.
            foreach( var a in applyTo )
            {
                if( !a.IsValid( this ) )
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Solves the flow network iteratively until convergence or max iterations.
        /// </summary>
        /// <param name="dt">Time step, in [s].</param>
        /// <param name="fluidAccelerationSceneSpace">Acceleration of fluid in scene space, in [m/s^2].</param>
        public void Step( float dt )
        {
#warning TODO - figure out the correct values for every tank, etc. Potentially cached across steps later.
            Vector3 fluidAccelerationSceneSpace = Vector3.zero;
            Vector3 angularVelocity = Vector3.zero;

            const int MAX_ITERATIONS = 50;
            const float CONVERGENCE_THRESHOLD = 0.001f; // m^3/s

            // Settle the fluids so we know what can flow out of which inlet.
            for( int i = 0; i < Producers.Length; i++ )
            {
                IResourceProducer producer = Producers[i];
                producer.Acceleration = fluidAccelerationSceneSpace;
                producer.AngularVelocity = angularVelocity;

                if( producer is FlowTank tank ) // only settle tanks once, since they will be added to both producers and consumers.
                    tank.DistributeFluids();
            }
            for( int i = 0; i < Consumers.Length; i++ )
            {
                IResourceConsumer consumer = Consumers[i];
                consumer.Acceleration = fluidAccelerationSceneSpace;
                consumer.AngularVelocity = angularVelocity;
            }

            float[] currentFlowRates = new float[Pipes.Length];
            (float, float)[] currentPressures = new (float, float)[Pipes.Length];

            float[] nextFlowRates = new float[Pipes.Length];
            (float, float)[] nextPressures = new (float, float)[Pipes.Length];

            for( int iteration = 0; iteration < MAX_ITERATIONS; iteration++ )
            {
                // Calculate flow rates for each pipe
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    var pipe = Pipes[i];
                    var pressure = nextPressures[i];
                    float flowRate = pipe.ComputeFlowRate( pressure.Item1, pressure.Item2 );
                    nextFlowRates[i] = flowRate;
                }

                // Apply flows and clamp
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    var pipe = Pipes[i];
                    float flowRate = nextFlowRates[i];

                    if( flowRate <= 0f )
                        continue;

                    // This computation needs to be cheap.
                    var flow = pipe.SampleFlowResources( flowRate, dt );

                    if( flow.IsEmpty() )
                        continue;

                    // now apply flow for each pipe
                    // remove from producer, add to consumer
                    if( pipe.FromInlet.Producer != null && pipe.FromInlet.Producer.Outflow != null )
                        pipe.FromInlet.Producer.Outflow.Add( flow, -dt );

                    if( pipe.ToInlet.Consumer != null && pipe.ToInlet.Consumer.Inflow != null )
                        pipe.ToInlet.Consumer.Inflow.Add( flow, dt );

                }

                // update temporary pressures based on the predicted flows (avoid a full update of the tank contents).
                for( int i = 0; i < ProducersAndPipes.Length; i++ )
                {
#warning TODO - figure out how much pressure change this flow would cause in each tank. 

                    // we want to update each real tank only once (at the very end).
                }
                for( int i = 0; i < ConsumersAndPipes.Length; i++ )
                {
#warning TODO - figure out how much pressure change this flow would cause in each tank. 

                    // we want to update each real tank only once (at the very end).
                }

                // Check for convergence
                bool converged = true;
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    float change = Mathf.Abs( nextFlowRates[i] - currentFlowRates[i] );
                    if( change > CONVERGENCE_THRESHOLD )
                    {
                        converged = false;
                        break;
                    }
                }

                if( converged )
                {
                    // Apply the result to the 'real' monobehaviours.
                    foreach( var a in applyTo )
                    {
                        a.ApplySnapshot( this );
                    }

                    break;
                }

                // Update previous flow rates for next iteration
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    currentFlowRates[i] = nextFlowRates[i];
                    currentPressures[i] = nextPressures[i];
                }
            }
        }
    }
}