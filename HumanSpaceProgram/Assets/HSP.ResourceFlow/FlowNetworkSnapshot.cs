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

        public readonly GameObject RootObject;

        // In general, only parts of the 'real' full network that need solving/evaluating will/should be included here.
        // E.g. tanks that actually are connected to something through an open pipe.
        private readonly IBuildsFlowNetwork[] _applyTo;
        private readonly FlowPipe[] _pipes;

        private readonly IResourceProducer[] _producers;
        private readonly int[][] _producersAndPipes; // Lists indices to the Pipes array for each producer (which producer is connected to which pipes).
        private readonly IResourceConsumer[] _consumers;
        private readonly int[][] _consumersAndPipes;
        private readonly IReadOnlyDictionary<object, object> _owner;

        private (double, double)[] _currentPotentials;
        private double[] _previousFlowRates; // For oscillation detection
        private double[] _currentFlowRates;
        private double[] _nextFlowRates; // Buffer for unrelaxed flows

        private double _relaxationFactor;

        public FlowNetworkSnapshot( GameObject rootObject, IReadOnlyDictionary<object, object> owner, IBuildsFlowNetwork[] applyTo, IResourceProducer[] producers, int[][] producersAndPipes, IResourceConsumer[] consumers, int[][] consumersAndPipes, FlowPipe[] pipes )
        {
            RootObject = rootObject;
            _owner = owner;
            _applyTo = applyTo;
            _producers = producers;
            _producersAndPipes = producersAndPipes;
            _consumers = consumers;
            _consumersAndPipes = consumersAndPipes;
            _pipes = pipes;
            _currentFlowRates = new double[_pipes.Length];
            _currentPotentials = new (double, double)[_pipes.Length];
            _nextFlowRates = new double[_pipes.Length];
            _previousFlowRates = new double[_pipes.Length];
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
            const double CONVERGENCE_THRESHOLD = 0.0001;
            _relaxationFactor = 0.7; // Start with a reasonable under-relaxation.
            Array.Clear( _previousFlowRates, 0, _previousFlowRates.Length );

            // 1. Initial State: Sample potentials based on the tanks' current state (fluid surface potential).
            UpdatePotentials();

            bool converged = false;

            // 2. Solver Loop: Find equilibrium Flow Rates and Potentials.
            for( int iteration = 0; iteration < MAX_ITERATIONS; iteration++ )
            {
                // A. Calculate Flow Rates based on current Potentials
                for( int i = 0; i < _pipes.Length; i++ )
                {
                    FlowPipe pipe = _pipes[i];
                    (double potFrom, double potTo) = _currentPotentials[i];

                    // ComputeFlowRate typically uses potential difference logic.
                    // Result > 0 implies Flow From->To. Result < 0 implies To->From.
                    _nextFlowRates[i] = pipe.ComputeFlowRate( potFrom, potTo );
                }

                // B. Check Convergence, Detect Oscillation, and Apply Relaxation
                converged = true;
                bool hasOscillations = false;
                for( int i = 0; i < _pipes.Length; i++ )
                {
                    double prevFlow = _currentFlowRates[i];
                    double prevPrevFlow = _previousFlowRates[i];
                    double newFlowUnrelaxed = _nextFlowRates[i];

                    // Oscillation detection: check if the flow rate overshot the previous value.
                    // A negative product means the direction of change has flipped.
                    if( (newFlowUnrelaxed - prevFlow) * (prevFlow - prevPrevFlow) < -1e-12 )
                    {
                        hasOscillations = true;
                    }

                    // Apply relaxation factor to the change in flow.
                    double relaxedFlow = prevFlow + (newFlowUnrelaxed - prevFlow) * _relaxationFactor;

                    // Check absolute delta for convergence against the relaxed value.
                    if( Math.Abs( relaxedFlow - prevFlow ) > CONVERGENCE_THRESHOLD )
                    {
                        converged = false;
                    }

                    // Update history for the next iteration.
                    _previousFlowRates[i] = prevFlow;
                    _currentFlowRates[i] = relaxedFlow;
                }

                // Dynamically adjust relaxation factor for next iteration.
                if( hasOscillations )
                {
                    // If oscillating, aggressively reduce the factor.
                    _relaxationFactor = Math.Max( 0.1, _relaxationFactor * 0.9 );
                }
                else if( !converged )
                {
                    // If stable but not converged, gently increase the factor towards 1.0.
                    _relaxationFactor = Math.Min( 1.0, _relaxationFactor * 1.05 );
                }


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
            for( int i = 0; i < _producers.Length; i++ )
            {
                IResourceProducer producer = _producers[i];
                int[] pipeIndices = _producersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = _pipes[pipeIdx];

                    // Sample potential at the connection point
                    if( ReferenceEquals( pipe.FromInlet.Producer, producer ) )
                    {
                        double p = producer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item1 = p;
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Producer, producer ) )
                    {
                        double p = producer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item2 = p;
                    }
                }
            }

            // Sample potentials from Consumers
            for( int i = 0; i < _consumers.Length; i++ )
            {
                IResourceConsumer consumer = _consumers[i];
                int[] pipeIndices = _consumersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = _pipes[pipeIdx];

                    if( ReferenceEquals( pipe.FromInlet.Consumer, consumer ) )
                    {
                        double p = consumer.Sample( pipe.FromInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item1 = p;
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Consumer, consumer ) )
                    {
                        double p = consumer.Sample( pipe.ToInlet.pos, pipe.CrossSectionArea ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item2 = p;
                    }
                }
            }
        }

        private void ApplyTransport( float dt )
        {
            // Clear previous IO states
            foreach( var producer in _producers ) 
                producer.Outflow?.Clear();
            foreach( var consumer in _consumers )
                consumer.Inflow?.Clear();

            // --- Step 1: Calculate Demand (Ideal Flow) ---
            // We store the signed flow rate for every pipe.
            // Positive = From -> To, Negative = To -> From
            double[] proposedFlows = new double[_pipes.Length];

            // We need to track total requested mass OUT of each tank.
            // Dictionary maps Tank Object -> Total Volume Requested to leave
            Dictionary<object, double> totalVolumeDemand = new Dictionary<object, double>();

            for( int i = 0; i < _pipes.Length; i++ )
            {
                double rate = _currentFlowRates[i];
                if( Math.Abs( rate ) < 1e-9 )
                    continue;

                proposedFlows[i] = rate;
                double volRequested = Math.Abs( rate ) * dt;

                // Identify Source
                object sourceObj = (rate > 0) ? _pipes[i].FromInlet.Producer : _pipes[i].ToInlet.Producer;

                if( sourceObj != null )
                {
                    if( !totalVolumeDemand.ContainsKey( sourceObj ) )
                        totalVolumeDemand[sourceObj] = 0;

                    totalVolumeDemand[sourceObj] += volRequested;
                }
            }

            // --- Step 2 & 3: Limit & Scale ---
            // We compute a scaling factor (0.0 to 1.0) for every pipe.
            double[] flowScalars = new double[_pipes.Length];
            Array.Fill( flowScalars, 1.0 );

            for( int i = 0; i < _pipes.Length; i++ )
            {
                double rate = proposedFlows[i];
                if( Math.Abs( rate ) < 1e-9 ) 
                    continue;

                object sourceObj = (rate > 0) ? _pipes[i].FromInlet.Producer : _pipes[i].ToInlet.Producer;

                if( sourceObj is FlowTank tank )
                {
                    // Retrieve the total demand calculated in Step 1
                    if( totalVolumeDemand.TryGetValue( tank, out double totalDemand ) )
                    {
                        // Check tank contents (Volume is usually easier to check than specific mass at this stage)
                        // If your tank mixes fluids, use total volume. 
                        double availableVolume = tank.Volume; // Or tank.Contents.TotalVolume();

                        if( totalDemand > availableVolume && totalDemand > 0 )
                        {
                            // The tank is over-subscribed. Scale down.
                            // Example: 10L available, 20L demanded. Scale = 0.5.
                            double scale = availableVolume / totalDemand;

                            // We apply the most restrictive limit found. 
                            // (Usually a pipe has 1 source, so we just set it, but min protects against edge cases).
                            flowScalars[i] = Math.Min( flowScalars[i], scale );
                        }
                    }
                }
            }

            // --- Step 4: Execution ---
            for( int i = 0; i < _pipes.Length; i++ )
            {
                double rawRate = proposedFlows[i];
                if( Math.Abs( rawRate ) < 1e-9 ) 
                    continue;

                // Apply the scaling factor we calculated
                double finalRate = rawRate * flowScalars[i];

                FlowPipe pipe = _pipes[i];
                bool flowForward = finalRate > 0;

                IResourceProducer source = flowForward ? pipe.FromInlet.Producer : pipe.ToInlet.Producer;
                IResourceConsumer sink = flowForward ? pipe.ToInlet.Consumer : pipe.FromInlet.Consumer;

                if( source == null || sink == null ) 
                    continue;

                // Now we can safely sample. 
                // The tank might still do internal clamping, but we know physically we aren't asking for more 
                // than the TOTAL tank contains across all pipes.
                IReadonlySubstanceStateCollection flowResources = pipe.SampleFlowResources( finalRate, dt );

                if( flowResources.IsEmpty() ) 
                    continue;

                source.Outflow?.Add( flowResources, 1.0 );
                sink.Inflow?.Add( flowResources, 1.0 );
            }
        }
    }
}