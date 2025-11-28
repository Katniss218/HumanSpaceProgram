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
        public double deltaTime { get; private set; }

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


        private double[] currentFlowRates;
        private (double, double)[] currentPotentials;

        private double[] nextFlowRates;

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
            currentFlowRates = new double[Pipes.Length];
            currentPotentials = new (double, double)[Pipes.Length];
            nextFlowRates = new double[Pipes.Length];
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
        public void Step( float dt )
        {
            deltaTime = dt;
            const int MAX_ITERATIONS = 50;
            const double CONVERGENCE_THRESHOLD = 0.0001; // Tuned for precision

            // 1. Initial State: Sample potentials based on the tanks' current state (fluid surface potential).
            UpdatePotentials();

            bool converged = false;

            // 2. Solver Loop: Find equilibrium Flow Rates and Potentials.
            // Note: We do NOT move mass (SubstanceStateCollection) here. 
            // We only solve for the hydrodynamic scalar values (FlowRate).
            for( int iteration = 0; iteration < MAX_ITERATIONS; iteration++ )
            {
                // A. Calculate Flow Rates based on current Potentials
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    FlowPipe pipe = Pipes[i];
                    (double potFrom, double potTo) = currentPotentials[i];

                    // ComputeFlowRate typically uses potential difference logic.
                    // Result > 0 implies Flow From->To. Result < 0 implies To->From.
                    nextFlowRates[i] = pipe.ComputeFlowRate( potFrom, potTo );
                }

                // B. Check Convergence
                converged = true;
                for( int i = 0; i < Pipes.Length; i++ )
                {
                    // Check absolute delta
                    if( Math.Abs( nextFlowRates[i] - currentFlowRates[i] ) > CONVERGENCE_THRESHOLD )
                    {
                        converged = false;
                        break;
                    }
                }

                // Swap buffers
                var temp = currentFlowRates;
                currentFlowRates = nextFlowRates;
                nextFlowRates = temp;

                if( converged )
                    break;

                // C. Update Potentials based on new Flow Rates
                UpdatePotentials();
            }

            if( !converged )
            {
                throw new Exception( "FlowNetworkSnapshot.Step: Failed to converge within max iterations." );
            }

            // 3. Transport Phase: Move the actual substances based on the solved flow rates.
            ApplyTransport( dt );

            // 4. Notify objects
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
        }

        private void UpdatePotentials()
        {
            // Sample potentials from Producers
            for( int i = 0; i < Producers.Length; i++ )
            {
                IResourceProducer producer = Producers[i];
                int[] pipeIndices = ProducersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = Pipes[pipeIdx];

#warning TODO - include gas pressure in the overall potential?
                    // Sample potential at the connection point
                    if( ReferenceEquals( pipe.FromInlet.Producer, producer ) )
                    {
                        double p = producer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        currentPotentials[pipeIdx].Item1 = p;
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Producer, producer ) )
                    {
                        double p = producer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        currentPotentials[pipeIdx].Item2 = p;
                    }
                }
            }

            // Sample potentials from Consumers
            for( int i = 0; i < Consumers.Length; i++ )
            {
                IResourceConsumer consumer = Consumers[i];
                int[] pipeIndices = ConsumersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = Pipes[pipeIdx];

                    if( ReferenceEquals( pipe.FromInlet.Consumer, consumer ) )
                    {
                        double p = consumer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        currentPotentials[pipeIdx].Item1 = p;
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Consumer, consumer ) )
                    {
                        double p = consumer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        currentPotentials[pipeIdx].Item2 = p;
                    }
                }
            }
        }

        private void ApplyTransport( float dt )
        {
            for( int i = 0; i < Producers.Length; i++ )
            {
                Producers[i].Outflow?.Clear();
            }

            for( int i = 0; i < Consumers.Length; i++ )
            {
                Consumers[i].Inflow?.Clear();
            }

            for( int i = 0; i < Pipes.Length; i++ )
            {
                double signedFlowRate = currentFlowRates[i];
                if( Math.Abs( signedFlowRate ) < 1e-4 )
                    continue;

                FlowPipe pipe = Pipes[i];
                bool flowForward = signedFlowRate > 0;

                IResourceProducer source = flowForward ? pipe.FromInlet.Producer : pipe.ToInlet.Producer;
                IResourceConsumer sink = flowForward ? pipe.ToInlet.Consumer : pipe.FromInlet.Consumer;

                if( source == null || sink == null )
                    continue;

                IReadonlySubstanceStateCollection flowResources = pipe.SampleFlowResources( signedFlowRate, dt );

                if( flowResources.IsEmpty() )
                    continue;

                source.Outflow?.Add( flowResources, 1.0 );
                sink.Inflow?.Add( flowResources, 1.0 );
            }
        }
    }
}