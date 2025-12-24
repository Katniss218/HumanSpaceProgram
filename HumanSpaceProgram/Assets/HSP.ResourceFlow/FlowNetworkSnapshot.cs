using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a snapshot of the fluid network at a given time. This class contains the core solver logic. <br/>
    /// The solver is iterative, and uses potential-based flow and under-relaxation for stability.
    /// </summary>
    public sealed class FlowNetworkSnapshot : IDisposable
    {
        /// <summary>
        /// Represents the data for a single pipe required by the solver. This is a blittable struct intended to be used in a NativeArray for the C# Job System.
        /// </summary>
        private struct PipeJobData
        {
            // Topology
            public int End1_ProducerIndex;      // -1 if this end is not a producer
            public int End1_ConsumerIndex;      // -1 if this end is not a consumer
            public int End2_ProducerIndex;      // -1 if this end is not a producer
            public int End2_ConsumerIndex;      // -1 if this end is not a consumer

            public double PotentialEnd1;
            public double PotentialEnd2;

            // Static properties
            public double Length;
            public double Diameter;
            public double HeadAdded; // From pumps, etc.

            // Input for conductance job (Lagged state)
            public double MassFlowRateLastStep;
            public double LearnedRelaxationFactor; // For reactive damping. Persists across frames.
            public double ConductanceLastStep;     // For smoothing

            // Output of conductance job, input for solve job
            public double MassFlowConductance;

            // Properties for conductance job
            public bool IsGas;
            public double Density;
            public double Viscosity;
            public double SpeedOfSound;

            // Thermodynamic data
            public double JouleThomsonCoefficient; // K/Pa. For J-T effect calculation.
        }

        /// <summary>
        /// Represents the data for a single node (producer/consumer) required by the solver.
        /// This is a blittable struct for use in a NativeArray.
        /// </summary>
        private struct NodeJobData
        {
            public double Stiffness; // dPotential/dMass

            // Thermodynamic data
            public double Temperature; // K
            public double Pressure;    // Pa
            public double SpecificEnthalpy; // J/kg
        }

        public GameObject RootObject { get; }

        public IReadOnlyList<FlowPipe> Pipes => _pipes;

        public IReadOnlyList<IResourceProducer> Producers => _producers;

        public IReadOnlyList<IResourceConsumer> Consumers => _consumers;

        public double[] CurrentFlowRates => _finalFlowRatesMainThread;

        // Simulation objects
        private readonly IBuildsFlowNetwork[] _applyTo;
        private readonly List<FlowPipe> _pipes;
        private readonly List<IResourceProducer> _producers;
        private readonly List<IResourceConsumer> _consumers;
        private readonly List<object> _participants; // Unique producers and consumers (deduped).

        // Index mappings
        private readonly Dictionary<FlowPipe, int> _pipeToIndex = new();
        private readonly Dictionary<IResourceProducer, int> _producerToIndex = new();
        private readonly Dictionary<IResourceConsumer, int> _consumerToIndex = new();
        private readonly IReadOnlyDictionary<object, object> _owner;

        // Cached Topology for fast Marshalling (Stride = 4: End1_Prod, End1_Cons, End2_Prod, End2_Cons)
        private int[] _cachedPipeTopology;

        private readonly Queue<int> _freePipeSlots = new();
        private readonly Queue<int> _freeProducerSlots = new();
        private readonly Queue<int> _freeConsumerSlots = new();

        // Solver buffers (persistent)
        private NativeArray<PipeJobData> _pipeJobData;
        private NativeArray<NodeJobData> _producerJobData;
        private NativeArray<NodeJobData> _consumerJobData;
        private NativeArray<bool> _convergenceFlags; // For parallel convergence checking.
        private NativeArray<int> _oscillationFlag;   // For communicating oscillation back to main thread.

        // Solver intermediate/result buffers
        private NativeArray<double> _unrelaxedFlows;
        private NativeArray<double> _relaxedFlows;
        private double[] _finalFlowRatesMainThread;

        // Transport buffers (Flat arrays for O(1) access during Apply)
        private double[] _consumerVolumeDemandBuffer;
        private double[] _producerVolumeSupplyBuffer;
        private double[] _consumerScalingFactorsBuffer;
        private double[] _producerScalingFactorsBuffer;

        // Global solver state (NativeArrays of size 1 to allow job access)
        private NativeArray<double> _globalRelaxationFactor;
        private NativeArray<bool> _globalConverged;

        private double[] _pipeLearnedRelaxationFactors;

        private bool _disposed;

        public FlowNetworkSnapshot( GameObject rootObject, IReadOnlyDictionary<object, object> owner, IBuildsFlowNetwork[] flowBuilders, List<IResourceProducer> producers, List<IResourceConsumer> consumers, List<FlowPipe> pipes )
        {
            RootObject = rootObject;
            _owner = owner;
            _applyTo = flowBuilders;
            _producers = producers;
            _consumers = consumers;
            _pipes = pipes;

            _participants = new List<object>();
            _pipeLearnedRelaxationFactors = new double[_pipes.Count];
            _finalFlowRatesMainThread = new double[_pipes.Count];
            _cachedPipeTopology = new int[_pipes.Count * 4];

            _consumerVolumeDemandBuffer = new double[_consumers.Count];
            _consumerScalingFactorsBuffer = new double[_consumers.Count];
            _producerVolumeSupplyBuffer = new double[_producers.Count];
            _producerScalingFactorsBuffer = new double[_producers.Count];

            for( int i = 0; i < _pipes.Count; i++ )
            {
                _pipeLearnedRelaxationFactors[i] = 1.0;
                _pipeToIndex[_pipes[i]] = i;
            }

            for( int i = 0; i < _producers.Count; i++ )
                _producerToIndex[_producers[i]] = i;
            for( int i = 0; i < _consumers.Count; i++ )
                _consumerToIndex[_consumers[i]] = i;

            RebuildParticipantsList();
            RebuildCachedTopology();

            // Allocate NativeArrays
            _pipeJobData = new NativeArray<PipeJobData>( _pipes.Count, Allocator.Persistent );
            _producerJobData = new NativeArray<NodeJobData>( _producers.Count, Allocator.Persistent );
            _consumerJobData = new NativeArray<NodeJobData>( _consumers.Count, Allocator.Persistent );
            _convergenceFlags = new NativeArray<bool>( _pipes.Count, Allocator.Persistent );
            _unrelaxedFlows = new NativeArray<double>( _pipes.Count, Allocator.Persistent );
            _relaxedFlows = new NativeArray<double>( _pipes.Count, Allocator.Persistent );
            _oscillationFlag = new NativeArray<int>( _pipes.Count, Allocator.Persistent );

            // Global scalar state
            _globalRelaxationFactor = new NativeArray<double>( 1, Allocator.Persistent );
            _globalRelaxationFactor[0] = 0.7; // Initial guess
            _globalConverged = new NativeArray<bool>( 1, Allocator.Persistent );
        }

        public void Dispose()
        {
            if( _disposed )
                return;

            _pipeJobData.Dispose();
            _producerJobData.Dispose();
            _consumerJobData.Dispose();
            _convergenceFlags.Dispose();
            _unrelaxedFlows.Dispose();
            _relaxedFlows.Dispose();
            _oscillationFlag.Dispose();
            _globalRelaxationFactor.Dispose();
            _globalConverged.Dispose();

            _disposed = true;
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

            int count = Math.Min( savedRates.Length, _pipes.Count );
            for( int i = 0; i < count; i++ )
            {
                if( _pipes[i] == null )
                                    continue;
                
                double flowRate = savedRates[i];

                // Set main thread buffers for immediate use and for the next iteration.
                _finalFlowRatesMainThread[i] = flowRate;
                _relaxedFlows[i] = flowRate;

                // Update the C# object state. This is the source of truth that will be
                // marshalled into the job data at the start of the next solve step.
                _pipes[i].MassFlowRateLastStep = flowRate;
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
            foreach( IBuildsFlowNetwork builder in _applyTo )
            {
                if( !builder.IsValid( null ) ) // The refactored snapshot doesn't need to be passed here
                {
                    invalidComponents.Add( builder );
                }
            }
        }

        public void ApplyTransaction( FlowNetworkBuilder networkChangeTransaction )
        {
            bool structureChanged = false;

            // 1. Process Pipe Removals
            foreach( FlowPipe pipe in networkChangeTransaction.PipeRemovals )
            {
                if( _pipeToIndex.TryGetValue( pipe, out int pipeIndex ) )
                {
                    _pipes[pipeIndex] = null;
                    _pipeToIndex.Remove( pipe );
                    _freePipeSlots.Enqueue( pipeIndex );
                    structureChanged = true;

                    // BUG FIX 2: Zero out the corresponding data in job arrays to prevent processing stale data.
                    if( pipeIndex < _pipeJobData.Length )
                        _pipeJobData[pipeIndex] = default;
                    if( pipeIndex < _unrelaxedFlows.Length )
                        _unrelaxedFlows[pipeIndex] = 0;
                    if( pipeIndex < _relaxedFlows.Length )
                        _relaxedFlows[pipeIndex] = 0;
                    if( pipeIndex < _convergenceFlags.Length )
                        _convergenceFlags[pipeIndex] = true; // Mark as converged so it doesn't block global convergence
                    if( pipeIndex < _finalFlowRatesMainThread.Length )
                        _finalFlowRatesMainThread[pipeIndex] = 0;
                    if( pipeIndex < _pipeLearnedRelaxationFactors.Length )
                        _pipeLearnedRelaxationFactors[pipeIndex] = 1.0; // Reset learned factor
                }
            }

            // 2. Process Producer Removals
            foreach( IResourceProducer producer in networkChangeTransaction.ProducerRemovals )
            {
                if( _producerToIndex.TryGetValue( producer, out int producerIndex ) )
                {
                    _producers[producerIndex] = null;
                    _producerToIndex.Remove( producer );
                    _freeProducerSlots.Enqueue( producerIndex );
                    structureChanged = true;
                }
            }

            // 3. Process Consumer Removals
            foreach( IResourceConsumer consumer in networkChangeTransaction.ConsumerRemovals )
            {
                if( _consumerToIndex.TryGetValue( consumer, out int consumerIndex ) )
                {
                    _consumers[consumerIndex] = null;
                    _consumerToIndex.Remove( consumer );
                    _freeConsumerSlots.Enqueue( consumerIndex );
                    structureChanged = true;
                }
            }

            // 4. Process Pipe Additions
            if( networkChangeTransaction.Pipes.Any() )
            {
                structureChanged = true;
                foreach( FlowPipe pipe in networkChangeTransaction.Pipes )
                {
                    if( _pipeToIndex.ContainsKey( pipe ) )
                        continue;


                    if( _freePipeSlots.TryDequeue( out int freeIndex ) )
                    {
                        _pipes[freeIndex] = pipe;
                        _pipeToIndex[pipe] = freeIndex;
                    }
                    else
                    {
                        int newIndex = _pipes.Count;
                        _pipes.Add( pipe );
                        _pipeToIndex[pipe] = newIndex;
                    }
                }
            }

            // 5. Process Producer Additions
            if( networkChangeTransaction.Producers.Any() )
            {
                structureChanged = true;
                foreach( IResourceProducer producer in networkChangeTransaction.Producers )
                {
                    if( _producerToIndex.ContainsKey( producer ) )
                        continue;

                    if( _freeProducerSlots.TryDequeue( out int freeIndex ) )
                    {
                        _producers[freeIndex] = producer;
                        _producerToIndex[producer] = freeIndex;
                    }
                    else
                    {
                        int newIndex = _producers.Count;
                        _producers.Add( producer );
                        _producerToIndex[producer] = newIndex;
                    }
                }
            }

            // 6. Process Consumer Additions
            if( networkChangeTransaction.Consumers.Any() )
            {
                structureChanged = true;
                foreach( IResourceConsumer consumer in networkChangeTransaction.Consumers )
                {
                    if( _consumerToIndex.ContainsKey( consumer ) )
                        continue;

                    if( _freeConsumerSlots.TryDequeue( out int freeIndex ) )
                    {
                        _consumers[freeIndex] = consumer;
                        _consumerToIndex[consumer] = freeIndex;
                    }
                    else
                    {
                        int newIndex = _consumers.Count;
                        _consumers.Add( consumer );
                        _consumerToIndex[consumer] = newIndex;
                    }
                }
            }

            // 7. Handle Resize
            if( structureChanged )
            {
                RebuildParticipantsList();

                // Rebuild the cached topology indices. 
                // This array must resize if the pipe list grows.
                if( _cachedPipeTopology.Length < _pipes.Count * 4 )
                                    Array.Resize( ref _cachedPipeTopology, _pipes.Count * 4 );
                
                RebuildCachedTopology();

                // Resize NativeArrays if capacity has increased.
                ResizeNativeArray( ref _pipeJobData, _pipes.Count );
                ResizeNativeArray( ref _producerJobData, _producers.Count );
                ResizeNativeArray( ref _consumerJobData, _consumers.Count );
                ResizeNativeArray( ref _convergenceFlags, _pipes.Count );
                ResizeNativeArray( ref _unrelaxedFlows, _pipes.Count );
                ResizeNativeArray( ref _relaxedFlows, _pipes.Count );
                ResizeNativeArray( ref _oscillationFlag, _pipes.Count );

                // Resize main thread arrays
                if( _finalFlowRatesMainThread.Length < _pipes.Count )
                {
                    Array.Resize( ref _finalFlowRatesMainThread, _pipes.Count );
                }

                // Resize Transport Buffers if needed
                if( _consumerVolumeDemandBuffer.Length < _consumers.Count )
                {
                    Array.Resize( ref _consumerVolumeDemandBuffer, _consumers.Count );
                    Array.Resize( ref _consumerScalingFactorsBuffer, _consumers.Count );
                }
                if( _producerVolumeSupplyBuffer.Length < _producers.Count )
                {
                    Array.Resize( ref _producerVolumeSupplyBuffer, _producers.Count );
                    Array.Resize( ref _producerScalingFactorsBuffer, _producers.Count );
                }

                // BUG FIX 1: Explicitly initialize new elements of the learned relaxation factor array to 1.0.
                int oldLearnedFactorsLength = _pipeLearnedRelaxationFactors.Length;
                if( oldLearnedFactorsLength < _pipes.Count )
                {
                    Array.Resize( ref _pipeLearnedRelaxationFactors, _pipes.Count );
                    for( int i = oldLearnedFactorsLength; i < _pipes.Count; i++ )
                    {
                        _pipeLearnedRelaxationFactors[i] = 1.0;
                    }
                }
            }
        }

        private void ResizeNativeArray<T>( ref NativeArray<T> array, int requiredCapacity ) where T : struct
        {
            if( array.IsCreated && array.Length >= requiredCapacity )
                return;

            int newCapacity = (array.IsCreated) ? Math.Max( requiredCapacity, array.Length * 2 ) : requiredCapacity;
            NativeArray<T> newArray = new( newCapacity, Allocator.Persistent );

            if( array.IsCreated )
            {
                NativeArray<T>.Copy( array, newArray, array.Length );
                array.Dispose();
            }

            array = newArray;
        }

        /// <summary>
        /// Phase 1 of the simulation step. Gathers state and performs all heavy computation.
        /// This is an internally-blocking method that should be called from a manager's `FixedUpdateStart` phase.
        /// </summary>
        public void PrepareAndSolve( float deltaTime )
        {
            PreSolveSynchronization( deltaTime );

            Solve( deltaTime );
        }

        /// <summary>
        /// Phase 2 of the simulation step. Applies the results of the completed solve back to the game world.
        /// This should be called from a manager's `FixedUpdateEnd` phase.
        /// </summary>
        public void ApplyResults( float deltaTime )
        {
            TransportMass( deltaTime );

            PostSolveUpdates( deltaTime );
        }

        private void PreSolveSynchronization( double deltaTime )
        {
            foreach( IBuildsFlowNetwork builder in _applyTo )
            {
                builder.SynchronizeState( this );
            }

            foreach( object participant in _participants )
            {
                if( participant is IResourceProducer resourceProducer )
                    resourceProducer.PreSolveUpdate( deltaTime );
                else if( participant is IResourceConsumer resourceConsumer )
                    resourceConsumer.PreSolveUpdate( deltaTime );
            }
        }

        private void Solve( float deltaTime )
        {
            // 1. Marshalling
            // Prepare data from the managed heap into native arrays for the job system.
            InitializeAndMarshall( deltaTime );

            int pipeCount = _pipes.Count;
            if( pipeCount == 0 )
            {
                FinishAndDeMarshall( true );
                return;
            }

            // 2. Schedule the monolithic solver job
            // We use a single job to avoid the massive overhead of scheduling 100+ small jobs per frame
            // which was causing significant performance regression in the Editor.
            NetworkSolveJob solveJob = new NetworkSolveJob()
            {
                PipeCount = pipeCount,
                Pipes = _pipeJobData,
                Producers = _producerJobData,
                Consumers = _consumerJobData,
                UnrelaxedFlows = _unrelaxedFlows,
                RelaxedFlows = _relaxedFlows,
                ConvergenceFlags = _convergenceFlags,
                OscillationFlag = _oscillationFlag,
                GlobalRelaxationFactor = _globalRelaxationFactor,
                GlobalConverged = _globalConverged
            };

            JobHandle handle = solveJob.Schedule();

            // 3. Block Main Thread ONCE
            handle.Complete();

            // 4. De-marshalling & Finishing
            bool converged = _globalConverged[0];
            FinishAndDeMarshall( converged );
        }

        private void InitializeAndMarshall( double deltaTime )
        {
            // 1. Marshall Producer data
            // We only need stiffness and thermodynamic properties.
            // Potential/Conductance properties are calculated per-pipe because they depend on the specific connection point.
            for( int i = 0; i < _producers.Count; i++ )
            {
                IResourceProducer producer = _producers[i];
                if( producer == null )
                    continue;

                FluidState state = producer.Sample( Vector3.zero, 0.1f ); // Position/area don't affect stiffness.
                _producerJobData[i] = new NodeJobData()
                {
                    Stiffness = (producer is IStiffnessProvider stiffnessProvider) ? stiffnessProvider.GetPotentialDerivativeWrtVolume() : 0.0,
                    Temperature = state.Temperature,
                    Pressure = state.Pressure,
                };
            }

            // 2. Marshall Consumer data
            for( int i = 0; i < _consumers.Count; i++ )
            {
                IResourceConsumer consumer = _consumers[i];
                if( consumer == null )
                    continue;

                FluidState state = consumer.Sample( Vector3.zero, 0.1f );
                _consumerJobData[i] = new NodeJobData()
                {
                    Stiffness = (consumer is IStiffnessProvider stiffnessProvider) ? stiffnessProvider.GetPotentialDerivativeWrtVolume() : 0.0,
                    Temperature = state.Temperature,
                    Pressure = state.Pressure
                };
            }

            // 3. Marshall Pipe data
            // This includes calculating the potential at each end and the fluid properties needed for conductance.
            for( int i = 0; i < _pipes.Count; i++ )
            {
                FlowPipe flowPipe = _pipes[i];
                if( flowPipe == null )
                    continue;

                // Calculate potentials (FIXES HYDROSTATICS).
                // Consumer potential overrides producer potential (if the two disagree, which they shouldn't but mods I guess).
                // Position needs to be the actual pipe end position for hydrostatics to work correctly.
                double potentialEnd1 = 0.0;
                double potentialEnd2 = 0.0;

                IResourceConsumer consumerEnd1 = flowPipe.FromInlet.Consumer;
                IResourceProducer producerEnd1 = flowPipe.FromInlet.Producer;
                if( consumerEnd1 != null )
                    potentialEnd1 = consumerEnd1.Sample( flowPipe.FromInlet.pos, flowPipe.FromInlet.area ).FluidSurfacePotential;
                else if( producerEnd1 != null )
                    potentialEnd1 = producerEnd1.Sample( flowPipe.FromInlet.pos, flowPipe.FromInlet.area ).FluidSurfacePotential;

                IResourceConsumer consumerEnd2 = flowPipe.ToInlet.Consumer;
                IResourceProducer producerEnd2 = flowPipe.ToInlet.Producer;
                if( consumerEnd2 != null )
                    potentialEnd2 = consumerEnd2.Sample( flowPipe.ToInlet.pos, flowPipe.ToInlet.area ).FluidSurfacePotential;
                else if( producerEnd2 != null )
                    potentialEnd2 = producerEnd2.Sample( flowPipe.ToInlet.pos, flowPipe.ToInlet.area ).FluidSurfacePotential;

                // Fallback for invalid potentials to prevent NaN propagation
                bool isPotential1Valid = !double.IsInfinity( potentialEnd1 ) && !double.IsNaN( potentialEnd1 );
                bool isPotential2Valid = !double.IsInfinity( potentialEnd2 ) && !double.IsNaN( potentialEnd2 );

                if( !isPotential1Valid && isPotential2Valid )
                {
                    potentialEnd1 = potentialEnd2;
                }
                else if( !isPotential2Valid && isPotential1Valid )
                {
                    potentialEnd2 = potentialEnd1;
                }
                else if( !isPotential1Valid && !isPotential2Valid )
                {
                    potentialEnd1 = 0;
                    potentialEnd2 = 0;
                }

                // Calculate properties for physical conductance (FIXES PROBE MASS).
                // Physical conductance depends on the actual fluid in the pipe (density/viscosity).
                // Flows will usually be stable(-ish), so just look at the stored flow direction from the previous frame.
                double density = 0;
                double viscosity = 0;
                double speedOfSound = 1500;
                bool isGas = false;

                bool flowFromEnd1 = flowPipe.MassFlowRateLastStep >= 0; // if true then flow: End1 -> End2
                if( Math.Abs( flowPipe.MassFlowRateLastStep ) < 1e-9 )
                {
                    // If no previous flow, guess based on potential gradient.
                    flowFromEnd1 = (potentialEnd1 - potentialEnd2) >= 0;
                }

                FlowPipe.Port sourcePort = flowFromEnd1 ? flowPipe.FromInlet : flowPipe.ToInlet;
                FlowPipe.Port sinkPort = flowFromEnd1 ? flowPipe.ToInlet : flowPipe.FromInlet;
                IResourceProducer sourceProducer = sourcePort.Producer;

                if( sourceProducer != null )
                {
                    FluidState sourceState = sourceProducer.Sample( sourcePort.pos, sourcePort.area );

                    // Check what substances will flow *in this step* based on the last known flow rate.
                    double sampleMass = Math.Abs( flowPipe.MassFlowRateLastStep * deltaTime );
                    // Use a reasonable fallback mass flowrate if the last step was zero (on init).
                    // This can result in differences in actual flow contents in stratified tanks, but it's close enough.
                    if( sampleMass < 1e-5 )
                        sampleMass = 1.0;

                    using ISampledSubstanceStateCollection substances = sourceProducer.SampleSubstances( sourcePort.pos, sampleMass );
                    if( !substances.IsEmpty() )
                    {
                        isGas = substances.IsSinglePhase( SubstancePhase.Gas );
                        density = substances.GetAverageDensity( sourceState.Temperature, sourceState.Pressure );
                        viscosity = substances.GetAverageViscosity( sourceState.Temperature, sourceState.Pressure );
                        speedOfSound = isGas ? substances.GetAverageSpeedOfSound( sourceState.Temperature, sourceState.Pressure ) : 1500;
                    }
                }

                // Fallbacks if sampling failed or returned nothing.
                if( density <= 1e-9 )
                    density = flowPipe.DensityLastStep > 1e-9 ? flowPipe.DensityLastStep : 1000.0;
                if( viscosity <= 1e-9 )
                    viscosity = flowPipe.ViscosityLastStep > 1e-9 ? flowPipe.ViscosityLastStep : 0.001;

                // Property blending for near-zero flow to smooth transitions during reversal.
                if( Math.Abs( flowPipe.MassFlowRateLastStep ) < (0.01 * flowPipe.Diameter) && sinkPort.Producer != null )
                {
                    FluidState sinkState = sinkPort.Producer.Sample( sinkPort.pos, sinkPort.area );
                    double sampleMass = Math.Abs( flowPipe.MassFlowRateLastStep * deltaTime ) > 1e-9 ? Math.Abs( flowPipe.MassFlowRateLastStep * deltaTime ) : 1.0;

                    using ISampledSubstanceStateCollection sinkSubstances = sinkPort.Producer.SampleSubstances( sinkPort.pos, sampleMass );
                    if( !sinkSubstances.IsEmpty() )
                    {
                        const double blendFactor = 0.5;
                        density = (1 - blendFactor) * density + blendFactor * sinkSubstances.GetAverageDensity( sinkState.Temperature, sinkState.Pressure );
                        viscosity = (1 - blendFactor) * viscosity + blendFactor * sinkSubstances.GetAverageViscosity( sinkState.Temperature, sinkState.Pressure );
                    }
                }

                // Populate PipeJobData
                // Use cached indices instead of dictionary lookups
                int baseIndex = i * 4;
                int producerIndexEnd1 = _cachedPipeTopology[baseIndex];
                int consumerIndexEnd1 = _cachedPipeTopology[baseIndex + 1];
                int producerIndexEnd2 = _cachedPipeTopology[baseIndex + 2];
                int consumerIndexEnd2 = _cachedPipeTopology[baseIndex + 3];

                _pipeJobData[i] = new PipeJobData()
                {
                    End1_ProducerIndex = producerIndexEnd1,
                    End1_ConsumerIndex = consumerIndexEnd1,
                    End2_ProducerIndex = producerIndexEnd2,
                    End2_ConsumerIndex = consumerIndexEnd2,

                    PotentialEnd1 = potentialEnd1,
                    PotentialEnd2 = potentialEnd2,

                    Length = flowPipe.Length,
                    Diameter = flowPipe.Diameter,

                    MassFlowRateLastStep = flowPipe.MassFlowRateLastStep,
                    LearnedRelaxationFactor = _pipeLearnedRelaxationFactors[i],
                    ConductanceLastStep = flowPipe.ConductanceLastStep,

                    Density = density,
                    Viscosity = viscosity,
                    IsGas = isGas,
                    SpeedOfSound = speedOfSound,

                    HeadAdded = flowPipe.HeadAdded
                };
            }
        }

        private void FinishAndDeMarshall( bool converged )
        {
            // SUCCESS: Copy the results from the job's output buffer to the main-thread buffer for transport.
            if( converged )
            {
                _relaxedFlows.CopyTo( _finalFlowRatesMainThread );
            }
            // FAILURE: Revert to the known-stable flow rates from the previous frame.
            //   This prevents the simulation from exploding if the solver diverges.
            else
            {
                for( int i = 0; i < _pipes.Count; i++ )
                {
                    if( _pipes[i] != null )
                    {
                        _finalFlowRatesMainThread[i] = _pipeJobData[i].MassFlowRateLastStep;
                    }
                }
            }
        }

        private void TransportMass( double deltaTime )
        {
            // 1. Clear previous frame's inflow/outflow buffers.
            foreach( IResourceProducer producer in _producers )
            {
                if( producer != null )
                    producer.Outflow?.Clear();
            }
            foreach( IResourceConsumer consumer in _consumers )
            {
                if( consumer != null )
                    consumer.Inflow?.Clear();
            }

            // Clear optimization buffers (reusing arrays instead of creating new dicts)
            Array.Clear( _consumerVolumeDemandBuffer, 0, _consumers.Count );
            Array.Clear( _producerVolumeSupplyBuffer, 0, _producers.Count );

            // 2. Calculate requested volumes.
            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null )
                    continue;

                double proposedMassFlow = _finalFlowRatesMainThread[i];
                if( Math.Abs( proposedMassFlow ) < 1e-9 )
                    continue;

                PipeJobData pipeData = _pipeJobData[i]; // Use job data for density, cached from Marshalling
                double density = pipeData.Density;
                if( density < 1e-9 )
                    continue;

                double volume = Math.Abs( proposedMassFlow * deltaTime ) / density;

                // Use the cached indices from pipeJobData to access the flat buffers
                bool isFlowForward = proposedMassFlow > 0;

                // Get Producer Index
                int producerIndex = isFlowForward ? pipeData.End1_ProducerIndex : pipeData.End2_ProducerIndex;
                if( producerIndex != -1 )
                {
                    _producerVolumeSupplyBuffer[producerIndex] += volume;
                }

                // Get Consumer Index
                int consumerIndex = isFlowForward ? pipeData.End2_ConsumerIndex : pipeData.End1_ConsumerIndex;
                if( consumerIndex != -1 )
                {
                    _consumerVolumeDemandBuffer[consumerIndex] += volume;
                }
            }

            // 3. Calculate scaling factors.
            for( int i = 0; i < _producers.Count; i++ )
            {
                IResourceProducer producer = _producers[i];
                if( producer == null )
                    continue;

                double volumeSupply = _producerVolumeSupplyBuffer[i];
                if( volumeSupply > 1e-9 )
                {
                    double available = producer.GetAvailableOutflowVolume();
                    _producerScalingFactorsBuffer[i] = (volumeSupply > available) ? available / volumeSupply : 1.0;
                }
                else
                {
                    _producerScalingFactorsBuffer[i] = 1.0;
                }
            }
            for( int i = 0; i < _consumers.Count; i++ )
            {
                IResourceConsumer consumer = _consumers[i];
                if( consumer == null )
                    continue;

                double volumeDemand = _consumerVolumeDemandBuffer[i];
                if( volumeDemand > 1e-9 )
                {
                    double available = consumer.GetAvailableInflowVolume( deltaTime );
                    _consumerScalingFactorsBuffer[i] = (volumeDemand > available) ? available / volumeDemand : 1.0;
                }
                else
                {
                    _consumerScalingFactorsBuffer[i] = 1.0;
                }
            }

            // 4. Apply final transport.
            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] == null )
                    continue;

                double rawMassFlow = _finalFlowRatesMainThread[i];
                if( Math.Abs( rawMassFlow ) < 1e-9 )
                    continue;

                PipeJobData pipeData = _pipeJobData[i];
                bool isFlowForward = rawMassFlow > 0;

                int producerIndex = isFlowForward ? pipeData.End1_ProducerIndex : pipeData.End2_ProducerIndex;
                int consumerIndex = isFlowForward ? pipeData.End2_ConsumerIndex : pipeData.End1_ConsumerIndex;

                double producerScale = (producerIndex != -1) ? _producerScalingFactorsBuffer[producerIndex] : 1.0;
                double consumerScale = (consumerIndex != -1) ? _consumerScalingFactorsBuffer[consumerIndex] : 1.0;

                double finalMassFlowRate = rawMassFlow * Math.Min( producerScale, consumerScale );

                if( Math.Abs( finalMassFlowRate ) < 1e-9 )
                    continue;

                // Resolve objects for final transfer
                IResourceProducer source = (producerIndex != -1) ? _producers[producerIndex] : null;
                IResourceConsumer sink = (consumerIndex != -1) ? _consumers[consumerIndex] : null;

                if( source != null && sink != null )
                {
                    using ISampledSubstanceStateCollection resources = _pipes[i].SampleFlowResources( finalMassFlowRate, deltaTime );
                    source.Outflow?.Add( resources, 1.0 );
                    sink.Inflow?.Add( resources, 1.0 );
                }
            }
        }

        private void PostSolveUpdates( double deltaTime )
        {
            // 1. Notify simulation objects (Tanks, etc.) to process the inflows/outflows we just calculated.
            foreach( object participant in _participants )
            {
                if( participant is IResourceProducer resourceProducer )
                    resourceProducer.ApplySolveResults( deltaTime );
                else if( participant is IResourceConsumer resourceConsumer )
                    resourceConsumer.ApplySolveResults( deltaTime );
            }

            // 2. Notify Unity components to pull the new state from the simulation objects.
            foreach( IBuildsFlowNetwork builder in _applyTo )
            {
                builder.ApplySnapshot( this ); // Pass null as snapshot is not needed here in refactor
            }

            // 3. Save the results as the starting point for the next frame.
            CacheFinalFlowRates();
        }

        private void CacheFinalFlowRates()
        {
            // This is where the results from the current frame's solve are persisted back onto the
            // main `FlowPipe` C# objects and persistent snapshot buffers, becoming the lagged
            // properties for the *next* frame.
            for( int i = 0; i < _pipes.Count; i++ )
            {
                if( _pipes[i] != null )
                {
                    PipeJobData pipeData = _pipeJobData[i]; // get the job data struct

                    // Cache solved flow rate for next frame's conductance calculation.
                    _pipes[i].MassFlowRateLastStep = _finalFlowRatesMainThread[i];

                    // DE-MARSHALL: Persist the updated learned relaxation factor for the next frame.
                    _pipeLearnedRelaxationFactors[i] = pipeData.LearnedRelaxationFactor;

                    // Also de-marshall the last used conductance for smoothing.
                    _pipes[i].ConductanceLastStep = pipeData.MassFlowConductance;

                    // Persist the physical properties used in this frame's calculation
                    _pipes[i].DensityLastStep = pipeData.Density;
                    _pipes[i].ViscosityLastStep = pipeData.Viscosity;
                }
            }
        }

        private void RebuildParticipantsList()
        {
            _participants.Clear();
            HashSet<object> uniqueParticipants = new();

            foreach( IResourceProducer p in _producers )
            {
                if( p != null )
                    uniqueParticipants.Add( p );
            }
            foreach( IResourceConsumer c in _consumers )
            {
                if( c != null )
                    uniqueParticipants.Add( c );
            }

            _participants.AddRange( uniqueParticipants );
        }

        private void RebuildCachedTopology()
        {
            // Populate the topology cache for all pipes.
            // This happens only when network structure changes, making per-frame marshalling fast.
            for( int i = 0; i < _pipes.Count; i++ )
            {
                FlowPipe flowPipe = _pipes[i];
                if( flowPipe == null )
                    continue;

                int baseIndex = i * 4;
                int producerIndexEnd1 = -1;
                int consumerIndexEnd1 = -1;
                int producerIndexEnd2 = -1;
                int consumerIndexEnd2 = -1;

                if( flowPipe.FromInlet.Producer != null )
                    _producerToIndex.TryGetValue( flowPipe.FromInlet.Producer, out producerIndexEnd1 );
                if( flowPipe.FromInlet.Consumer != null )
                    _consumerToIndex.TryGetValue( flowPipe.FromInlet.Consumer, out consumerIndexEnd1 );
                if( flowPipe.ToInlet.Producer != null )
                    _producerToIndex.TryGetValue( flowPipe.ToInlet.Producer, out producerIndexEnd2 );
                if( flowPipe.ToInlet.Consumer != null )
                    _consumerToIndex.TryGetValue( flowPipe.ToInlet.Consumer, out consumerIndexEnd2 );

                _cachedPipeTopology[baseIndex] = producerIndexEnd1;
                _cachedPipeTopology[baseIndex + 1] = consumerIndexEnd1;
                _cachedPipeTopology[baseIndex + 2] = producerIndexEnd2;
                _cachedPipeTopology[baseIndex + 3] = consumerIndexEnd2;
            }
        }

        #region Solver Kernels & Physics Module

        // For encapsulation, these can be private nested types within the snapshot.

        private static class FlowPhysics
        {
            public static void CalculateMassConductance( ref PipeJobData pipe )
            {
                const double ZERO_FLOW_TOLERANCE = 1e-9;

                double density = pipe.Density;
                double viscosity = pipe.Viscosity;
                double deltaPotential = pipe.PotentialEnd1 - pipe.PotentialEnd2;

                if( density <= ZERO_FLOW_TOLERANCE || viscosity <= ZERO_FLOW_TOLERANCE )
                {
                    pipe.MassFlowConductance = 0;
                    return;
                }

                double area = Math.PI * (pipe.Diameter / 2.0) * (pipe.Diameter / 2.0);
                double reynoldsNumber;

                // Determine Reynolds number.
                // If flow was zero last step, we use the potential difference to estimate an initial velocity.
                if( Math.Abs( pipe.MassFlowRateLastStep ) < ZERO_FLOW_TOLERANCE )
                {
                    // Frame 0 guess
                    double potentialVelocity = Math.Sqrt( 2 * Math.Abs( deltaPotential ) );
                    double potentialMassFlow = area * density * potentialVelocity;
                    reynoldsNumber = FlowEquations.GetReynoldsNumber( potentialMassFlow, pipe.Diameter, viscosity );
                }
                else
                {
                    reynoldsNumber = FlowEquations.GetReynoldsNumber( pipe.MassFlowRateLastStep, pipe.Diameter, viscosity );
                }

                double massConductance;
                if( reynoldsNumber < 2300 ) // Laminar
                {
                    massConductance = FlowEquations.GetLaminarMassConductance( density, area, pipe.Length, viscosity );
                }
                else if( reynoldsNumber > 4000 ) // Turbulent
                {
                    double frictionFactor = FlowEquations.GetDarcyFrictionFactor( reynoldsNumber );
                    massConductance = FlowEquations.GetTurbulentMassConductance( density, area, pipe.Diameter, pipe.Length, frictionFactor, pipe.MassFlowRateLastStep );
                }
                else // Transitional
                {
                    double laminarConductance = FlowEquations.GetLaminarMassConductance( density, area, pipe.Length, viscosity );
                    const double reTurbulentThreshold = 4000.01;
                    double frictionFactorTurbulent = FlowEquations.GetDarcyFrictionFactor( reTurbulentThreshold );
                    double turbulentMassFlow = reTurbulentThreshold * Math.PI * pipe.Diameter * viscosity / 4.0;
                    double turbulentConductance = FlowEquations.GetTurbulentMassConductance( density, area, pipe.Diameter, pipe.Length, frictionFactorTurbulent, turbulentMassFlow );

                    double t = (reynoldsNumber - 2300.0) / (4000.0 - 2300.0);
                    massConductance = (1.0 - t) * laminarConductance + t * turbulentConductance;
                }

                // Apply smoothing (Low-pass filter on conductance) to prevent jitter.
                double alpha = pipe.LearnedRelaxationFactor * pipe.LearnedRelaxationFactor;
                massConductance = (alpha * massConductance) + ((1 - alpha) * pipe.ConductanceLastStep);

                // Enforce choked flow limit for gases.
                if( pipe.IsGas && Math.Abs( deltaPotential ) > ZERO_FLOW_TOLERANCE )
                {
                    double maxMassFlow = FlowEquations.GetChokedMassFlow( density, area, pipe.SpeedOfSound );
                    double potentialMaxConductance = maxMassFlow / Math.Abs( deltaPotential );
                    if( massConductance > potentialMaxConductance )
                    {
                        massConductance = potentialMaxConductance;
                    }
                }

                pipe.MassFlowConductance = massConductance;
            }
        }

        [BurstCompile]
        private struct NetworkSolveJob : IJob
        {
            public int PipeCount;

            public NativeArray<PipeJobData> Pipes;
            [ReadOnly] public NativeArray<NodeJobData> Producers;
            [ReadOnly] public NativeArray<NodeJobData> Consumers;

            // Buffers
            public NativeArray<double> UnrelaxedFlows;
            public NativeArray<double> RelaxedFlows;
            public NativeArray<bool> ConvergenceFlags;
            public NativeArray<int> OscillationFlag;

            // Global State
            public NativeArray<double> GlobalRelaxationFactor;
            public NativeArray<bool> GlobalConverged;

            const double OSCILLATION_RELAXATION_MULTIPLIER = 0.2;
            const double RELAXATION_RECOVERY_MULTIPLIER = 1.03;
            const double STIFFNESS_DAMPING_K = 1e-6;
            const double REL_TOLERANCE = 0.5; // 50% relative change
            const double ABS_TOLERANCE = 0.1; // kg/s absolute change
            const double EPSILON = 1e-9;

            const int MAX_ITERATIONS = 20;

            public void Execute()
            {
                // 1. Initialize
                GlobalConverged[0] = false;
                for( int i = 0; i < PipeCount; i++ )
                {
                    RelaxedFlows[i] = Pipes[i].MassFlowRateLastStep;
                }

                // 2. Update Conductances
                for( int i = 0; i < PipeCount; i++ )
                {
                    var pipe = Pipes[i];
                    FlowPhysics.CalculateMassConductance( ref pipe );
                    Pipes[i] = pipe;
                }

                // 3. Calculate Unrelaxed Flows
                for( int i = 0; i < PipeCount; i++ )
                {
                    var pipe = Pipes[i];
                    double rawFlowRate = pipe.MassFlowConductance * (pipe.PotentialEnd1 - pipe.PotentialEnd2 + pipe.HeadAdded);
                    UnrelaxedFlows[i] = !double.IsFinite( rawFlowRate ) ? 0.0 : rawFlowRate;
                }

                // 4. Iterative Loop
                for( int iter = 0; iter < MAX_ITERATIONS; iter++ )
                {
                    bool allConverged = true;
                    bool hasOscillations = false;

                    double globalRelax = GlobalRelaxationFactor[0];

                    for( int i = 0; i < PipeCount; i++ )
                    {
                        // A. Apply Relaxation and Check Local Convergence
                        PipeJobData pipe = Pipes[i];

                        double flowRatePreviousIteration = RelaxedFlows[i];
                        double startOfStepFlow = pipe.MassFlowRateLastStep;
                        double unrelaxedTargetFlowRate = UnrelaxedFlows[i];

                        // 1. Reactive Damping (Learned Relaxation)
                        double learnedFactor = pipe.LearnedRelaxationFactor;

                        bool isOscillating = (unrelaxedTargetFlowRate * startOfStepFlow < -EPSILON) || (flowRatePreviousIteration * startOfStepFlow < -EPSILON);

                        if( isOscillating )
                        {
                            hasOscillations = true;
                            learnedFactor = Math.Max( 0.01, learnedFactor * OSCILLATION_RELAXATION_MULTIPLIER );
                        }
                        else
                        {
                            learnedFactor = Math.Min( 1.0, learnedFactor * RELAXATION_RECOVERY_MULTIPLIER );
                        }

                        pipe.LearnedRelaxationFactor = learnedFactor;

                        // 2. Proactive Damping (Stiffness-based)
                        double stiffnessEnd1 = 0.0;
                        if( pipe.End1_ConsumerIndex != -1 )
                            stiffnessEnd1 = Consumers[pipe.End1_ConsumerIndex].Stiffness;
                        else if( pipe.End1_ProducerIndex != -1 )
                            stiffnessEnd1 = Producers[pipe.End1_ProducerIndex].Stiffness;

                        double stiffnessEnd2 = 0.0;
                        if( pipe.End2_ConsumerIndex != -1 )
                            stiffnessEnd2 = Consumers[pipe.End2_ConsumerIndex].Stiffness;
                        else if( pipe.End2_ProducerIndex != -1 )
                            stiffnessEnd2 = Producers[pipe.End2_ProducerIndex].Stiffness;

                        double totalStiffness = stiffnessEnd1 + stiffnessEnd2;
                        double proactiveDamping = 1.0;
                        if( totalStiffness > EPSILON )
                        {
                            proactiveDamping = 1.0 / (1.0 + STIFFNESS_DAMPING_K * pipe.MassFlowConductance * totalStiffness);
                        }

                        // 3. Calculate local relaxation factor
                        double localRelaxation = Math.Min( globalRelax, learnedFactor );
                        localRelaxation = Math.Min( localRelaxation, proactiveDamping );

                        // 4. Apply Relaxation
                        double relaxedFlow;
                        if( isOscillating )
                            relaxedFlow = unrelaxedTargetFlowRate * localRelaxation;
                        else
                            relaxedFlow = flowRatePreviousIteration + (unrelaxedTargetFlowRate - flowRatePreviousIteration) * localRelaxation;

                        // 5. Check Convergence
                        double flowDifference = Math.Abs( relaxedFlow - flowRatePreviousIteration );
                        double flowScale = Math.Max( Math.Abs( relaxedFlow ), Math.Abs( flowRatePreviousIteration ) );
                        bool isConverged = flowDifference <= ABS_TOLERANCE || flowDifference <= flowScale * REL_TOLERANCE;

                        if( !isConverged ) allConverged = false;

                        // 6. Write back
                        RelaxedFlows[i] = relaxedFlow;
                        Pipes[i] = pipe;
                    }

                    // B. Post-Iteration Logic
                    if( allConverged )
                    {
                        GlobalConverged[0] = true;
                        break;
                    }

                    if( hasOscillations )
                        globalRelax = Math.Max( 0.1, globalRelax * 0.75 );
                    else
                        globalRelax = Math.Min( 1.0, globalRelax * 1.01 );

                    GlobalRelaxationFactor[0] = globalRelax;
                }
            }
        }

        #endregion
    }
}