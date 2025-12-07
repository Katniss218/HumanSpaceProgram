using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a snapshot of the fluid network at a given time. This class contains the core solver logic.
    /// The solver is iterative, and uses potential-based flow and under-relaxation for stability.
    /// </summary>
    public sealed class FlowNetworkSnapshot
    {
        public readonly GameObject RootObject;

        private readonly IBuildsFlowNetwork[] _applyTo;
        private readonly List<FlowPipe> _pipes;
        private readonly List<IResourceProducer> _producers;
        private readonly List<IResourceConsumer> _consumers;
        private readonly List<object> _participants = new();

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
        private double[] _flowRatesLastStep; // For oscillation detection
        private double[] _currentFlowRates;
        private double[] _nextFlowRates; // Buffer for unrelaxed flows
        private double[] _pipeFlowScalingFactors;
        private double[] _pipeLearnedRelaxationFactors;
        private double[] _producerStiffness;
        private double[] _consumerStiffness;

        private double _relaxationFactor = 0.7;

        // --- Transport Buffers ---
        private readonly Dictionary<IResourceConsumer, double> _consumerVolumeDemand = new();
        private readonly Dictionary<IResourceProducer, double> _producerVolumeSupply = new();
        private readonly Dictionary<IResourceProducer, double> _producerScalingFactors = new();
        private readonly Dictionary<IResourceConsumer, double> _consumerScalingFactors = new();


        // --- Public Read-only access for debug visualizers and similar ---
        public IReadOnlyList<FlowPipe> Pipes => _pipes;
        public IReadOnlyList<IResourceProducer> Producers => _producers;
        public IReadOnlyList<IResourceConsumer> Consumers => _consumers;
        public double[] CurrentFlowRates => _currentFlowRates;


        public FlowNetworkSnapshot( GameObject rootObject, IReadOnlyDictionary<object, object> owner, IBuildsFlowNetwork[] applyTo, List<IResourceProducer> producers, List<IResourceConsumer> consumers, List<FlowPipe> pipes )
        {
            RootObject = rootObject;
            _owner = owner;
            _applyTo = applyTo;
            _producers = producers;
            _consumers = consumers;
            _pipes = pipes;

            // Initialize solver buffers to the correct capacity.
            _pipeLearnedRelaxationFactors = new double[_pipes.Count];
            for( int i = 0; i < _pipes.Count; i++ )
            {
                _pipeLearnedRelaxationFactors[i] = 1.0;
            }

            ResizeSolverBuffers( _pipes.Count, true );
            ResizeStiffnessBuffers( _producers.Count, _consumers.Count, true );
            _pipeFlowScalingFactors = new double[_pipes.Count];

            // Populate reverse lookup maps for the initial state.
            for( int i = 0; i < _producers.Count; i++ )
                _producerToIndex[_producers[i]] = i;
            for( int i = 0; i < _consumers.Count; i++ )
                _consumerToIndex[_consumers[i]] = i;
            for( int i = 0; i < _pipes.Count; i++ )
                _pipeToIndex[_pipes[i]] = i;

            // Build initial connectivity maps.
            RebuildConnectivityMaps();
            RebuildParticipantsList();
        }

        private void ResizeSolverBuffers( int capacity, bool exact )
        {
            if( _currentFlowRates == null || _currentFlowRates.Length < capacity )
            {
                int oldSize = _currentFlowRates?.Length ?? 0;
                int newSize = exact ? capacity : Math.Max( capacity, oldSize * 2 );
                Array.Resize( ref _currentFlowRates, newSize );
                Array.Resize( ref _flowRatesLastStep, newSize );
                Array.Resize( ref _nextFlowRates, newSize );
                Array.Resize( ref _currentPotentials, newSize );
                Array.Resize( ref _pipeLearnedRelaxationFactors, newSize );

                if( newSize > oldSize )
                {
                    // Initialize new elements for learned relaxation factors
                    for( int i = oldSize; i < newSize; i++ )
                    {
                        _pipeLearnedRelaxationFactors[i] = 1.0;
                    }
                }
            }
        }

        private void ResizeStiffnessBuffers( int producerCapacity, int consumerCapacity, bool exact )
        {
            if( _producerStiffness == null || _producerStiffness.Length < producerCapacity )
            {
                int newSize = exact ? producerCapacity : Math.Max( producerCapacity, _producerStiffness?.Length * 2 ?? producerCapacity );
                Array.Resize( ref _producerStiffness, newSize );
            }
            if( _consumerStiffness == null || _consumerStiffness.Length < consumerCapacity )
            {
                int newSize = exact ? consumerCapacity : Math.Max( consumerCapacity, _consumerStiffness?.Length * 2 ?? consumerCapacity );
                Array.Resize( ref _consumerStiffness, newSize );
            }
        }

        /// <summary>
        /// Initializes the solver's flow rates from a previously saved state.
        /// This is crucial for preventing a "hard reset" of flow on game load.
        /// </summary>
        /// <param name="savedRates">The array of flow rates from the last saved state.</param>
        public void InitializeFlowRates( double[] savedRates )
        {
            if( savedRates == null )
                return;

            // Copy saved rates to both current and last step buffers to prevent the oscillation
            // detector from firing on the first frame due to a large delta from zero.
            Array.Copy( savedRates, _currentFlowRates, Math.Min( savedRates.Length, _currentFlowRates.Length ) );
            Array.Copy( savedRates, _flowRatesLastStep, Math.Min( savedRates.Length, _flowRatesLastStep.Length ) );
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
                    if( _pipeToIndex.ContainsKey( pipe ) )
                        continue;

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
                    if( _producerToIndex.ContainsKey( producer ) )
                        continue;

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
                    if( _consumerToIndex.ContainsKey( consumer ) )
                        continue;

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
                RebuildParticipantsList();

                // Ensure solver buffers are large enough.
                ResizeSolverBuffers( _pipes.Count, false );
                ResizeStiffnessBuffers( _producers.Count, _consumers.Count, false );
                if( _pipeFlowScalingFactors.Length < _pipes.Count )
                {
                    Array.Resize( ref _pipeFlowScalingFactors, _pipes.Count );
                }
            }
        }

        private void RebuildConnectivityMaps()
        {
            _producersAndPipes = BuildConnectivityMap( _producers, _pipes );
            _consumersAndPipes = BuildConnectivityMap( _consumers, _pipes );
        }

        private void RebuildParticipantsList()
        {
            _participants.Clear();
            var uniqueParticipants = new HashSet<object>();
            foreach( var p in _producers )
            {
                if( p != null )
                    uniqueParticipants.Add( p );
            }
            foreach( var c in _consumers )
            {
                if( c != null )
                    uniqueParticipants.Add( c );
            }
            _participants.AddRange( uniqueParticipants );
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
                    if( pipe == null )
                        continue;

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
            const int MAX_ITERATIONS = 50;
            //Debug.Log( "_relaxationFactor " + _relaxationFactor );

            // 1. Initial State: Sample potentials and stiffness based on the tanks' current state.
            UpdateStiffnessCache();
            UpdatePotentials();

            bool converged = false;
            for( int iteration = 0; iteration < MAX_ITERATIONS; iteration++ )
            {
                // A. Calculate new flow rates based on current potentials.
                CalculateUnrelaxedFlows( dt );

                // B. Check convergence and apply relaxation with adaptive damping.
                converged = CheckConvergenceAndApplyRelaxation( out bool hasOscillations );

                // C. Dynamically adjust global relaxation factor for the next iteration.
                if( hasOscillations )
                {
#warning INFO - related to how much the relaxation can go down in total, since it resets.
                    _relaxationFactor = Math.Max( 0.1, _relaxationFactor * 0.75 ); // More aggressive damping
                }
                else if( !converged )
                {
                    _relaxationFactor = Math.Min( 1.0, _relaxationFactor * 1.01 ); // Gently accelerate convergence
                }

                if( converged )
                    break;
            }

            if( !converged )
            {
                // Revert to last known stable state if convergence fails.
                // This is safer than zeroing flow, which can cause sudden stops/starts.
                if( _pipes.Count > 0 )
                {
                    Array.Copy( _flowRatesLastStep, _currentFlowRates, _pipes.Count );
                }
            }

            // 3. Transport Phase: Move the actual substances based on the solved flow rates.
            ApplyTransport( dt );

            // Apply flows to simulation objects
            foreach( var participant in _participants )
            {
                if( participant is IResourceProducer producer )
                {
                    producer.ApplyFlows( dt );
                }
                else if( participant is IResourceConsumer consumer )
                {
                    // This handles cases where an object is only a consumer
                    consumer.ApplyFlows( dt );
                }
            }

            // After the solver has run and flows have been applied to simulation objects for this step,
            // store the final flow rates. These will be used as the stable starting point for the next step's solver.
            if( _pipes.Count > 0 )
            {
                Array.Copy( _currentFlowRates, _flowRatesLastStep, _pipes.Count );
            }
        }

        public void ApplySnapshotToComponents()
        {
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

        private void UpdateStiffnessCache()
        {
            for( int i = 0; i < _producers.Count; i++ )
            {
                var p = _producers[i];
                if( p is IStiffnessProvider sp )
                    _producerStiffness[i] = sp.GetPotentialDerivativeWrtVolume();
                else
                    _producerStiffness[i] = 0.0;
            }
            for( int i = 0; i < _consumers.Count; i++ )
            {
                var c = _consumers[i];
                if( c is IStiffnessProvider sp )
                    _consumerStiffness[i] = sp.GetPotentialDerivativeWrtVolume();
                else
                    _consumerStiffness[i] = 0.0;
            }
        }

        private void UpdatePotentials()
        {
            // Sample potentials from Producers
            for( int i = 0; i < _producers.Count; i++ )
            {
                IResourceProducer producer = _producers[i];
                if( producer == null )
                    continue;

                int[] pipeIndices = _producersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = _pipes[pipeIdx];
                    if( pipe == null )
                        continue;

                    // Sample potential at the connection point
                    if( ReferenceEquals( pipe.FromInlet.Producer, producer ) )
                    {
                        double p = producer.Sample( pipe.FromInlet.pos, pipe.FromInlet.area ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item1 = p;
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Producer, producer ) )
                    {
                        double p = producer.Sample( pipe.ToInlet.pos, pipe.FromInlet.area ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item2 = p;
                    }
                }
            }

            // Sample potentials from Consumers
            for( int i = 0; i < _consumers.Count; i++ )
            {
                IResourceConsumer consumer = _consumers[i];
                if( consumer == null )
                    continue;

                int[] pipeIndices = _consumersAndPipes[i];

                for( int k = 0; k < pipeIndices.Length; k++ )
                {
                    int pipeIdx = pipeIndices[k];
                    FlowPipe pipe = _pipes[pipeIdx];
                    if( pipe == null )
                        continue;

                    if( ReferenceEquals( pipe.FromInlet.Consumer, consumer ) )
                    {
                        double p = consumer.Sample( pipe.FromInlet.pos, pipe.FromInlet.area ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item1 = p;
                    }
                    else if( ReferenceEquals( pipe.ToInlet.Consumer, consumer ) )
                    {
                        double p = consumer.Sample( pipe.ToInlet.pos, pipe.FromInlet.area ).FluidSurfacePotential;
                        _currentPotentials[pipeIdx].Item2 = p;
                    }
                }
            }
        }

#warning TODO - we need (edit mode) tests that test the various relaxation and convergence behaviors, with the flow network created without any wrappers.

        private void CalculateUnrelaxedFlows( double dt )
        {
            for( int i = 0; i < _pipes.Count; i++ )
            {
                FlowPipe pipe = _pipes[i];
                if( pipe == null )
                    continue;

                (double potentialFrom, double potentialTo) = _currentPotentials[i];

                // 1. Calculate the raw flow rate.
#warning TODO - after splitting, potentials nearly equal -14 -12 = -9950 flowrate. before splitting, flowrate is 80000
                double rawFlowRate = pipe.ComputeFlowRate( potentialFrom, potentialTo );

                double maxSourceFlow = double.MaxValue;
                double maxSinkFlow = double.MaxValue;

                bool isForward = rawFlowRate > 0;
                IResourceProducer source = isForward ? pipe.FromInlet.Producer : pipe.ToInlet.Producer;
                IResourceConsumer sink = isForward ? pipe.ToInlet.Consumer : pipe.FromInlet.Consumer;

                // 2. Limit flow so it doesn't fill/drain more than ~50% of the available volume in a single frame.
                const double MAX_FILL_DRAIN_FACTOR = 0.5;

                if( source != null )
                {
                    maxSourceFlow = (source.GetAvailableOutflowVolume() * MAX_FILL_DRAIN_FACTOR) / dt;
                }
                if( sink != null )
                {
                    maxSinkFlow = (sink.GetAvailableInflowVolume( dt ) * MAX_FILL_DRAIN_FACTOR) / dt;
                }

                // 3. Apply the limit, preserving the sign.
                double maxFlow = Math.Min( maxSourceFlow, maxSinkFlow );
                if( Math.Abs( rawFlowRate ) > maxFlow )
                {
                    rawFlowRate = Math.Sign( rawFlowRate ) * maxFlow;
                }

                _nextFlowRates[i] = rawFlowRate;
            }
        }

        private bool CheckConvergenceAndApplyRelaxation( out bool hasOscillations )
        {
            const double OSCILLATION_RELAXATION = 0.2;
            const double RELAXATION_RECOVERY = 1.03;
            const double REL_TOLERANCE = 0.5; // Tuning the tolerances can change the convergence value, not just speed of convergence.
            const double ABS_TOLERANCE = 0.1;

            bool converged = true;
            hasOscillations = false;

            for( int i = 0; i < _pipes.Count; i++ )
            {
                FlowPipe pipe = _pipes[i];
                if( pipe == null )
                    continue;

                double prevFlow = _currentFlowRates[i];
                double startOfStepFlow = _flowRatesLastStep[i];
                double newFlowUnrelaxed = _nextFlowRates[i];

                double localRelaxation = _relaxationFactor;

                // Reactive Learned Damping (History-Based)
                double learnedFactor = _pipeLearnedRelaxationFactors[i];

                // 1. Oscillation detection (sign flipping in subsequent steps).
                bool isOscillating = (newFlowUnrelaxed * startOfStepFlow < -1e-10) || (prevFlow * startOfStepFlow < -1e-10);
                if( isOscillating )
                {
                    //Debug.LogWarning( "OSCILLATION" );
                    hasOscillations = true;
                    // Aggressively damp this specific pipe
                    learnedFactor = Math.Max( 0.01, learnedFactor * OSCILLATION_RELAXATION );
                }
#warning TODO - don't relax within the same step? (this causes slower convergence, because the current relaxed flow limit keeps moving relative to the previous flow)
                else
                {
                    //Debug.Log( "recovery" );
                    // Slowly recover if stable
                    learnedFactor = Math.Min( 1.0, learnedFactor * RELAXATION_RECOVERY );
                }
                _pipeLearnedRelaxationFactors[i] = learnedFactor;

                localRelaxation = Math.Min( localRelaxation, learnedFactor );

                // Proactive Stiffness Damping
                const double STIFFNESS_DAMPING_K = 1e-6; // Tuning constant
                double stiffnessA = 0.0;
                double stiffnessB = 0.0;

                var componentA = pipe.FromInlet.Consumer ?? (object)pipe.FromInlet.Producer;
                if( componentA is IResourceConsumer consumerA && _consumerToIndex.TryGetValue( consumerA, out int ca ) )
                    stiffnessA = _consumerStiffness[ca];
                else if( componentA is IResourceProducer producerA && _producerToIndex.TryGetValue( producerA, out int pa ) )
                    stiffnessA = _producerStiffness[pa];

                var componentB = pipe.ToInlet.Consumer ?? (object)pipe.ToInlet.Producer;
                if( componentB is IResourceConsumer consumerB && _consumerToIndex.TryGetValue( consumerB, out int cb ) )
                    stiffnessB = _consumerStiffness[cb];
                else if( componentB is IResourceProducer producerB && _producerToIndex.TryGetValue( producerB, out int pb ) )
                    stiffnessB = _producerStiffness[pb];

                double totalStiffness = stiffnessA + stiffnessB;
                if( totalStiffness > 1e-9 )
                {
                    double proactiveDamping = 1.0 / (1.0 + STIFFNESS_DAMPING_K * pipe.Conductance * totalStiffness);
                    localRelaxation = Math.Min( localRelaxation, proactiveDamping );
                }

                // 2. Apply relaxation and check convergence.
                double relaxedFlow;
                if( isOscillating )
                {
                    // Not using prevFlow on oscillations prevents the sign of the flowrate from flipping and the solver getting "latched" on the wrong sign.
                    relaxedFlow = newFlowUnrelaxed * localRelaxation;
                }
                else
                {
                    // Standard relaxation from the previous iteration's value.
                    relaxedFlow = prevFlow + (newFlowUnrelaxed - prevFlow) * localRelaxation;
                }
                double flowDifference = Math.Abs( relaxedFlow - prevFlow );
                double flowScale = Math.Max( Math.Abs( relaxedFlow ), Math.Abs( prevFlow ) );
                if( flowDifference > ABS_TOLERANCE && flowDifference > flowScale * REL_TOLERANCE )
                {
                    converged = false;
                }

                _currentFlowRates[i] = relaxedFlow;
            }

            return converged;
        }

        private void ApplyTransport( float dt )
        {
            // Clear previous IO states
            foreach( var producer in _producers )
            {
                if( producer == null )
                    continue;
                producer.Outflow?.Clear();
            }
            foreach( var consumer in _consumers )
            {
                if( consumer == null )
                    continue;
                consumer.Inflow?.Clear();
            }

            // 1. Calculate available supply/demand values for each producer/consumer.
            _consumerVolumeDemand.Clear();
            _producerVolumeSupply.Clear();
            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null )
                    continue;

                double proposedFlowrate = _currentFlowRates[i];
                if( Math.Abs( proposedFlowrate ) < 1e-9 )
                    continue;

                double volume = Math.Abs( proposedFlowrate ) * dt;

                object sourceObj = (proposedFlowrate > 0) ? _pipes[i].FromInlet.Producer : _pipes[i].ToInlet.Producer;
                if( sourceObj is IResourceProducer producer )
                {
                    if( !_producerVolumeSupply.ContainsKey( producer ) )
                        _producerVolumeSupply[producer] = 0;
                    _producerVolumeSupply[producer] += volume;
                }

                object sinkObj = (proposedFlowrate > 0) ? _pipes[i].ToInlet.Consumer : _pipes[i].FromInlet.Consumer;
                if( sinkObj is IResourceConsumer consumer )
                {
                    if( !_consumerVolumeDemand.ContainsKey( consumer ) )
                        _consumerVolumeDemand[consumer] = 0;
                    _consumerVolumeDemand[consumer] += volume;
                }
            }

            // 2. Calculate scaling factors for producers and consumers that are over capacity.
            _producerScalingFactors.Clear();
            _consumerScalingFactors.Clear();
            foreach( var kvp in _producerVolumeSupply )
            {
                IResourceProducer producer = kvp.Key;
                double totalSupply = kvp.Value;
                double availableSupply = producer.GetAvailableOutflowVolume();
                if( totalSupply > availableSupply && totalSupply > 0 )
                {
                    _producerScalingFactors[producer] = availableSupply / totalSupply;
                }
            }
            foreach( var kvp in _consumerVolumeDemand )
            {
                IResourceConsumer consumer = kvp.Key;
                double totalDemand = kvp.Value;
                double availableCapacity = consumer.GetAvailableInflowVolume( dt );
                if( totalDemand > availableCapacity && totalDemand > 0 )
                {
                    _consumerScalingFactors[consumer] = availableCapacity / totalDemand;
                }
            }

            // 3. Calculate per-pipe flow scaling factors based on producer/consumer limits of each pipe.
            if( _pipeFlowScalingFactors.Length < _pipes.Count )
            {
                Array.Resize( ref _pipeFlowScalingFactors, _pipes.Count );
            }
            for( int i = 0; i < _pipes.Count; i++ )
            {
#warning TODO - optimize - add a flag and only recalc if the _producerScalingFactors / _consumerScalingFactors are non-1.
                _pipeFlowScalingFactors[i] = 1.0;
                if( _pipes[i] == null )
                    continue;

                double rate = _currentFlowRates[i];
                if( Math.Abs( rate ) < 1e-9 )
                    continue;

                IResourceProducer source = (rate > 0) ? _pipes[i].FromInlet.Producer : _pipes[i].ToInlet.Producer;
                IResourceConsumer sink = (rate > 0) ? _pipes[i].ToInlet.Consumer : _pipes[i].FromInlet.Consumer;

                if( _producerScalingFactors.TryGetValue( source, out double pScale ) )
                {
                    _pipeFlowScalingFactors[i] = Math.Min( _pipeFlowScalingFactors[i], pScale );
                }
                if( _consumerScalingFactors.TryGetValue( sink, out double cScale ) )
                {
                    _pipeFlowScalingFactors[i] = Math.Min( _pipeFlowScalingFactors[i], cScale );
                }
            }

            // 4. Finally, actually apply the trannsport.
            // The tank might still do internal clamping, but at least we aren't asking for more than what is available across all pipes.
            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null )
                    continue;

                double rawRate = _currentFlowRates[i];
                if( Math.Abs( rawRate ) < 1e-9 )
                    continue;

                // Apply the scaling factor we calculated
                double finalRate = rawRate * _pipeFlowScalingFactors[i];

                FlowPipe pipe = _pipes[i];
                bool flowForward = finalRate > 0;

                IResourceProducer source = flowForward ? pipe.FromInlet.Producer : pipe.ToInlet.Producer;
                IResourceConsumer sink = flowForward ? pipe.ToInlet.Consumer : pipe.FromInlet.Consumer;

                if( source == null || sink == null )
                    continue;

                //Debug.LogWarning( "rate " + finalRate + " : " + _currentFlowRates[i] );

                using var flowResources = pipe.SampleFlowResources( finalRate, dt );
                if( flowResources.IsEmpty() )
                    continue;

                source.Outflow?.Add( flowResources, 1.0 );
                sink.Inflow?.Add( flowResources, 1.0 );
            }
        }
    }
}