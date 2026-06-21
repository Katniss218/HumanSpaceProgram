using HSP.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HSP.Trajectories
{
    /// <summary>
    /// A robust, drop-in replacement for TrajectorySimulator2 that solves structural coupling issues,
    /// tracking individual body simulation boundaries to prevent global ephemeris resets ("dragdown")
    /// and eliminating the "Can't insert in the middle of an ephemeris" InvalidOperationException.
    /// </summary>
    public sealed class TrajectorySimulator : IReadonlyTrajectorySimulator
    {
        [Flags]
        public enum SimulatedIntervalOptions
        {
            IncludeAttractors = 1 << 0,
            IncludeFollowers = 1 << 1,
            IncludeAttractorsAndFollowers = IncludeAttractors | IncludeFollowers
        }

        public enum SimulationDirection
        {
            Forward,
            Backward
        }

        /// <summary>
        /// Individual body entry within the simulation to isolate and encapsulate their 
        /// state, ephemeris, and validation intervals.
        /// </summary>
        private class BodySimulationAgent
        {
            public ITrajectoryTransform Transform;
            public Ephemeris Ephemeris;
            public ITrajectoryIntegrator Integrator;
            public ITrajectoryStepProvider[] AccelerationProviders;
            public TrajectoryStateVector CurrentState;
            public TrajectoryStateVector NextState;

            public double StartUT; // The precise UT at which CurrentState was sampled from the physics transform when the ephemeris was empty

            public bool IsAttractor;
            public int TimestepperIndex;
            public bool IsStale;
        }

        // --- Core Configuration and Public API ---

        public double MaxStepSize { get; set; } = 1.0;
        public double DefaultStepSize { get; set; } = 1.0;

        public ReadOnlySpan<ITrajectoryTransform> Attractors => _attractorCache;

        public int BodyCount => _bodies.Count;
        public int AttractorCount => _attractorCache?.Length ?? 0;
        public int FollowerCount => _followerCache?.Length ?? 0;
        public bool IsSimulating => _isSimulating;

        // --- Internal Fields ---

        private readonly Dictionary<ITrajectoryTransform, BodySimulationAgent> _bodies = new();
        private ITrajectoryTransform[] _attractorCache = Array.Empty<ITrajectoryTransform>();
        private ITrajectoryTransform[] _followerCache = Array.Empty<ITrajectoryTransform>();

        private readonly HashSet<ITrajectoryTransform> _staleExisting = new();
        private readonly HashSet<ITrajectoryTransform> _staleToAdd = new();
        private readonly HashSet<ITrajectoryTransform> _staleToRemove = new();

        private double _initialUT;
        private double _ephemerisMaxError = 0.02;
        private double _ephemerisDuration = 1000000;
        private SimulationDirection _direction = SimulationDirection.Forward;

        private bool _isStale = true;
        private bool _staleAttractorChanged = false;
        private volatile bool _isSimulating = false;
        private readonly object _simulationLock = new object();

        /// <summary>
        /// Special constructor mirroring TrajectorySimulator2 signatures for seamless replacement.
        /// </summary>
        public TrajectorySimulator( double step, int count )
        {
            this.DefaultStepSize = step;
            ResetToCurrent();
        }

        // --- Public Interface Methods ---

        /// <summary>
        /// Computes the interval where the ephemerides of all selected bodies are valid.
        /// </summary>
        public TimeInterval GetSimulatedInterval( SimulatedIntervalOptions options = SimulatedIntervalOptions.IncludeAttractorsAndFollowers )
        {
            if( _bodies.Count == 0 )
                return new TimeInterval( _initialUT );

            double lowUT = double.NegativeInfinity;
            double highUT = double.PositiveInfinity;
            bool foundAny = false;

            foreach( var kvp in _bodies )
            {
                var agent = kvp.Value;
                if( (agent.IsAttractor && !options.HasFlag( SimulatedIntervalOptions.IncludeAttractors )) ||
                    (!agent.IsAttractor && !options.HasFlag( SimulatedIntervalOptions.IncludeFollowers )) )
                    continue;

                foundAny = true;
                double agentLow, agentHigh;
                if( agent.Ephemeris.Count == 0 )
                {
                    agentLow = agent.StartUT;
                    agentHigh = agent.StartUT;
                }
                else
                {
                    agentLow = agent.Ephemeris.LowUT;
                    agentHigh = agent.Ephemeris.HighUT;
                }

                if( agentLow > lowUT ) lowUT = agentLow;
                if( agentHigh < highUT ) highUT = agentHigh;
            }

            if( !foundAny )
                return new TimeInterval( _initialUT );

            if( lowUT <= highUT )
                return new TimeInterval( lowUT, highUT );

            double fallback = lowUT != double.NegativeInfinity ? lowUT : _initialUT;
            return new TimeInterval( Math.Max( _initialUT, fallback ) );
        }

        /// <summary>
        /// Gets the timestepper index of the given attractor. Matches the expected IReadonlyTrajectorySimulator API.
        /// </summary>
        public int GetAttractorIndex( ITrajectoryTransform transform )
        {
            if( transform == null ) return -1;
            if( _bodies.TryGetValue( transform, out var agent ) && agent.IsAttractor )
                return agent.TimestepperIndex;
            return -1;
        }

        /// <summary>
        /// Sets the initial time origin for the simulation.
        /// </summary>
        public void SetInitialTime( double ut )
        {
            _initialUT = ut;
            ResetToCurrent();
        }

        /// <summary>
        /// Adjusts accuracy parameters for the adaptive ephemeris generators.
        /// </summary>
        public void SetEphemerisParameters( double maxError, double maxDuration, int initialCapacity )
        {
            _ephemerisMaxError = maxError;
            _ephemerisDuration = maxDuration;
            foreach( var kvp in _bodies )
            {
                var ephemeris = kvp.Value.Ephemeris;
                ephemeris.MaxError = _ephemerisMaxError;
                ephemeris.MaxDuration = _ephemerisDuration;
            }
        }

        public double GetHighUT( ITrajectoryTransform transform )
        {
            if( transform == null ) throw new ArgumentNullException( nameof( transform ) );
            if( _bodies.TryGetValue( transform, out var agent ) )
                return agent.Ephemeris.Count > 0 ? agent.Ephemeris.HighUT : agent.StartUT;
            return _initialUT;
        }

        public double GetLowUT( ITrajectoryTransform transform )
        {
            if( transform == null ) throw new ArgumentNullException( nameof( transform ) );
            if( _bodies.TryGetValue( transform, out var agent ) )
                return agent.Ephemeris.Count > 0 ? agent.Ephemeris.LowUT : agent.StartUT;
            return _initialUT;
        }

        public bool HasBody( ITrajectoryTransform transform )
        {
            if( transform == null ) return false;
            return (_bodies.ContainsKey( transform ) && !_staleToRemove.Contains( transform )) || _staleToAdd.Contains( transform );
        }

        public IEnumerable<(ITrajectoryTransform t, IReadonlyEphemeris e)> GetBodies()
        {
            FixStale();
            foreach( var kvp in _bodies )
            {
                yield return (kvp.Key, kvp.Value.Ephemeris);
            }
        }

        public bool TryGetBody( ITrajectoryTransform transform, out IReadonlyEphemeris ephemeris )
        {
            if( transform == null ) throw new ArgumentNullException( nameof( transform ) );
            FixStale();
            if( _bodies.TryGetValue( transform, out var agent ) )
            {
                ephemeris = agent.Ephemeris;
                return true;
            }
            ephemeris = null;
            return false;
        }

        public bool TryAddBody( ITrajectoryTransform transform )
        {
            if( transform == null ) return false;
            if( _staleToRemove.Contains( transform ) )
            {
                bool wasAttractor = _bodies.TryGetValue( transform, out var existingAgent ) && existingAgent.IsAttractor;
                if( wasAttractor != transform.IsAttractor )
                {
                    _staleAttractorChanged = true;
                    _staleToAdd.Add( transform );
                    _isStale = true;
                    return true;
                }
                _staleToRemove.Remove( transform );
                return true;
            }
            if( _bodies.ContainsKey( transform ) && !_staleToRemove.Contains( transform ) ) return false;
            if( _staleToAdd.Contains( transform ) ) return false;

            _staleToAdd.Add( transform );
            _isStale = true;
            if( transform.IsAttractor ) _staleAttractorChanged = true;
            return true;
        }

        public bool TryRemoveBody( ITrajectoryTransform transform )
        {
            if( transform == null ) return false;
            if( _staleToAdd.Contains( transform ) )
            {
                _staleToAdd.Remove( transform );
                _isStale = true;
                return true;
            }
            if( !_bodies.TryGetValue( transform, out var agent ) ) return false;
            if( _staleToRemove.Contains( transform ) ) return false;

            _staleToRemove.Add( transform );
            _isStale = true;
            if( agent.IsAttractor ) _staleAttractorChanged = true;
            return true;
        }

        public void Clear()
        {
            _bodies.Clear();
            _staleToAdd.Clear();
            _staleToRemove.Clear();
            _staleExisting.Clear();
            _attractorCache = Array.Empty<ITrajectoryTransform>();
            _followerCache = Array.Empty<ITrajectoryTransform>();
            _staleAttractorChanged = false;
            _isStale = true;
        }

        public TrajectoryStateVector GetStateVector( double ut, ITrajectoryTransform transform )
        {
            FixStale();
            if( transform == null || !_bodies.TryGetValue( transform, out var agent ) )
                throw new ArgumentException( "Transform is not registered in the trajectory simulator.", nameof( transform ) );

            if( agent.Ephemeris.Count == 0 )
                return agent.CurrentState; // If it's single valid boundary, return physical state

            return agent.Ephemeris.Evaluate( ut, Ephemeris.Side.IncreasingUT );
        }

        public bool TryGetStateVector( double ut, ITrajectoryTransform transform, out TrajectoryStateVector stateVector )
        {
            FixStale();
            if( transform == null || !_bodies.TryGetValue( transform, out var agent ) )
                throw new ArgumentException( "Transform is not registered in the trajectory simulator.", nameof( transform ) );

            if( agent.Ephemeris.Count == 0 )
            {
                if( ut == agent.StartUT )
                {
                    stateVector = agent.CurrentState;
                    return true;
                }
                stateVector = default;
                return false;
            }

            if( ut >= agent.Ephemeris.LowUT && ut <= agent.Ephemeris.HighUT )
            {
                stateVector = agent.Ephemeris.Evaluate( ut, Ephemeris.Side.IncreasingUT );
                return true;
            }

            stateVector = default;
            return false;
        }

        public TrajectoryStateVector GetCurrentStateVector( ITrajectoryTransform transform )
        {
            FixStale();
            if( transform == null || !_bodies.TryGetValue( transform, out var agent ) )
                throw new ArgumentException( "Transform is not registered in the trajectory simulator.", nameof( transform ) );

            return agent.CurrentState;
        }

        public void MarkStale( ITrajectoryTransform transform )
        {
            if( transform == null ) return;
            if( !_bodies.TryGetValue( transform, out var agent ) ) return;

            _staleExisting.Add( transform );

            if( agent.IsAttractor != transform.IsAttractor )
            {
                _staleToRemove.Add( transform );
                _staleToAdd.Add( transform );
                _staleAttractorChanged = true;
            }

            _isStale = true;
        }

        public void ResetToCurrent()
        {
            foreach( var kvp in _bodies )
            {
                var agent = kvp.Value;
                agent.Ephemeris.Clear();
                _staleExisting.Add( kvp.Key );
            }
            _isStale = true;
        }

        // --- Core Internal Routines ---

        /// <summary>
        /// Synchronizes pending state changes, additions, removals, and class reorganizations safely.
        /// </summary>
        public void FixStale()
        {
            if( !_isStale ) return;

            bool resetAllFollowers = _staleAttractorChanged || _staleExisting.Any( t => t.IsAttractor );

            if( resetAllFollowers )
            {
                foreach( var kvp in _bodies )
                {
                    if( !kvp.Value.IsAttractor )
                        _staleExisting.Add( kvp.Key );
                }
            }

            foreach( var transform in _staleExisting )
            {
                if( _bodies.TryGetValue( transform, out var agent ) )
                {
                    agent.CurrentState = transform.GetBodyState();
                    agent.StartUT = TimeManager.UT;
                    agent.Ephemeris.Clear();
                }
            }

            foreach( var transform in _staleToRemove )
            {
                _bodies.Remove( transform );
            }

            foreach( var transform in _staleToAdd )
            {
                if( !_bodies.TryGetValue( transform, out var agent ) )
                {
                    agent = new BodySimulationAgent
                    {
                        Transform = transform,
                        Ephemeris = new Ephemeris( 64, _ephemerisMaxError, _ephemerisDuration )
                    };
                    _bodies.Add( transform, agent );
                }

                agent.Integrator = transform.Integrator;
                agent.AccelerationProviders = transform.AccelerationProviders?.ToArray() ?? Array.Empty<ITrajectoryStepProvider>();
                agent.IsAttractor = transform.IsAttractor;
                agent.CurrentState = transform.GetBodyState();
                agent.StartUT = TimeManager.UT;
                agent.Ephemeris.Clear();
            }

            var newAttractors = new List<ITrajectoryTransform>();
            var newFollowers = new List<ITrajectoryTransform>();

            int attractorIndex = 0;
            int followerIndex = 0;

            foreach( var kvp in _bodies )
            {
                var agent = kvp.Value;
                if( agent.IsAttractor )
                {
                    agent.TimestepperIndex = attractorIndex++;
                    newAttractors.Add( kvp.Key );
                }
                else
                {
                    agent.TimestepperIndex = followerIndex++;
                    newFollowers.Add( kvp.Key );
                }
            }

            _attractorCache = newAttractors.ToArray();
            _followerCache = newFollowers.ToArray();

            _staleExisting.Clear();
            _staleToRemove.Clear();
            _staleToAdd.Clear();
            _staleAttractorChanged = false;
            _isStale = false;
        }

        /// <summary>
        /// Main entry point for performing numerical integrations over time intervals.
        /// </summary>
        public void Simulate( double endUT )
        {
            lock( _simulationLock )
            {
                if( _isSimulating )
                    throw new InvalidOperationException( "Simulation is already running on another thread." );

                _isSimulating = true;
                try
                {
                    Simulate_Internal( endUT );
                }
                finally
                {
                    _isSimulating = false;
                }
            }
        }

        private void Simulate_Internal( double endUT )
        {
            FixStale();

            double defaultStepSize = _direction == SimulationDirection.Backward ? -Math.Abs( DefaultStepSize ) : Math.Abs( DefaultStepSize );

            // Step A: Simulate Attractor Bodies
            TrajectoryStateVector[] attractorStates = new TrajectoryStateVector[_attractorCache.Length];
            TrajectoryStateVector[] nextAttractorStates = new TrajectoryStateVector[_attractorCache.Length];
            ReadOnlySpan<ITrajectoryTransform> attractorsSpan = _attractorCache.AsSpan();

            for( int i = 0; i < attractorsSpan.Length; i++ )
            {
                var transform = attractorsSpan[i];
                var agent = _bodies[transform];

                double localUT;
                TrajectoryStateVector currentState;

                if( agent.Ephemeris.Count == 0 )
                {
                    localUT = agent.StartUT;
                    currentState = agent.CurrentState;
                }
                else
                {
                    localUT = _direction == SimulationDirection.Forward ? agent.Ephemeris.HighUT : agent.Ephemeris.LowUT;
                    currentState = agent.CurrentState; // CurrentState should be strictly updated to last inserted!
                }

                while( (_direction == SimulationDirection.Forward && localUT < endUT) ||
                       (_direction == SimulationDirection.Backward && localUT > endUT) )
                {
                    double step = defaultStepSize;
                    if( _direction == SimulationDirection.Forward && localUT + step > endUT )
                        step = endUT - localUT;
                    else if( _direction == SimulationDirection.Backward && localUT + step < endUT )
                        step = endUT - localUT;

                    // Evaluate other attractors at localUT
                    for( int j = 0; j < attractorsSpan.Length; j++ )
                    {
                        if( i == j )
                            attractorStates[j] = currentState;
                        else
                        {
                            var otherAgent = _bodies[attractorsSpan[j]];
                            if( otherAgent.Ephemeris.Count > 0 )
                            {
                                // Cap evaluation to other attractor's boundaries for safety
                                double evalUT = Math.Max( otherAgent.Ephemeris.LowUT, Math.Min( otherAgent.Ephemeris.HighUT, localUT ) );
                                attractorStates[j] = otherAgent.Ephemeris.Evaluate( evalUT, Ephemeris.Side.IncreasingUT );
                            }
                            else
                            {
                                attractorStates[j] = otherAgent.CurrentState;
                            }
                        }
                    }

                    var context = new TrajectorySimulationContext( localUT, step, currentState, i, attractorStates );
                    double usedStep = agent.Integrator.Step( context, agent.AccelerationProviders, out TrajectoryStateVector nextState );

                    if( _direction == SimulationDirection.Backward && usedStep > 0 )
                        usedStep = -usedStep;
                    else if( _direction == SimulationDirection.Forward && usedStep < 0 )
                        usedStep = -usedStep;

                    localUT += usedStep;
                    currentState = nextState;

                    agent.Ephemeris.InsertAdaptive( localUT, currentState );
                }

                agent.CurrentState = currentState;
            }

            // Step B: Simulate Followers
            ITrajectoryTransform[] followersArray = _followerCache;
            ITrajectoryTransform[] attractorsArray = _attractorCache;

            Parallel.ForEach( followersArray, ( transform ) =>
            {
                var agent = _bodies[transform];
                double localUT;
                TrajectoryStateVector currentState;

                if( agent.Ephemeris.Count == 0 )
                {
                    localUT = agent.StartUT;
                    currentState = agent.CurrentState;
                }
                else
                {
                    localUT = _direction == SimulationDirection.Forward ? agent.Ephemeris.HighUT : agent.Ephemeris.LowUT;
                    currentState = agent.CurrentState;
                }

                TrajectoryStateVector[] localAttractorStates = new TrajectoryStateVector[attractorsArray.Length];

                while( (_direction == SimulationDirection.Forward && localUT < endUT) ||
                       (_direction == SimulationDirection.Backward && localUT > endUT) )
                {
                    double step = defaultStepSize;
                    if( _direction == SimulationDirection.Forward && localUT + step > endUT )
                        step = endUT - localUT;
                    else if( _direction == SimulationDirection.Backward && localUT + step < endUT )
                        step = endUT - localUT;

                    for( int j = 0; j < attractorsArray.Length; j++ )
                    {
                        var otherAgent = _bodies[attractorsArray[j]];
                        if( otherAgent.Ephemeris.Count > 0 )
                        {
                            double evalUT = Math.Max( otherAgent.Ephemeris.LowUT, Math.Min( otherAgent.Ephemeris.HighUT, localUT ) );
                            localAttractorStates[j] = otherAgent.Ephemeris.Evaluate( evalUT, Ephemeris.Side.IncreasingUT );
                        }
                        else
                        {
                            localAttractorStates[j] = otherAgent.CurrentState;
                        }
                    }

                    var context = new TrajectorySimulationContext( localUT, step, currentState, -1, localAttractorStates );
                    double usedStep = agent.Integrator.Step( context, agent.AccelerationProviders, out TrajectoryStateVector nextState );

                    if( _direction == SimulationDirection.Backward && usedStep > 0 )
                        usedStep = -usedStep;
                    else if( _direction == SimulationDirection.Forward && usedStep < 0 )
                        usedStep = -usedStep;

                    localUT += usedStep;
                    currentState = nextState;

                    agent.Ephemeris.InsertAdaptive( localUT, currentState );
                }

                agent.CurrentState = currentState;
            } );
        }
    }
}
