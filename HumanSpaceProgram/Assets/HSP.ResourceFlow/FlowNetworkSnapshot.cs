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

            return new FlowNetworkSnapshot( obj, builder.Owner, applyTo, builder.Producers.ToList(), builder.Consumers.ToList(), builder.Pipes.ToList() );
        }

        public readonly GameObject RootObject;

        private readonly IBuildsFlowNetwork[] _applyTo;
        private readonly List<FlowPipe> _pipes;
        private readonly List<IResourceProducer> _producers;
        private readonly List<IResourceConsumer> _consumers;

        private int[][] _producersAndPipes;
        private int[][] _consumersAndPipes;
        private readonly IReadOnlyDictionary<object, object> _owner;

        // --- Partial Rebuild Data ---
        private readonly Dictionary<FlowPipe, int> _pipeToIndex = new();
        private readonly Dictionary<IResourceProducer, int> _producerToIndex = new();
        private readonly Dictionary<IResourceConsumer, int> _consumerToIndex = new();

        private readonly Queue<int> _freePipeSlots = new();
        private readonly Queue<int> _freeProducerSlots = new();
        private readonly Queue<int> _freeConsumerSlots = new();

        // --- Solver Buffers ---
        private (double, double)[] _currentPotentials;
        private double[] _previousFlowRates; // For oscillation detection
        private double[] _currentFlowRates;
        private double[] _nextFlowRates; // Buffer for unrelaxed flows

        private double _relaxationFactor;

        public FlowNetworkSnapshot( GameObject rootObject, IReadOnlyDictionary<object, object> owner, IBuildsFlowNetwork[] applyTo, List<IResourceProducer> producers, List<IResourceConsumer> consumers, List<FlowPipe> pipes )
        {
            RootObject = rootObject;
            _owner = owner;
            _applyTo = applyTo;
            _producers = producers;
            _consumers = consumers;
            _pipes = pipes;

            // Initialize solver buffers to the correct capacity.
            ResizeSolverBuffers( _pipes.Count, true );

            // Populate reverse lookup maps for the initial state.
            for( int i = 0; i < _producers.Count; i++ ) _producerToIndex[_producers[i]] = i;
            for( int i = 0; i < _consumers.Count; i++ ) _consumerToIndex[_consumers[i]] = i;
            for( int i = 0; i < _pipes.Count; i++ ) _pipeToIndex[_pipes[i]] = i;

            // Build initial connectivity maps.
            RebuildConnectivityMaps();
        }

        private void ResizeSolverBuffers( int capacity, bool exact )
        {
            if( _currentFlowRates == null || _currentFlowRates.Length < capacity )
            {
                int newSize = exact ? capacity : Math.Max( capacity, (_currentFlowRates?.Length ?? 0) * 2 );
                Array.Resize( ref _currentFlowRates, newSize );
                Array.Resize( ref _previousFlowRates, newSize );
                Array.Resize( ref _nextFlowRates, newSize );
                Array.Resize( ref _currentPotentials, newSize );
            }
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

        public void GetInvalidComponents( List<IBuildsFlowNetwork> invalidComponents )
        {
            foreach( var a in _applyTo )
            {
                if( !a.IsValid( this ) )
                {
                    invalidComponents.Add( a );
                }
            }
        }

        public void SynchronizeStateWithComponents()
        {
            foreach( var a in _applyTo )
            {
                a.SynchronizeState( this );
            }
        }

        public void ApplyTransaction( FlowNetworkBuilder transaction )
        {
            bool structureChanged = false;

            // Process Removals
            foreach( var pipe in transaction.PipeRemovals )
            {
                if( _pipeToIndex.TryGetValue( pipe, out int index ) )
                {
                    _pipes[index] = null;
                    _pipeToIndex.Remove( pipe );
                    _freePipeSlots.Enqueue( index );
                    structureChanged = true;
                }
            }
            foreach( var producer in transaction.ProducerRemovals )
            {
                if( _producerToIndex.TryGetValue( producer, out int index ) )
                {
                    _producers[index] = null;
                    _producerToIndex.Remove( producer );
                    _freeProducerSlots.Enqueue( index );
                    structureChanged = true;
                }
            }
            foreach( var consumer in transaction.ConsumerRemovals )
            {
                if( _consumerToIndex.TryGetValue( consumer, out int index ) )
                {
                    _consumers[index] = null;
                    _consumerToIndex.Remove( consumer );
                    _freeConsumerSlots.Enqueue( index );
                    structureChanged = true;
                }
            }

            // Process Additions
            if( transaction.Pipes.Any() )
            {
                structureChanged = true;
                foreach( var pipe in transaction.Pipes )
                {
                    if( _pipeToIndex.ContainsKey( pipe ) ) continue;

                    if( _freePipeSlots.TryDequeue( out int index ) )
                    {
                        _pipes[index] = pipe;
                        _pipeToIndex[pipe] = index;
                    }
                    else
                    {
                        int newIndex = _pipes.Count;
                        _pipes.Add( pipe );
                        _pipeToIndex[pipe] = newIndex;
                    }
                }
            }
            if( transaction.Producers.Any() )
            {
                structureChanged = true;
                foreach( var producer in transaction.Producers )
                {
                    if( _producerToIndex.ContainsKey( producer ) ) continue;

                    if( _freeProducerSlots.TryDequeue( out int index ) )
                    {
                        _producers[index] = producer;
                        _producerToIndex[producer] = index;
                    }
                    else
                    {
                        int newIndex = _producers.Count;
                        _producers.Add( producer );
                        _producerToIndex[producer] = newIndex;
                    }
                }
            }
            if( transaction.Consumers.Any() )
            {
                structureChanged = true;
                foreach( var consumer in transaction.Consumers )
                {
                    if( _consumerToIndex.ContainsKey( consumer ) ) continue;

                    if( _freeConsumerSlots.TryDequeue( out int index ) )
                    {
                        _consumers[index] = consumer;
                        _consumerToIndex[consumer] = index;
                    }
                    else
                    {
                        int newIndex = _consumers.Count;
                        _consumers.Add( consumer );
                        _consumerToIndex[consumer] = newIndex;
                    }
                }
            }

            if( structureChanged )
            {
                // Rebuild connectivity maps since pipes were added/removed.
                // This is simpler than trying to patch the maps.
                RebuildConnectivityMaps();

                // Ensure solver buffers are large enough.
                ResizeSolverBuffers( _pipes.Count, false );
            }
        }

        private void RebuildConnectivityMaps()
        {
            _producersAndPipes = BuildConnectivityMap( _producers, _pipes );
            _consumersAndPipes = BuildConnectivityMap( _consumers, _pipes );
        }

        private int[][] BuildConnectivityMap<T>( List<T> nodes, List<FlowPipe> pipes )
        {
            int[][] map = new int[nodes.Count][];
            List<int> tempPipeArray = new();

            for( int i = 0; i < nodes.Count; i++ )
            {
                object node = nodes[i];
                if( node == null )
                {
                    map[i] = Array.Empty<int>();
                    continue;
                }

                for( int j = 0; j < pipes.Count; j++ )
                {
                    FlowPipe pipe = pipes[j];
                    if( pipe == null ) continue;

                    bool connected = false;
                    if( node is IResourceConsumer consumer && (ReferenceEquals( pipe.FromInlet.Consumer, consumer ) || ReferenceEquals( pipe.ToInlet.Consumer, consumer )) )
                        connected = true;
                    if( node is IResourceProducer producer && (ReferenceEquals( pipe.FromInlet.Producer, producer ) || ReferenceEquals( pipe.ToInlet.Producer, producer )) )
                        connected = true;

                    if( connected )
                    {
                        tempPipeArray.Add( j );
                    }
                }
                map[i] = tempPipeArray.ToArray();
                tempPipeArray.Clear();
            }
            return map;
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
                for( int i = 0; i < _pipes.Count; i++ )
                {
                    FlowPipe pipe = _pipes[i];
                    if( pipe == null ) continue;

                    (double potFrom, double potTo) = _currentPotentials[i];

                    // ComputeFlowRate typically uses potential difference logic.
                    // Result > 0 implies Flow From->To. Result < 0 implies To->From.
                    _nextFlowRates[i] = pipe.ComputeFlowRate( potFrom, potTo );
                }

                // B. Check Convergence, Detect Oscillation, and Apply Relaxation
                converged = true;
                bool hasOscillations = false;
                for( int i = 0; i < _pipes.Count; i++ )
                {
                    if( _pipes[i] == null ) continue;

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
            for( int i = 0; i < _producers.Count; i++ )
            {
                IResourceProducer producer = _producers[i];
                if( producer == null ) continue;

                int[] pipeIndices = _producersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = _pipes[pipeIdx];
                    if( pipe == null ) continue;

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
            for( int i = 0; i < _consumers.Count; i++ )
            {
                IResourceConsumer consumer = _consumers[i];
                if( consumer == null ) continue;

                int[] pipeIndices = _consumersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = _pipes[pipeIdx];
                    if( pipe == null ) continue;

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
            {
                if( producer == null ) continue;
                producer.Outflow?.Clear();
            }
            foreach( var consumer in _consumers )
            {
                if( consumer == null ) continue;
                consumer.Inflow?.Clear();
            }

            // --- Step 1: Calculate Demand (Ideal Flow) ---
            // We store the signed flow rate for every pipe.
            // Positive = From -> To, Negative = To -> From
            double[] proposedFlows = _currentFlowRates; // Use the already calculated rates

            // We need to track total requested mass OUT of each tank.
            // Dictionary maps Tank Object -> Total Volume Requested to leave
            Dictionary<object, double> totalVolumeDemand = new Dictionary<object, double>();

            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null ) continue;

                double rate = proposedFlows[i];
                if( Math.Abs( rate ) < 1e-9 )
                    continue;

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
            double[] flowScalars = new double[_pipes.Count];
            Array.Fill( flowScalars, 1.0 );

            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null ) continue;

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
            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null ) continue;

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