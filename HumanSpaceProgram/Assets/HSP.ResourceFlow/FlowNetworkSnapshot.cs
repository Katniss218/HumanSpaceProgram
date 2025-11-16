using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
            while( retryList != null && retryList.Count > 0 )
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
                    if( pipe.FromInlet.Consumer == consumer || pipe.ToInlet.Consumer == consumer )
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
                    if( pipe.FromInlet.Producer == producer || pipe.ToInlet.Producer == producer )
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
        private readonly IBuildsFlowNetwork[] _applyTo;
        public readonly GameObject RootObject;
        private readonly FlowPipe[] Pipes;

        private readonly IResourceProducer[] Producers;
        private readonly int[][] ProducersAndPipes; // Lists indices to the Pipes array for each producer (which producer is connected to which pipes).
        private readonly IResourceConsumer[] Consumers;
        private readonly int[][] ConsumersAndPipes;
        private readonly IReadOnlyDictionary<object, object> _owner;


        private float[] currentFlowRates;
        private (float, float)[] currentPressures;

        private float[] nextFlowRates;
        private (float, float)[] nextPressures; // pressure at FromInlet and ToInlet

        public FlowNetworkSnapshot( GameObject rootObject, IReadOnlyDictionary<object, object> owner, IBuildsFlowNetwork[] applyTo, IResourceProducer[] producers, int[][] producersAndPipes, IResourceConsumer[] consumers, int[][] consumersAndPipes, FlowPipe[] pipes )
        {
            RootObject = rootObject;
            _owner = owner;
            _applyTo = applyTo;
            Producers = producers;
            ProducersAndPipes = producersAndPipes;
            Consumers = consumers;
            ConsumersAndPipes = consumersAndPipes;
            Pipes = pipes;
            currentFlowRates = new float[Pipes.Length];
            currentPressures = new (float, float)[Pipes.Length];
            nextFlowRates = new float[Pipes.Length];
            nextPressures = new (float, float)[Pipes.Length];
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
            // TODO - optimization: partial rebuild - rebuild only those parts/objects that have changed.

            // invalid if the 'real' objects moved/connections have been made, fluid was changed, etc.
            // each 'real' object needs to validate whether the simulation snapshot is still valid for itself.
            foreach( var a in _applyTo )
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
            const int MAX_ITERATIONS = 50;
            const float CONVERGENCE_THRESHOLD = 0.01f; // m^3/s

            // The fluids should come pre-settled when the snapshot is created.

            // Initialize temp pressures.
            for( int pIndex = 0; pIndex < ProducersAndPipes.Length; pIndex++ )
            {
                var producer = Producers[pIndex];
                var pipeList = ProducersAndPipes[pIndex]; // array of pipe indices connected to this producer
                for( int k = 0; k < pipeList.Length; k++ )
                {
                    int pipeIdx = pipeList[k];
                    FlowPipe pipe = Pipes[pipeIdx];
                    if( ReferenceEquals( pipe.FromInlet.Producer, producer ) )
                    {
                        float sampleP = producer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).Pressure;
                        var (from, to) = nextPressures[pipeIdx];
                        from = sampleP;
                        nextPressures[pipeIdx] = (from, to);
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Producer, producer ) )
                    {
                        float sampleP = producer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).Pressure;
                        var (from, to) = nextPressures[pipeIdx];
                        to = sampleP;
                        nextPressures[pipeIdx] = (from, to);
                    }
                    else
                    {
                        // somethng really bad happened.
                        throw new Exception( "FlowNetworkSnapshot.Step: Producer not found on its connected pipe." );
                    }
                }
            }
            for( int cIndex = 0; cIndex < ConsumersAndPipes.Length; cIndex++ )
            {
                var consumer = Consumers[cIndex];
                var pipeList = ConsumersAndPipes[cIndex];
                for( int k = 0; k < pipeList.Length; k++ )
                {
                    int pipeIdx = pipeList[k];
                    FlowPipe pipe = Pipes[pipeIdx];
                    if( ReferenceEquals( pipe.FromInlet.Consumer, consumer ) )
                    {
                        float sampleP = consumer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).Pressure;
                        var (from, to) = nextPressures[pipeIdx];
                        from = sampleP;
                        nextPressures[pipeIdx] = (from, to);
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Consumer, consumer ) )
                    {
                        float sampleP = consumer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).Pressure;
                        var (from, to) = nextPressures[pipeIdx];
                        to = sampleP;
                        nextPressures[pipeIdx] = (from, to);
                    }
                    else
                    {
                        // somethng really bad happened.
                        throw new Exception( "FlowNetworkSnapshot.Step: Producer not found on its connected pipe." );
                    }
                }
            }

            for( int iteration = 0; iteration < MAX_ITERATIONS; iteration++ )
            {
                // Calculate flow rates for each pipe.
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    FlowPipe pipe = Pipes[i];
                    (float from, float to) = currentPressures[i]; // from/to refer to the orientation of the pipe, not flow direction.
                    float flowRate = pipe.ComputeFlowRate( from, to );
                    nextFlowRates[i] = flowRate;
                }

                // Apply flows and clamp.
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    FlowPipe pipe = Pipes[i];
                    float signedFlowRate = nextFlowRates[i];

                    // This 'view' needs to be cheap.
                    IReadonlySubstanceStateCollection flowResources = pipe.SampleFlowResources( signedFlowRate, dt );

                    if( flowResources.IsEmpty() )
                        continue;

                    // Apply flow for each pipe.
                    // These are 'temporary' objects created for this snapshot (may be reused over multiple solves, if the 'real' objects haven't been changed/desynced).
                    // flowResources are always positive, regardless of flowrate sign.
                    if( pipe.FromInlet.Producer != null && pipe.FromInlet.Producer.Outflow != null )
                        pipe.FromInlet.Producer.Outflow.Add( flowResources, -dt );

                    if( pipe.ToInlet.Consumer != null && pipe.ToInlet.Consumer.Inflow != null )
                        pipe.ToInlet.Consumer.Inflow.Add( flowResources, dt );
                }

                // update temporary pressures based on the predicted flows (avoid a full update of the tank contents).
                for( int pIndex = 0; pIndex < ProducersAndPipes.Length; pIndex++ )
                {
                    var producer = Producers[pIndex];
                    var pipeList = ProducersAndPipes[pIndex]; // array of pipe indices connected to this producer
                    for( int k = 0; k < pipeList.Length; k++ )
                    {
                        int pipeIdx = pipeList[k];
                        FlowPipe pipe = Pipes[pipeIdx];
                        if( ReferenceEquals( pipe.FromInlet.Producer, producer ) )
                        {
                            float sampleP = producer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).Pressure;
                            var (from, to) = nextPressures[pipeIdx];
                            from = sampleP;
                            nextPressures[pipeIdx] = (from, to);
                        }
                        else if( ReferenceEquals( pipe.ToInlet.Producer, producer ) )
                        {
                            float sampleP = producer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).Pressure;
                            var (from, to) = nextPressures[pipeIdx];
                            to = sampleP;
                            nextPressures[pipeIdx] = (from, to);
                        }
                        else
                        {
                            // somethng really bad happened.
                            throw new Exception( "FlowNetworkSnapshot.Step: Producer not found on its connected pipe." );
                        }
                    }
                }
                for( int cIndex = 0; cIndex < ConsumersAndPipes.Length; cIndex++ )
                {
                    var consumer = Consumers[cIndex];
                    var pipeList = ConsumersAndPipes[cIndex];
                    for( int k = 0; k < pipeList.Length; k++ )
                    {
                        int pipeIdx = pipeList[k];
                        FlowPipe pipe = Pipes[pipeIdx];
                        if( ReferenceEquals( pipe.FromInlet.Consumer, consumer ) )
                        {
                            float sampleP = consumer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).Pressure;
                            var (from, to) = nextPressures[pipeIdx];
                            from = sampleP;
                            nextPressures[pipeIdx] = (from, to);
                        }
                        else if( ReferenceEquals( pipe.ToInlet.Consumer, consumer ) )
                        {
                            float sampleP = consumer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).Pressure;
                            var (from, to) = nextPressures[pipeIdx];
                            to = sampleP;
                            nextPressures[pipeIdx] = (from, to);
                        }
                        else
                        {
                            // somethng really bad happened.
                            throw new Exception( "FlowNetworkSnapshot.Step: Producer not found on its connected pipe." );
                        }
                    }
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
                    foreach( var a in _applyTo )
                    {
                        try
                        {
                            a.ApplySnapshot( this );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Exception occurred while applying flow snapshot to {a}." );
                            Debug.LogException( ex );
                        }
                    }

                    return;
                }

                // Update previous flow rates for next iteration
                var temp = currentFlowRates;
                currentFlowRates = nextFlowRates;
                nextFlowRates = temp;
                var tempP = currentPressures;
                currentPressures = nextPressures;
                nextPressures = tempP;
            }

            throw new Exception( "FlowNetworkSnapshot.Step: Failed to converge within max iterations." );
        }
    }
}