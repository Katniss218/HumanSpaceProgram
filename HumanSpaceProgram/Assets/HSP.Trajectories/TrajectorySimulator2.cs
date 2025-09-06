//using HSP.Time;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace HSP.Trajectories
//{
//    public sealed class TrajectorySimulator2 : IReadonlyTrajectorySimulator
//    {
//        public enum SimulationDirection
//        {
//            Forward,
//            Backward
//        }

//        // Timestepper stuff (individual arrays).

//        /// <summary>
//        /// Gets or sets the maximum step size for the simulation, in [s]. <br/>
//        /// A single step will never exceed this value.
//        /// </summary>
//        public double MaxStepSize { get; set; } = 1.0;

//        /// <summary>
//        /// Gets the number of bodies in the simulation.
//        /// </summary>
//        public int BodyCount => _bodies.Count;

//        public ReadOnlySpan<ITrajectoryTransform> Attractors => _attractorCache;

//        private ITrajectoryIntegrator[] _attractors;
//        private ITrajectoryStepProvider[][] _attractorAccelerationProviders;

//        private ITrajectoryIntegrator[] _followers;
//        private ITrajectoryStepProvider[][] _followerAccelerationProviders;

//        private TrajectoryStateVector[] _currentStateAttractors;
//        private TrajectoryStateVector[] _nextStateAttractors;

//        private TrajectoryStateVector[] _currentStateFollowers;
//        private TrajectoryStateVector[] _nextStateFollowers;

//        //private double _headUT;
//        //private double _tailUT; // usually equal to TimeManager.UT, but store it here because that can change during the frame/thread safety.

//        private Ephemeris2[] _attractorEphemerides;
//        private Ephemeris2[] _followerEphemerides;

//        private SimulationDirection _direction;

//        // Storage stuff.

//        private class Entry
//        {
//            public bool isAttractor;
//            public int timestepperIndex;
//            public Ephemeris2 ephemeris;
//        }

//        private Dictionary<ITrajectoryTransform, Entry> _bodies = new();

//        private HashSet<ITrajectoryTransform> _staleExisting = new();
//        private HashSet<ITrajectoryTransform> _staleToAdd = new();
//        private HashSet<ITrajectoryTransform> _staleToRemove = new();
//        private bool _staleAttractorChanged = false;
//        private bool _isStale = true;

//        private ITrajectoryTransform[] _attractorCache;

//        /// <summary>
//        /// Initializes a new instance of the TrajectorySimulator2 class.
//        /// </summary>
//        public TrajectorySimulator2()
//        {
//            ResetToCurrent();
//        }

//        public int GetAttractorIndex( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                return -1;

//            if( !_bodies.TryGetValue( transform, out var entry ) )
//                return -1;

//            if( !entry.isAttractor )
//                return -1;

//            return entry.timestepperIndex;
//        }

//        /// <summary>
//        /// Sets the ephemeris parameters for all bodies.
//        /// </summary>
//        public void SetEphemerisParameters( double maxError, double maxDuration )
//        {
//            foreach( var (_, entry) in _bodies )
//            {
//                entry.ephemeris.MaxError = maxError;
//                entry.ephemeris.MaxDuration = maxDuration;
//            }
//        }

//        public double GetHighUT( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                throw new ArgumentNullException( nameof( transform ) );

//            if( !_bodies.TryGetValue( transform, out var entry ) )
//                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

//            return entry.ephemeris.HighUT;
//        }

//        public double GetLowUT( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                throw new ArgumentNullException( nameof( transform ) );

//            if( !_bodies.TryGetValue( transform, out var entry ) )
//                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

//            return entry.ephemeris.LowUT;
//        }

//        public bool HasBody( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                return false;
//            return _bodies.ContainsKey( transform );
//        }

//        public IEnumerable<(ITrajectoryTransform t, IReadonlyEphemeris e)> GetBodies()
//        {
//            return _bodies.Select( kvp => (kvp.Key, (IReadonlyEphemeris)kvp.Value.ephemeris) );
//        }

//        public bool TryGetBody( ITrajectoryTransform transform, out IReadonlyEphemeris ephemeris )
//        {
//            bool x = _bodies.TryGetValue( transform, out var entry );
//            ephemeris = entry?.ephemeris;
//            return x;
//        }

//        [Obsolete( "untested" )]
//        public bool TryAddBody( ITrajectoryTransform transform, Ephemeris2 ephemeris = null )
//        {
//            if( transform == null )
//                return false;

//            if( _bodies.ContainsKey( transform ) )
//                return false;

//            if( ephemeris != null ) // clear but leave settings.
//                ephemeris.Clear();

//            bool wasAdded = _bodies.TryAdd( transform, new Entry() { timestepperIndex = -1, isAttractor = transform.IsAttractor, ephemeris = ephemeris } );
//            if( !wasAdded )
//                return false;

//            _staleToAdd.Add( transform );
//            _staleAttractorChanged |= transform.IsAttractor;
//            _isStale = true;
//            return true;
//        }

//        [Obsolete( "untested" )]
//        public bool TryRemoveBody( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                return false;

//            bool wasRemoved = _bodies.Remove( transform );
//            if( !wasRemoved )
//                return false;

//            _staleToRemove.Add( transform );
//            _staleAttractorChanged |= transform.IsAttractor;
//            _isStale = true;
//            return true;
//        }

//        [Obsolete( "untested" )]
//        public void Clear()
//        {
//            _bodies.Clear();
//            _staleToAdd.Clear();
//            _staleToRemove.Clear();
//            _staleExisting.Clear();
//            _staleAttractorChanged = true;
//            _isStale = true;
//        }

//        // 'MoveTo' moves assuming the simulation is not stale.
//        public void MoveTo( SimulationDirection direction )
//        {
//            (double headUT, double tailUT) = GetSimulatedInterval();
//            if( direction == SimulationDirection.Forward )
//                MoveTo( headUT );
//            else
//                MoveTo( tailUT );
//        }

//        public void MoveTo( double ut )
//        {
//#warning TODO 
//        }

//        /// <summary>
//        /// Resets the simulation state to the current game time.
//        /// </summary>
//        [Obsolete( "untested" )]
//        public void ResetToCurrent()
//        {
//#warning TODO - this needs to tell to reset or actually reset right away.
//            _isStale = true;
//        }

//        public TrajectoryStateVector GetStateVector( double ut, ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                throw new ArgumentNullException( nameof( transform ) );

//            if( !_bodies.TryGetValue( transform, out var entry ) )
//                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

//            return entry.ephemeris.Evaluate( ut, Ephemeris2.Side.IncreasingUT );
//        }

//        [Obsolete( "untested" )]
//        public void ResetStateVector( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                throw new ArgumentNullException( nameof( transform ) );

//            if( !_bodies.ContainsKey( transform ) )
//                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

//            _staleExisting.Add( transform );
//            _isStale = true;
//        }


//        /// <summary>
//        /// Marks the body as stale, meaning that the state vector stored in the simulation, and the ephemerides no longer match the actual body's state vector in the game.
//        /// </summary>
//        [Obsolete( "untested" )]
//        public void MarkStale( ITrajectoryTransform transform )
//        {
//            if( transform == null )
//                return;

//            if( !_bodies.ContainsKey( transform ) )
//                return;

//            _staleExisting.Add( transform );
//            _isStale = true;
//        }

//        //public void MarkStale( ITrajectoryTransform transform, double fromUT, double toUT )
//        //{

//        //}

//        [Obsolete( "untested" )]
//        private void FixStale( SimulationDirection direction )
//        {
//            // Update attractor cache
//            _attractorCache = _bodies.Keys
//                .Where( t => t.IsAttractor )
//                .ToArray();

//            bool bodyCountChanged = _staleToAdd.Count > 0 || _staleToRemove.Count > 0;
//            int attractorIndex = _attractors.Length;
//            int followerIndex = _followers.Length;

//            // Copy the old data to the new arrays first.
//            //   This will 'defragment' the gaps where existing bodies were removed.
//            // The attractor/follower indices will point at the end of the copied section,
//            //   and the arrays will have space for the new bodies after that.
//            if( bodyCountChanged )
//            {
//                int totalBodies = _bodies.Count + _staleToAdd.Count - _staleToRemove.Count;
//                int numAttractors = _attractorCache.Length;
//                int numFollowers = totalBodies - numAttractors;

//                // new arrays and copy ephemerides.
//                var oldAttractors = _attractors;
//                var oldFollowers = _followers;
//                var oldAttractorAccelerationProviders = _attractorAccelerationProviders;
//                var oldFollowerAccelerationProviders = _followerAccelerationProviders;

//                var oldCurrentStateAttractors = _currentStateAttractors;
//                var oldCurrentStateFollowers = _currentStateFollowers;
//                var oldNextStateAttractors = _nextStateAttractors;
//                var oldNextStateFollowers = _nextStateFollowers;

//                var oldAttractorEphemerides = _attractorEphemerides;
//                var oldFollowerEphemerides = _followerEphemerides;

//                _attractors = new ITrajectoryIntegrator[numAttractors];
//                _followers = new ITrajectoryIntegrator[numFollowers];
//                _attractorAccelerationProviders = new ITrajectoryStepProvider[numAttractors][];
//                _followerAccelerationProviders = new ITrajectoryStepProvider[numFollowers][];

//                _currentStateAttractors = new TrajectoryStateVector[numAttractors];
//                _nextStateAttractors = new TrajectoryStateVector[numAttractors];
//                _currentStateFollowers = new TrajectoryStateVector[numFollowers];
//                _nextStateFollowers = new TrajectoryStateVector[numFollowers];

//                _attractorEphemerides = new Ephemeris2[numAttractors];
//                _followerEphemerides = new Ephemeris2[numFollowers];

//                attractorIndex = 0;
//                int sourceAttractorIndex = -1;
//                followerIndex = 0;
//                int sourceFollowerIndex = -1;
//                foreach( var (body, entry) in _bodies )
//                {
//                    // `entry.isAttractor` - what it was when it was added.
//                    // `body.IsAttractor`  - what it is now.
//                    if( entry.isAttractor )
//                        sourceAttractorIndex++;
//                    else
//                        sourceFollowerIndex++;

//                    // Only copy existing bodies that were not removed.
//                    if( _staleToRemove.Contains( body ) || _staleToAdd.Contains( body ) )
//                        continue;

//                    if( body.IsAttractor )
//                    {
//                        _attractors[attractorIndex] = entry.isAttractor
//                            ? oldAttractors[sourceAttractorIndex]
//                            : oldFollowers[sourceFollowerIndex];
//                        _attractorAccelerationProviders[attractorIndex] = entry.isAttractor
//                            ? oldAttractorAccelerationProviders[sourceAttractorIndex]
//                            : oldFollowerAccelerationProviders[sourceFollowerIndex];
//                        _currentStateAttractors[attractorIndex] = entry.isAttractor
//                            ? oldCurrentStateAttractors[sourceAttractorIndex]
//                            : oldCurrentStateFollowers[sourceFollowerIndex];
//                        _attractorEphemerides[attractorIndex] = entry.ephemeris;
//                        entry.isAttractor = true;
//                        entry.timestepperIndex = attractorIndex;

//                        attractorIndex++;
//                    }
//                    else
//                    {
//                        _followers[followerIndex] = entry.isAttractor
//                            ? oldAttractors[sourceAttractorIndex]
//                            : oldFollowers[sourceFollowerIndex];
//                        _followerAccelerationProviders[followerIndex] = entry.isAttractor
//                            ? oldAttractorAccelerationProviders[sourceAttractorIndex]
//                            : oldFollowerAccelerationProviders[sourceFollowerIndex];
//                        _currentStateFollowers[followerIndex] = entry.isAttractor
//                            ? oldCurrentStateAttractors[sourceAttractorIndex]
//                            : oldCurrentStateFollowers[sourceFollowerIndex];
//                        _followerEphemerides[followerIndex] = entry.ephemeris;
//                        entry.isAttractor = false;
//                        entry.timestepperIndex = followerIndex;

//                        followerIndex++;
//                    }
//                }
//            }

//            //  After that, update the stale entries.
//            if( _staleExisting.Count > 0 )
//            {
//                foreach( var body in _staleExisting )
//                {
//                    if( !_bodies.TryGetValue( body, out var entry ) )
//                        continue;

//                    // Update the body's state from the actual transform
//                    if( entry.isAttractor )
//                    {
//                        _attractors[entry.timestepperIndex] = body.Integrator;
//                        _attractorAccelerationProviders[entry.timestepperIndex] = body.AccelerationProviders.ToArray();
//                        _currentStateAttractors[entry.timestepperIndex] = body.GetBodyState();
//                        entry.ephemeris.Clear();
//                    }
//                    else
//                    {
//                        _followers[entry.timestepperIndex] = body.Integrator;
//                        _followerAccelerationProviders[entry.timestepperIndex] = body.AccelerationProviders.ToArray();
//                        _currentStateFollowers[entry.timestepperIndex] = body.GetBodyState();
//                        entry.ephemeris.Clear();
//                    }
//                }

//                _staleExisting.Clear();
//            }

//            if( _staleToAdd.Count > 0 )
//            {
//                foreach( var body in _staleToAdd )
//                {
//                    if( !_bodies.TryGetValue( body, out var entry ) )
//                        continue;

//                    if( entry.isAttractor )
//                    {
//                        _attractors[entry.timestepperIndex] = body.Integrator;
//                        _attractorAccelerationProviders[entry.timestepperIndex] = body.AccelerationProviders.ToArray();
//                        _currentStateAttractors[entry.timestepperIndex] = body.GetBodyState();
//                        if( entry.ephemeris == null )
//                            entry.ephemeris = new Ephemeris2( 1000, 0.01 ); // Default capacity and error
//                        entry.isAttractor = true;
//                        entry.timestepperIndex = attractorIndex;

//                        attractorIndex++;
//                    }
//                    else
//                    {
//                        _followers[entry.timestepperIndex] = body.Integrator;
//                        _followerAccelerationProviders[entry.timestepperIndex] = body.AccelerationProviders.ToArray();
//                        _currentStateFollowers[entry.timestepperIndex] = body.GetBodyState();
//                        if( entry.ephemeris == null )
//                            entry.ephemeris = new Ephemeris2( 1000, 0.01 ); // Default capacity and error
//                        entry.isAttractor = false;
//                        entry.timestepperIndex = followerIndex;

//                        followerIndex++;
//                    }
//                }

//                _staleToAdd.Clear();
//            }

//            // ephemeris of a stale body needs to be reset, and the body needs to be resimulated.
//            // the 'head' UT of the simulation needs to be rolled back to the min() of the bodies' head UT.


//            // Update head and tail UT
//            if( _staleAttractorChanged )
//            {
//                foreach( var (body, entry) in _bodies )
//                {
//                    entry.ephemeris.Clear();
//                    entry.ephemeris.InsertAdaptive( TimeManager.UT, body.GetBodyState() );
//                }
//            }

//            // body can theoretically be marked as partially stale, if e.g. a maneuver node was added in the middle of the ephemeris.
//            // that's for later tho.
//            _staleAttractorChanged = false;
//        }

//        static double GetMiddleValue( double a, double b, double c )
//        {
//            if( (a <= b && b <= c) || (c <= b && b <= a) ) return b;
//            if( (b <= a && a <= c) || (c <= a && a <= b) ) return a;
//            return c;
//        }

//        public (double headUT, double tailUT) GetSimulatedInterval()
//        {
//            double headUT = double.MaxValue;
//            double tailUT = double.MinValue;
//            foreach( var (_, entry) in _bodies )
//            {
//                if( entry.ephemeris.HighUT > headUT )
//                    headUT = entry.ephemeris.HighUT;
//                if( entry.ephemeris.LowUT < tailUT )
//                    tailUT = entry.ephemeris.LowUT;
//            }

//            return (headUT, tailUT);
//        }

//        public void Simulate( double targetUT )
//        {
//            (double headUT, double tailUT) = GetSimulatedInterval();

//            double fromUT = GetMiddleValue( headUT, tailUT, targetUT );
//            if( !this._isStale && targetUT <= headUT && targetUT >= tailUT )
//            {
//                return;
//            }

//            Simulate_Internal( fromUT, targetUT, MaxStepSize );
//        }

//        public async Task SimulateAsync( double targetUT )
//        {
//            (double headUT, double tailUT) = GetSimulatedInterval();

//            double fromUT = GetMiddleValue( headUT, tailUT, targetUT );
//            if( !this._isStale && targetUT <= headUT && targetUT >= tailUT )
//            {
//                return;
//            }

//            await Task.Run( () => Simulate_Internal( fromUT, targetUT, MaxStepSize ) );
//        }

//        [Obsolete( "untested" )]
//        private void Simulate_Internal( double startUT, double endUT, double step )
//        {
//            var newDirection = endUT > startUT ? SimulationDirection.Forward : SimulationDirection.Backward;
//            if( this._isStale )
//            {
//                this.FixStale( newDirection );
//            }

//            if( newDirection != _direction )
//            {
//#warning TODO
//                // reset states to current.
//            }

//            _direction = newDirection;

//            // simulate some bodies. use the already simulated bodies (if any) to retrieve state vectors from ephemerides.
//            // only followers can be simulated using ephemerides.

//            // use a heavy per-step integrator with larger step size.

//            // we'll call 'simulate' to perform a chunk of work at a given time?
//            // selectable which followers should be included in the simulation. where though? maybe just not add them to the simulation at all?

//            // find what we actually need to simulate, and the interval to simulate from/to.

//            if( _attractors == null || _attractors.Length == 0 )
//                return;

//            const double EPSILON = 1e-4;
//            double ut = startUT;
//            double actualStep = step;

//            if( _direction == SimulationDirection.Forward )
//            {
//                // Forward simulation
//                while( endUT - ut > EPSILON )
//                {
//                    double minStep = double.MaxValue;

//                    for( int i = 0; i < _attractors.Length; i++ )
//                    {
//                        ITrajectoryIntegrator integrator = _attractors[i];
//                        var nextStep = integrator.Step( new TrajectorySimulationContext( ut, actualStep, _currentStateAttractors[i], i, _currentStateAttractors ), _attractorAccelerationProviders[i], out _nextStateAttractors[i] );

//                        if( nextStep < minStep )
//                        {
//                            minStep = nextStep;
//                        }
//                    }

//                    for( int i = 0; i < _attractorEphemerides.Length; i++ )
//                    {
//                        _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
//                    }

//                    ut += step;
//                    actualStep = minStep;

//                    var temp = _currentStateAttractors;
//                    _currentStateAttractors = _nextStateAttractors;
//                    _nextStateAttractors = temp;
//                }

//                // Store final attractor states
//                for( int i = 0; i < _attractorEphemerides.Length; i++ )
//                {
//                    _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
//                }

//                // Now simulate followers using attractor ephemerides
//                if( _followers != null && _followers.Length > 0 )
//                {
//                    SimulateFollowers( startUT, endUT, step );
//                }
//            }
//            else
//            {
//                // Backward simulation - for now, just throw not implemented
//                throw new NotImplementedException( "Backward simulation is not yet implemented." );
//            }

//        }

//        private void SimulateFollowers( double startUT, double endUT, double step )
//        {
//            const double EPSILON = 1e-4;
//            double ut = startUT;

//            // Simulate followers in parallel
//            Parallel.For( 0, _followers.Length, bodyIndex =>
//            {
//                while( endUT - ut > EPSILON )
//                {
//                    double minStep = double.MaxValue;
//                    double nextStep = step;


//                    ITrajectoryIntegrator integrator = _followers[bodyIndex];
//                    double localStep = integrator.Step( new TrajectorySimulationContext( ut, step, _currentStateFollowers[bodyIndex], -1, _currentStateAttractors ), _followerAccelerationProviders[bodyIndex], out _nextStateFollowers[bodyIndex] );

//                    // Use Interlocked to safely update minStep
//                    double currentMinStep;
//                    do
//                    {
//                        currentMinStep = minStep;
//                        if( localStep >= currentMinStep )
//                            break;
//                    }
//                    while( Interlocked.CompareExchange( ref minStep, localStep, currentMinStep ) != currentMinStep );


//                    // Store follower states in ephemerides
//                    for( int i = 0; i < _followerEphemerides.Length; i++ )
//                    {
//                        _followerEphemerides[i].InsertAdaptive( ut, _currentStateFollowers[i] );
//                    }

//                    ut += step;
//                    step = minStep;

//                    // Swap current and next state arrays
//                    var temp = _currentStateFollowers;
//                    _currentStateFollowers = _nextStateFollowers;
//                    _nextStateFollowers = temp;
//                }
//            } );

//            // Store final follower states
//            for( int i = 0; i < _followerEphemerides.Length; i++ )
//            {
//                _followerEphemerides[i].InsertAdaptive( ut, _currentStateFollowers[i] );
//            }
//        }
//    }
//}