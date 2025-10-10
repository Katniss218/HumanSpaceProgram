using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using static UnityEngine.Networking.UnityWebRequest;

namespace HSP.Trajectories
{
    public readonly struct TimeInterval
    {
        public readonly double minUT;
        public readonly double maxUT;

        public double duration => maxUT - minUT;

        public TimeInterval( double point )
        {
            this.minUT = point;
            this.maxUT = point;
        }

        public TimeInterval( double minUT, double maxUT )
        {
            if( maxUT < minUT )
                throw new ArgumentException( "maxUT must be greater than or equal to minUT." );

            this.minUT = minUT;
            this.maxUT = maxUT;
        }

        public bool Contains( double ut )
        {
            return ut >= minUT && ut <= maxUT;
        }
    }

    public sealed class TrajectorySimulator2 : IReadonlyTrajectorySimulator
    {
        static double GetMiddleValue( double a, double b, double c )
        {
            if( (a <= b && b <= c) || (c <= b && b <= a) ) return b;
            if( (b <= a && a <= c) || (c <= a && a <= b) ) return a;
            return c;
        }

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

        // Timestepper stuff (individual arrays).

        /// <summary>
        /// Gets or sets the maximum step size for the simulation, in [s]. <br/>
        /// A single step will never exceed this value.
        /// </summary>
        public double MaxStepSize { get; set; } = 1.0;

        public double DefaultStepSize { get; set; } = 1.0;

        /// <summary>
        /// Gets the number of bodies in the simulation.
        /// </summary>
        public int BodyCount => _bodies.Count;

        public ReadOnlySpan<ITrajectoryTransform> Attractors => _attractorCache;

        private ITrajectoryIntegrator[] _attractors;
        private ITrajectoryStepProvider[][] _attractorAccelerationProviders;

        private ITrajectoryIntegrator[] _followers;
        private ITrajectoryStepProvider[][] _followerAccelerationProviders;

        private TrajectoryStateVector[] _currentStateAttractors;
        private TrajectoryStateVector[] _nextStateAttractors;

        private TrajectoryStateVector[] _currentStateFollowers;
        private TrajectoryStateVector[] _nextStateFollowers;

        private double _initialUT;

        private Ephemeris2[] _attractorEphemerides;
        private Ephemeris2[] _followerEphemerides;

        private SimulationDirection _direction;

        private double _ephemerisMaxError = 0.02;
        private double _ephemerisDuration = 1000000;

        // Storage stuff.

        private class Entry
        {
            public bool isAttractor;
            public int timestepperIndex;
            public Ephemeris2 ephemeris;
        }

        private Dictionary<ITrajectoryTransform, Entry> _bodies = new();

        private HashSet<ITrajectoryTransform> _staleExisting = new();
        private HashSet<ITrajectoryTransform> _staleToAdd = new();
        private HashSet<ITrajectoryTransform> _staleToRemove = new();
        private bool _staleAttractorChanged = false;
        private bool _isStale = true;
        private volatile bool _isSimulating = false; // thread safety thing.
        private readonly object _simulationLock = new object();

        private ITrajectoryTransform[] _attractorCache;

        public bool IsSimulating => _isSimulating;

        /// <summary>
        /// Initializes a new instance of the TrajectorySimulator2 class.
        /// </summary>
        public TrajectorySimulator2( double step, int count )
        {
            this.DefaultStepSize = step;
            // ignore count.
            ResetToCurrent();
        }

        /// <summary>
        /// Computes the interval where the ephemerides are valid.
        /// </summary>
        public TimeInterval GetSimulatedInterval( SimulatedIntervalOptions options = SimulatedIntervalOptions.IncludeAttractorsAndFollowers )
        {
            if( _bodies.Count == 0 )
                return new TimeInterval( _initialUT, _initialUT );

            double headUT = double.MinValue;
            double tailUT = double.MaxValue;
            foreach( var (_, entry) in _bodies )
            {
                if( entry.isAttractor && !options.HasFlag( SimulatedIntervalOptions.IncludeAttractors ) )
                    continue;
                if( !entry.isAttractor && !options.HasFlag( SimulatedIntervalOptions.IncludeFollowers ) )
                    continue;

                if( entry.ephemeris.Count == 0 )
                    return new TimeInterval( _initialUT, _initialUT );

                if( entry.ephemeris.HighUT > headUT )
                    headUT = entry.ephemeris.HighUT;
                if( entry.ephemeris.LowUT < tailUT )
                    tailUT = entry.ephemeris.LowUT;
            }

            return new TimeInterval( tailUT, headUT );
        }

        public int GetAttractorIndex( ITrajectoryTransform transform )
        {
            if( transform == null )
                return -1;

            if( !_bodies.TryGetValue( transform, out var entry ) )
                return -1;

            if( !entry.isAttractor )
                return -1;

            return entry.timestepperIndex;
        }

        public void SetInitialTime( double ut )
        {
            _initialUT = ut;
            ResetToCurrent();
        }

        public void SetEphemerisParameters( double maxError, double maxDuration, int initialCapacity )
        {
            _ephemerisDuration = maxDuration;
            _ephemerisMaxError = maxError;
            foreach( var (_, entry) in _bodies )
            {
                entry.ephemeris.MaxError = maxError;
                entry.ephemeris.MaxDuration = maxDuration;
            }
        }

        public double GetHighUT( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

            return entry.ephemeris.HighUT;
        }

        public double GetLowUT( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

            return entry.ephemeris.LowUT;
        }

        public bool HasBody( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            return _bodies.ContainsKey( transform ) && !_staleToRemove.Contains( transform ) || _staleToAdd.Contains( transform );
        }

        public IEnumerable<(ITrajectoryTransform t, IReadonlyEphemeris e)> GetBodies()
        {
            FixStale( _direction );

            return _bodies.Select( kvp => (kvp.Key, (IReadonlyEphemeris)kvp.Value.ephemeris) );
        }

        public bool TryGetBody( ITrajectoryTransform transform, out IReadonlyEphemeris ephemeris )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            FixStale( _direction );

            bool x = _bodies.TryGetValue( transform, out var entry );
            ephemeris = entry?.ephemeris;
            return x;
        }

        public bool TryAddBody( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( _bodies.ContainsKey( transform ) )
                return false;

            if( !_staleToAdd.Add( transform ) )
                return false;

            _staleAttractorChanged |= transform.IsAttractor;
            _isStale = true;
            return true;
        }

        public bool TryRemoveBody( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                return false;

            if( !_staleToRemove.Add( transform ) )
                return false;

            _staleAttractorChanged |= entry.isAttractor;
            _isStale = true;
            return true;
        }

        public void Clear()
        {
            _bodies.Clear();
            _staleToAdd.Clear();
            _staleToRemove.Clear();
            _staleExisting.Clear();
            _staleAttractorChanged = false;
            _isStale = true;
        }

        public TrajectoryStateVector GetStateVector( double ut, ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            FixStale( _direction );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The trajectory transform '{transform}' is not registered in the simulator.", nameof( transform ) );

            return entry.ephemeris.Evaluate( ut, Ephemeris2.Side.IncreasingUT );
        }

        public bool TryGetStateVector( double ut, ITrajectoryTransform transform, out TrajectoryStateVector stateVector )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            FixStale( _direction );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The trajectory transform '{transform}' is not registered in the simulator.", nameof( transform ) );

            var emphemeris = entry.ephemeris;
            if( emphemeris.HighUT < ut || emphemeris.LowUT > ut )
            {
                stateVector = default;
                return false;
            }

            stateVector = emphemeris.Evaluate( ut );
            return true;
        }

        public TrajectoryStateVector GetCurrentStateVector( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            FixStale( _direction );
            if( !_bodies.TryGetValue( transform, out var bodyEntry ) )
                throw new ArgumentException( $"The trajectory transform '{transform}' is not registered in the simulator.", nameof( transform ) );

            // Current doesn't need to evaluate the ephemeris. The data is already in the timestepper.
            if( bodyEntry.isAttractor )
                return _currentStateAttractors[bodyEntry.timestepperIndex];
            else
                return _currentStateFollowers[bodyEntry.timestepperIndex];
        }


        /// <summary>
        /// Marks the body as stale, meaning that the state vector stored in the simulation, and the ephemerides no longer match the actual body's state vector in the game.
        /// </summary>
        public void MarkStale( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                return;

            _staleExisting.Add( transform );
            if( entry.isAttractor != transform.IsAttractor )
            {
                _staleToRemove.Add( transform ); // re-add as the other type
                _staleToAdd.Add( transform );
            }

            _staleAttractorChanged |= entry.isAttractor || transform.IsAttractor;
            _isStale = true;
        }

        /// <summary>
        /// Resets the simulation state to the current game time.
        /// </summary>
        public void ResetToCurrent()
        {
            // Clear all ephemerides and mark everything stale so the stale pipeline
            // (FixStale) refreshes integrators, acceleration providers, and state vectors.
            foreach( var (body, entry) in _bodies )
            {
                entry.ephemeris.Clear();
                _staleExisting.Add( body );
            }

            _isStale = true;
        }

        //public void MarkStale( ITrajectoryTransform transform, double fromUT, double toUT )
        //{

        //}

        void MoveFollowersTo( double ut )
        {
            foreach( var entry in _bodies.Values )
            {
                if( !entry.isAttractor )
                    _currentStateFollowers[entry.timestepperIndex] = entry.ephemeris.Evaluate( ut );
            }
        }

        void MoveAttractorsTo( double ut )
        {
            foreach( var entry in _bodies.Values )
            {
                if( entry.isAttractor )
                    _currentStateAttractors[entry.timestepperIndex] = entry.ephemeris.Evaluate( ut );
            }
        }

        private void FixStale( SimulationDirection direction )
        {
            if( !_isStale )
                return;

            if( _staleAttractorChanged )
            {
                ResetToCurrent();
            }

            // Update attractor cache
            _attractorCache = _bodies.Keys
                .Union( _staleToAdd )
                .Except( _staleToRemove )
                .Where( t => t.IsAttractor )
                .ToArray();

            bool bodyCountChanged = _staleToAdd.Count > 0 || _staleToRemove.Count > 0;
            int attractorIndex = _attractors?.Length ?? 0;
            int followerIndex = _followers?.Length ?? 0;

            // Copy the old data to the new arrays first.
            //   This will 'defragment' the gaps where existing bodies were removed.
            // The attractor/follower indices will point at the end of the copied section,
            //   and the arrays will have space for the new bodies after that.
            if( bodyCountChanged )
            {
                int totalBodies = _bodies.Count + _staleToAdd.Count - _staleToRemove.Count;
                int numAttractors = _attractorCache.Length;
                int numFollowers = totalBodies - numAttractors;

                // new arrays and copy ephemerides.
                var oldAttractors = _attractors;
                var oldFollowers = _followers;
                var oldAttractorAccelerationProviders = _attractorAccelerationProviders;
                var oldFollowerAccelerationProviders = _followerAccelerationProviders;

                var oldCurrentStateAttractors = _currentStateAttractors;
                var oldCurrentStateFollowers = _currentStateFollowers;
                var oldNextStateAttractors = _nextStateAttractors;
                var oldNextStateFollowers = _nextStateFollowers;

                var oldAttractorEphemerides = _attractorEphemerides;
                var oldFollowerEphemerides = _followerEphemerides;

                _attractors = new ITrajectoryIntegrator[numAttractors];
                _followers = new ITrajectoryIntegrator[numFollowers];
                _attractorAccelerationProviders = new ITrajectoryStepProvider[numAttractors][];
                _followerAccelerationProviders = new ITrajectoryStepProvider[numFollowers][];

                _currentStateAttractors = new TrajectoryStateVector[numAttractors];
                _nextStateAttractors = new TrajectoryStateVector[numAttractors];
                _currentStateFollowers = new TrajectoryStateVector[numFollowers];
                _nextStateFollowers = new TrajectoryStateVector[numFollowers];

                _attractorEphemerides = new Ephemeris2[numAttractors];
                _followerEphemerides = new Ephemeris2[numFollowers];

                attractorIndex = 0;
                int sourceAttractorIndex = -1;
                followerIndex = 0;
                int sourceFollowerIndex = -1;
                foreach( var (body, entry) in _bodies )
                {
                    // `entry.isAttractor` - what it was when it was added.
                    // `body.IsAttractor`  - what it is now.
                    if( entry.isAttractor )
                        sourceAttractorIndex++;
                    else
                        sourceFollowerIndex++;

                    // Only copy existing bodies that were not removed.
                    if( _staleToRemove.Contains( body ) )
                        continue;

                    if( body.IsAttractor )
                    {
                        _attractors[attractorIndex] = entry.isAttractor
                            ? oldAttractors[sourceAttractorIndex]
                            : oldFollowers[sourceFollowerIndex];
                        _attractorAccelerationProviders[attractorIndex] = entry.isAttractor
                            ? oldAttractorAccelerationProviders[sourceAttractorIndex]
                            : oldFollowerAccelerationProviders[sourceFollowerIndex];
                        _currentStateAttractors[attractorIndex] = entry.isAttractor
                            ? oldCurrentStateAttractors[sourceAttractorIndex]
                            : oldCurrentStateFollowers[sourceFollowerIndex];
                        _attractorEphemerides[attractorIndex] = entry.ephemeris;
                        entry.isAttractor = true;
                        entry.timestepperIndex = attractorIndex;

                        attractorIndex++;
                    }
                    else
                    {
                        _followers[followerIndex] = entry.isAttractor
                            ? oldAttractors[sourceAttractorIndex]
                            : oldFollowers[sourceFollowerIndex];
                        _followerAccelerationProviders[followerIndex] = entry.isAttractor
                            ? oldAttractorAccelerationProviders[sourceAttractorIndex]
                            : oldFollowerAccelerationProviders[sourceFollowerIndex];
                        _currentStateFollowers[followerIndex] = entry.isAttractor
                            ? oldCurrentStateAttractors[sourceAttractorIndex]
                            : oldCurrentStateFollowers[sourceFollowerIndex];
                        _followerEphemerides[followerIndex] = entry.ephemeris;
                        entry.isAttractor = false;
                        entry.timestepperIndex = followerIndex;

                        followerIndex++;
                    }
                }

                _staleToRemove.Clear();
            }

            // Update stale bodies with current data
            if( _staleExisting.Count > 0 )
            {
                foreach( var body in _staleExisting )
                {
                    if( _staleToAdd.Contains( body ) )
                        continue;

                    if( !_bodies.TryGetValue( body, out var entry ) )
                        continue;

                    // Update the body's state from the actual transform
                    if( entry.isAttractor )
                    {
                        _attractors[entry.timestepperIndex] = body.Integrator;
                        _attractorAccelerationProviders[entry.timestepperIndex] = body.AccelerationProviders.ToArray();
                        _currentStateAttractors[entry.timestepperIndex] = body.GetBodyState();
                        entry.ephemeris.Clear();
                    }
                    else
                    {
                        _followers[entry.timestepperIndex] = body.Integrator;
                        _followerAccelerationProviders[entry.timestepperIndex] = body.AccelerationProviders.ToArray();
                        _currentStateFollowers[entry.timestepperIndex] = body.GetBodyState();
                        entry.ephemeris.Clear();
                    }
                }

                _staleExisting.Clear();
            }

            if( _staleToAdd.Count > 0 )
            {
                foreach( var body in _staleToAdd )
                {
                    var entry = new Entry()
                    {
                        ephemeris = new Ephemeris2( _ephemerisMaxError, _ephemerisDuration ) // TODO - use the simulator's ephemeris parameters.
                    };

                    if( body.IsAttractor )
                    {
                        _attractors[attractorIndex] = body.Integrator;
                        _attractorAccelerationProviders[attractorIndex] = body.AccelerationProviders.ToArray();
                        _currentStateAttractors[attractorIndex] = body.GetBodyState();
                        _attractorEphemerides[attractorIndex] = entry.ephemeris;
                        entry.isAttractor = true;
                        entry.timestepperIndex = attractorIndex;

                        attractorIndex++;
                    }
                    else
                    {
                        _followers[followerIndex] = body.Integrator;
                        _followerAccelerationProviders[followerIndex] = body.AccelerationProviders.ToArray();
                        _currentStateFollowers[followerIndex] = body.GetBodyState();
                        _followerEphemerides[followerIndex] = entry.ephemeris;
                        entry.isAttractor = false;
                        entry.timestepperIndex = followerIndex;

                        followerIndex++;
                    }

                    _bodies.Add( body, entry );
                }

                _staleToAdd.Clear();
            }

            // ephemeris of a stale body needs to be reset, and the body needs to be resimulated.
            // the 'head' UT of the simulation needs to be rolled back to the min() of the bodies' head UT.

            // body can theoretically be marked as partially stale, if e.g. a maneuver node was added in the middle of the ephemeris. that's for later tho.
            _staleAttractorChanged = false;
            _isStale = false;
        }

        public void Simulate( double endUT )
        {
            lock( _simulationLock )
            {
                if( _isSimulating )
                    throw new InvalidOperationException( "The simulator is already simulating." );

                Simulate_Internal( endUT );
            }
        }

#warning TODO - ephemeris needs to be thread safe.
        /*public Task SimulateAsync( double endUT )
        {
            lock( _simulationLock )
            {
                if( _isSimulating )
                    throw new InvalidOperationException( "The simulator is already simulating." );

                return Task.Run( () => Simulate_Internal( endUT ) );
            }
        }*/

        private void Simulate_Internal( double endUT )
        {
            try
            {
                _isSimulating = true;

                Profiler.BeginSample( "TrajectorySimulator.Simulate_Internal" );

                TimeInterval interval = GetSimulatedInterval( SimulatedIntervalOptions.IncludeAttractorsAndFollowers );

                double startUT = GetMiddleValue( interval.minUT, interval.maxUT, endUT );
                if( !_isStale && endUT <= interval.maxUT && endUT >= interval.minUT )
                {
                    return;
                }

                // Determine the direction from the start/end times and run the stale pipeline for that direction.
                var newDirection = _direction;
                if( Math.Abs( endUT - startUT ) > 1e-10 )
                    newDirection = endUT > startUT ? SimulationDirection.Forward : SimulationDirection.Backward;

                FixStale( newDirection );

                // Move the timestepper arrays to the end of the valid interval.
                _direction = newDirection;

                const double STEP_EPSILON = 1e-4;
                double originalStep = _direction == SimulationDirection.Forward ? DefaultStepSize : -DefaultStepSize;
                bool forward = _direction == SimulationDirection.Forward;

                // Simulate attractors.

                if( _attractors != null && _attractors.Length > 0 )
                {
                    var attractorInterval = GetSimulatedInterval( SimulatedIntervalOptions.IncludeAttractors );
                    if( attractorInterval.maxUT != _initialUT && interval.minUT != _initialUT )
                    {
                        if( forward )
                            MoveAttractorsTo( attractorInterval.maxUT );
                        else
                            MoveAttractorsTo( attractorInterval.minUT );
                    }

                    double step = originalStep;
                    double ut = startUT;
                    while( Math.Abs( endUT - ut ) > STEP_EPSILON )
                    {
                        if( Math.Abs( step ) > MaxStepSize )
                        {
                            step = forward ? MaxStepSize : -MaxStepSize;
                        }

                        double remainingTime = endUT - ut;
                        if( forward && step > remainingTime )
                        {
                            step = remainingTime; // don't overshoot the end time.
                        }
                        else if( !forward && step < remainingTime )
                        {
                            step = remainingTime; // don't overshoot the end time.
                        }

                        double minStep = forward ? double.MaxValue : double.MinValue;
                        for( int i = 0; i < _attractors.Length; i++ )
                        {
                            ITrajectoryIntegrator integrator = _attractors[i];
                            var nextStep = integrator.Step( new TrajectorySimulationContext( ut, step, _currentStateAttractors[i], i, _currentStateAttractors ), _attractorAccelerationProviders[i], out _nextStateAttractors[i] );

                            // Takes into account the sign of the step (direction).
                            if( forward )
                            {
                                if( nextStep < minStep )
                                    minStep = nextStep;
                            }
                            else
                            {
                                if( nextStep > minStep )
                                    minStep = nextStep;
                            }
                        }

                        for( int i = 0; i < _attractorEphemerides.Length; i++ )
                        {
                            _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
                        }

                        ut += step;
                        step = minStep;

                        var temp = _currentStateAttractors;
                        _currentStateAttractors = _nextStateAttractors;
                        _nextStateAttractors = temp;
                    }

                    for( int i = 0; i < _attractorEphemerides.Length; i++ )
                    {
                        _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
                    }
                }

                // Simulate followers in parallel.
                // The simulation can include only followers (e.g. path-based, or maneuver-based, etc).

                if( _followers != null && _followers.Length > 0 )
                {
                    if( interval.maxUT != _initialUT && interval.minUT != _initialUT )
                    {
                        if( forward )
                            MoveFollowersTo( interval.maxUT );
                        else
                            MoveFollowersTo( interval.minUT );
                    }

                    Parallel.For( 0, _followers.Length, bodyIndex =>
                    // for( int bodyIndex = 0; bodyIndex < _followers.Length; bodyIndex++ )
                    {
                        TrajectoryStateVector[] currentStateAttractors = new TrajectoryStateVector[_currentStateAttractors.Length];
                        TrajectoryStateVector[] currentStateFollowers = _currentStateFollowers;
                        TrajectoryStateVector[] nextStateFollowers = _nextStateFollowers;

                        double localUT = startUT;
                        double localStep = originalStep;
                        while( Math.Abs( endUT - localUT ) > STEP_EPSILON )
                        {
                            if( Math.Abs( localStep ) > MaxStepSize )
                            {
                                localStep = forward ? MaxStepSize : -MaxStepSize;
                            }

                            double remainingTime = endUT - localUT;
                            if( forward && localStep > remainingTime )
                            {
                                localStep = remainingTime; // don't overshoot the end time.
                            }
                            else if( !forward && localStep < remainingTime )
                            {
                                localStep = remainingTime; // don't overshoot the end time.
                            }

                            for( int i = 0; i < currentStateAttractors.Length; i++ )
                                currentStateAttractors[i] = _attractorEphemerides[i].Evaluate( localUT );

                            ITrajectoryIntegrator integrator = _followers[bodyIndex];
                            var nextStep = integrator.Step( new TrajectorySimulationContext( localUT, localStep, currentStateFollowers[bodyIndex], -1, currentStateAttractors ), _followerAccelerationProviders[bodyIndex], out nextStateFollowers[bodyIndex] );

                            _followerEphemerides[bodyIndex].InsertAdaptive( localUT, currentStateFollowers[bodyIndex] );

                            localUT += localStep;
                            localStep = nextStep;

                            var temp = currentStateFollowers;
                            currentStateFollowers = nextStateFollowers;
                            nextStateFollowers = temp;
                        }

                        _followerEphemerides[bodyIndex].InsertAdaptive( localUT, currentStateFollowers[bodyIndex] );

                        // The local arrays might've been ended up swapped relative to the original arrays.
                        _currentStateFollowers[bodyIndex] = currentStateFollowers[bodyIndex];
                        _nextStateFollowers[bodyIndex] = nextStateFollowers[bodyIndex];
                    } );
                }

                Profiler.EndSample();
            }
            finally
            {
                _isSimulating = false;
            }
        }
    }
}