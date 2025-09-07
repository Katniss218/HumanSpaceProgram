using HSP.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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

        public bool NearZero()
        {
            return (maxUT - minUT) < 1e-10;
        }
    }

    public sealed class TrajectorySimulator2 : IReadonlyTrajectorySimulator
    {
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

        private ITrajectoryTransform[] _attractorCache;

        /// <summary>
        /// Initializes a new instance of the TrajectorySimulator2 class.
        /// </summary>
        public TrajectorySimulator2( double step, int count )
        {
            _initialUT = TimeManager.UT;
            this.DefaultStepSize = step;
            // ignore count.
            ResetToCurrent();
        }

        public void SetInitialTime( double ut )
        {
            Debug.LogWarning( "IN" );
            _initialUT = ut;
            ResetToCurrent();
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
                return false;
            return _bodies.ContainsKey( transform );
        }

        public IEnumerable<(ITrajectoryTransform t, IReadonlyEphemeris e)> GetBodies()
        {
            return _bodies.Select( kvp => (kvp.Key, (IReadonlyEphemeris)kvp.Value.ephemeris) );
        }

        public bool TryGetBody( ITrajectoryTransform transform, out IReadonlyEphemeris ephemeris )
        {
            bool x = _bodies.TryGetValue( transform, out var entry );
            ephemeris = entry?.ephemeris;
            return x;
        }

        public bool TryAddBody( ITrajectoryTransform transform )
        {
            if( transform == null )
                return false;

            if( _bodies.ContainsKey( transform ) )
                return false;

            bool wasAdded = _bodies.ContainsKey( transform );
            if( wasAdded )
                return false;

            _staleToAdd.Add( transform );
            _staleAttractorChanged |= transform.IsAttractor;
            _isStale = true;
            return true;
        }

        public bool TryRemoveBody( ITrajectoryTransform transform )
        {
            if( transform == null )
                return false;

            if( !_bodies.TryGetValue( transform, out var entry ) )
                return false;

            bool wasRemoved = _bodies.Remove( transform );
            if( !wasRemoved )
                return false;

            _staleToRemove.Add( transform );
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
            _staleAttractorChanged = true;
            _isStale = true;
        }

        // 'MoveTo' methods move assuming the simulation is not stale.
        public void MoveTo( SimulationDirection direction )
        {
            TimeInterval i = GetSimulatedInterval();
            if( i.NearZero() )
                return;

            if( direction == SimulationDirection.Forward )
                MoveTo( i.maxUT );
            else
                MoveTo( i.minUT );
        }

        public void MoveTo( double ut )
        {
            foreach( var entry in _bodies.Values )
            {
                if( entry.isAttractor )
                    _currentStateAttractors[entry.timestepperIndex] = entry.ephemeris.Evaluate( ut );
                else
                    _currentStateFollowers[entry.timestepperIndex] = entry.ephemeris.Evaluate( ut );
            }
        }

        /// <summary>
        /// Resets the simulation state to the current game time.
        /// </summary>
        public void ResetToCurrent()
        {
            foreach( var (body, entry) in _bodies )
            {
                entry.ephemeris.Clear();
            }
#warning TODO - resetting maybe doesn't work?
        }

        public TrajectoryStateVector GetStateVector( double ut, ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The trajectory transform '{transform}' is not registered in the simulator.", nameof( transform ) );

            return entry.ephemeris.Evaluate( ut, Ephemeris2.Side.IncreasingUT );
        }

        public bool TryGetStateVector( double ut, ITrajectoryTransform transform, out TrajectoryStateVector stateVector )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

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
            if( !_bodies.TryGetValue( transform, out var bodyEntry ) )
                throw new ArgumentException( $"The trajectory transform '{transform}' is not registered in the simulator.", nameof( transform ) );

            // Current doesn't need to evaluate the ephemeris. The data is already in the timestepper.
            if( bodyEntry.isAttractor )
                return _currentStateAttractors[bodyEntry.timestepperIndex];
            else
                return _currentStateFollowers[bodyEntry.timestepperIndex];
        }

        public void ResetStateVector( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );
#warning    TODO - this is called before _bodies is updated in fixstale, so it will throw.
            if( !_bodies.ContainsKey( transform ) ) return;
            //  throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

            _staleExisting.Add( transform );
            _isStale = true;
        }


        /// <summary>
        /// Marks the body as stale, meaning that the state vector stored in the simulation, and the ephemerides no longer match the actual body's state vector in the game.
        /// </summary>
        public void MarkStale( ITrajectoryTransform transform )
        {
            if( transform == null )
                return;

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

        //public void MarkStale( ITrajectoryTransform transform, double fromUT, double toUT )
        //{

        //}

        private void FixStale( SimulationDirection direction )
        {
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
                        Debug.Log( "STALE" );
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


            // Update head and tail UT
            if( _staleAttractorChanged )
            {
                ResetToCurrent();
            }

            // body can theoretically be marked as partially stale, if e.g. a maneuver node was added in the middle of the ephemeris.
            // that's for later tho.
            _staleAttractorChanged = false;
            _isStale = false;
        }

        static double GetMiddleValue( double a, double b, double c )
        {
            if( (a <= b && b <= c) || (c <= b && b <= a) ) return b;
            if( (b <= a && a <= c) || (c <= a && a <= b) ) return a;
            return c;
        }

        public TimeInterval GetSimulatedInterval()
        {
            if( _bodies.Count == 0 )
                return new TimeInterval( _initialUT, _initialUT );

            double headUT = double.MinValue;
            double tailUT = double.MaxValue;
            foreach( var (_, entry) in _bodies )
            {
                if( entry.ephemeris.Count == 0 )
                    return new TimeInterval( _initialUT, _initialUT );

                if( entry.ephemeris.HighUT > headUT )
                    headUT = entry.ephemeris.HighUT;
                if( entry.ephemeris.LowUT < tailUT )
                    tailUT = entry.ephemeris.LowUT;
            }

            return new TimeInterval( tailUT, headUT );
        }

        public void Simulate( double targetUT )
        {
            Debug.Log( "sim: " + TimeManager.UT + " : " + targetUT );

            TimeInterval i = GetSimulatedInterval();

            double fromUT = GetMiddleValue( i.minUT, i.maxUT, targetUT );
            if( !this._isStale && targetUT <= i.maxUT && targetUT >= i.minUT )
            {
                return;
            }

            Simulate_Internal( fromUT, targetUT );
        }

        public async Task SimulateAsync( double targetUT )
        {
            TimeInterval i = GetSimulatedInterval();

            double fromUT = GetMiddleValue( i.minUT, i.maxUT, targetUT );
            if( !this._isStale && targetUT <= i.maxUT && targetUT >= i.minUT )
            {
                return;
            }

            await Task.Run( () => Simulate_Internal( fromUT, targetUT ) );
        }
#warning TODO - when I attach a debugger here, it magically changes how it works wtf
        private void Simulate_Internal( double startUT, double endUT )
        {
            var newDirection = _direction;
            if( Math.Abs( endUT - startUT ) > 1e-10 )
                newDirection = endUT > startUT ? SimulationDirection.Forward : SimulationDirection.Backward;

            if( this._isStale )
            {
                this.FixStale( newDirection );
            }

            if( newDirection != _direction )
            {
                MoveTo( newDirection );
            }
#warning TODO - after 1 step, follower positions are not updated. because timemanager.UT is updated in the constructor.
            // the actual way to fix that would be to simulate 'this frame', so we need to explicitly set the starting UT before we start simulating!
            Debug.Log( "sim2: " + TimeManager.UT + " : " + _currentStateFollowers[0] );

            _direction = newDirection;

            // simulate some bodies. use the already simulated bodies (if any) to retrieve state vectors from ephemerides.
            // only followers can be simulated using ephemerides.

            // use a heavy per-step integrator with larger step size.

            // we'll call 'simulate' to perform a chunk of work at a given time?
            // selectable which followers should be included in the simulation. where though? maybe just not add them to the simulation at all?

            // find what we actually need to simulate, and the interval to simulate from/to.

            if( _attractors == null || _attractors.Length == 0 )
                return;

            const double EPSILON = 1e-4;
            double ut = startUT;
            double step = _direction == SimulationDirection.Forward ? DefaultStepSize : -DefaultStepSize;
            double originalStep = step;

            /*{
                while( endUT - ut > 1e-4 )
                {
                    if( step > MaxStepSize )
                    {
                        step = MaxStepSize;
                    }

                    if( step > (endUT - ut) )
                    {
                        step = endUT - ut; // don't overshoot the end time.
                    }

                    // Order in which the bodies are updated doesn't matter, since we're setting 'next' and that's not used in calculations until the... next step.

                    double minStep = double.MaxValue;
                    for( int i = 0; i < _currentStateAttractors.Length; i++ )
                    {
                        ITrajectoryIntegrator integrator = _attractors[i];
                        double step2 = integrator.Step( new TrajectorySimulationContext( ut, step, _currentStateAttractors[i], i, _currentStateAttractors ), _attractorAccelerationProviders[i], out _nextStateAttractors[i] );

                        if( step2 < minStep )
                        {
                            minStep = step2;
                        }
                    }

                    for( int i = 0; i < _currentStateFollowers.Length; i++ )
                    {
                        ITrajectoryIntegrator integrator = _followers[i];
                        double step2 = integrator.Step( new TrajectorySimulationContext( ut, step, _currentStateFollowers[i], -1, _currentStateAttractors ), _followerAccelerationProviders[i], out _nextStateFollowers[i] );

                        if( step2 < minStep )
                        {
                            minStep = step2;
                        }
                    }

                    // when ran far enough, store the points as ephemerides in the corresponding ephemeris structs.
                    for( int i = 0; i < _attractorEphemerides.Length; i++ )
                    {
                        _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
                    }
                    for( int i = 0; i < _followerEphemerides.Length; i++ )
                    {
                        _followerEphemerides[i].InsertAdaptive( ut, _currentStateFollowers[i] );
                    }

                    ut += step;
                    step = minStep;

                    // Swapping the reference is enough, we don't care what (if anything) is in the 'target' structs.
                    var temp = _currentStateAttractors;
                    _currentStateAttractors = _nextStateAttractors;
                    _nextStateAttractors = temp;

                    temp = _currentStateFollowers;
                    _currentStateFollowers = _nextStateFollowers;
                    _nextStateFollowers = temp;
                }

                for( int i = 0; i < _attractorEphemerides.Length; i++ )
                {
                    _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
                }
                for( int i = 0; i < _followerEphemerides.Length; i++ )
                {
                    _followerEphemerides[i].InsertAdaptive( ut, _currentStateFollowers[i] );
                }
                Debug.Log( "sim3: " + TimeManager.UT + " : " + _currentStateFollowers[0] );
                return;
            }*/

            double maxStepSizeSigned = _direction == SimulationDirection.Forward ? MaxStepSize : -MaxStepSize;
            bool forward = _direction == SimulationDirection.Forward;

#warning TODO - only simulate attractors is needed (no ephemeris data for the simulated interval) - because simulation will recalculate the followers often, while the attractors aren't affected.
            // Forward simulation
            while( endUT - ut > EPSILON )
            {
                if( step > MaxStepSize )
                {
                    step = MaxStepSize;
                }

                if( step > (endUT - ut) )
                {
                    step = endUT - ut; // don't overshoot the end time.
                }

                double minStep = double.MaxValue;
                for( int i = 0; i < _attractors.Length; i++ )
                {
                    ITrajectoryIntegrator integrator = _attractors[i];
                    var nextStep = integrator.Step( new TrajectorySimulationContext( ut, step, _currentStateAttractors[i], i, _currentStateAttractors ), _attractorAccelerationProviders[i], out _nextStateAttractors[i] );

                    if( (forward && nextStep < minStep) || nextStep > minStep )
                    {
                        minStep = nextStep;
                    }
                }

                for( int i = 0; i < _attractorEphemerides.Length; i++ )
                {
                    _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
                }

                ut += step;
                //step = minStep;

                var temp = _currentStateAttractors;
                _currentStateAttractors = _nextStateAttractors;
                _nextStateAttractors = temp;
            }

            for( int i = 0; i < _attractorEphemerides.Length; i++ )
            {
                _attractorEphemerides[i].InsertAdaptive( ut, _currentStateAttractors[i] );
            }

#warning TODO - only simulate followers from their own ephemeris end point to the target time (if a given follower doesn't have ephemeris data for the UT).
            // Simulate individual followers in parallel.
            if( _followers != null && _followers.Length > 0 )
            {
                //Parallel.For( 0, _followers.Length, bodyIndex =>
                for( int bodyIndex = 0; bodyIndex < _followers.Length; bodyIndex++ )
                {
                    TrajectoryStateVector[] currentStateAttractors = new TrajectoryStateVector[_currentStateAttractors.Length];
                    TrajectoryStateVector[] currentStateFollowers = _currentStateFollowers;
                    TrajectoryStateVector[] nextStateFollowers = _nextStateFollowers;

                    double localUT = startUT;
                    double localStep = originalStep;
                    while( endUT - localUT > EPSILON )
                    {
                        if( localStep > MaxStepSize )
                        {
                            localStep = MaxStepSize;
                        }

                        if( localStep > (endUT - localUT) )
                        {
                            localStep = endUT - localUT; // don't overshoot the end time.
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

                    // needed
                    _currentStateFollowers[bodyIndex] = currentStateFollowers[bodyIndex];
                    _nextStateFollowers[bodyIndex] = nextStateFollowers[bodyIndex];
                } //);
            }

            Debug.Log( "sim3: " + TimeManager.UT + " : " + _currentStateFollowers[0] );
        }
    }
}