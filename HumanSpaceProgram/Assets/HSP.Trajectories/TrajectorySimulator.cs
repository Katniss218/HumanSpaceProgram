using HSP.Trajectories.TrajectoryIntegrators;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;

namespace HSP.Trajectories
{
    public class TrajectorySimulator : IReadonlyTrajectorySimulator
    {
        public class Entry
        {
            public bool isAttractor;
            public int timestepperIndex; // either indexes to the attractor or follower arrays, depending on the isAttractor.
            public Ephemeris2 ephemeris;
        }

        // timestepper

        protected ITrajectoryIntegrator[] _attractors;
        protected ITrajectoryStepProvider[][] _attractorAccelerationProviders;

        protected ITrajectoryIntegrator[] _followers;
        protected ITrajectoryStepProvider[][] _followerAccelerationProviders;

        protected TrajectoryStateVector[] _currentStateAttractors;
        protected TrajectoryStateVector[] _nextStateAttractors;

        protected TrajectoryStateVector[] _currentStateFollowers;
        protected TrajectoryStateVector[] _nextStateFollowers;

#warning TODO - simulator should keep ground truth values for time (t_0) and have methods to update them. It should then predict forwards in time up to some t_p
        protected double _ut;
        protected double _step;

        /// <summary>
        /// Maximum global step size for the simulation, in [s].
        /// </summary>
        public double MaxStepSize { get; set; } = 1.0;

        protected Ephemeris2[] _attractorEphemerides;
        protected Ephemeris2[] _followerEphemerides;

        public int GetAttractorIndex( ITrajectoryTransform trajectoryTransform )
        {
            if( trajectoryTransform == null )
                return -1;

            if( !_bodies.TryGetValue( trajectoryTransform, out var entry ) )
                return -1;

            if( !entry.isAttractor )
                return -1;

            return entry.timestepperIndex;
        }

        public double UT => _ut;

        //

        public virtual ReadOnlySpan<ITrajectoryTransform> Attractors => _attractorCache;
        ITrajectoryTransform[] _attractorCache;
        Dictionary<ITrajectoryTransform, Entry> _bodies = new();

        HashSet<ITrajectoryTransform> _staleBodies = new();
        bool _isStaleAttractor;
        bool _staleBodyCountChanged;

        bool _staleEphemerisLengthChanged;
        int _ephemerisLength;
        //double _ephemerisDuration;
        //double _ephemerisTimeResolution;

        //public TrajectorySimulator( double ut, double step, double ephemerisTimeResolution, double ephemerisDuration )
        public TrajectorySimulator( double ut, double step, int ephemerisLength )
        {
            this._ut = ut;
            this._step = step;
            _isStaleAttractor = true; // will set up everything on first step.
            _staleBodyCountChanged = true;
            _ephemerisLength = ephemerisLength;
            //_ephemerisDuration = ephemerisDuration;
            //_ephemerisTimeResolution = ephemerisTimeResolution;
        }

        public bool HasBody( ITrajectoryTransform trajectoryTransform )
        {
            if( trajectoryTransform == null )
                return false;

            return _bodies.ContainsKey( trajectoryTransform );
        }

        public IEnumerable<(ITrajectoryTransform t, IReadonlyEphemeris e)> GetBodies()
        {
            return _bodies.Select( kvp => (kvp.Key, (IReadonlyEphemeris)kvp.Value.ephemeris) );
        }

        public virtual bool AddBody( ITrajectoryTransform transform )
        {
            Ephemeris2 ephemeris = new Ephemeris2( 1000, 0.01 );
            bool wasAdded = _bodies.TryAdd( transform, new Entry() { timestepperIndex = -1, isAttractor = transform.IsAttractor, ephemeris = ephemeris } );
            if( !wasAdded )
                return false;

            _staleBodies.Add( transform );
            _isStaleAttractor |= transform.IsAttractor;
            _staleBodyCountChanged = true;
            return true;
        }

        public virtual bool RemoveBody( ITrajectoryTransform transform )
        {
            bool wasRemoved = _bodies.Remove( transform );
            if( !wasRemoved )
                return false;

            _staleBodies.Add( transform );
            _isStaleAttractor |= transform.IsAttractor;
            _staleBodyCountChanged = true;
            return true;
        }

        public virtual void MarkBodyDirty( ITrajectoryTransform trajectoryTransform )
        {
            if( !_bodies.TryGetValue( trajectoryTransform, out var bodyEntry ) )
                throw new ArgumentException( $"The trajectory transform '{trajectoryTransform}' is not registered in the simulator.", nameof( trajectoryTransform ) );

            _staleBodies.Add( trajectoryTransform );
            _isStaleAttractor |= (trajectoryTransform.IsAttractor | bodyEntry.isAttractor); // if is attractor or was attractor (it could've changed).
        }

        //public virtual void SetEphemerisLength( double ephemerisDuration )
        public virtual void SetEphemerisLength( int ephemerisLength )
        {
            if( ephemerisLength <= 0 )
                throw new ArgumentOutOfRangeException( nameof( ephemerisLength ), "The ephemeris length must be greater than zero." );

            _staleEphemerisLengthChanged = true;
            this._ephemerisLength = ephemerisLength;
            //this._ephemerisDuration = ephemerisDuration;
        }

        public virtual void Clear()
        {
            _staleBodyCountChanged = true;
            _bodies.Clear();
        }

        public virtual TrajectoryStateVector GetCurrentStateVector( ITrajectoryTransform trajectoryTransform )
        {
            if( !_bodies.TryGetValue( trajectoryTransform, out var bodyEntry ) )
                throw new ArgumentException( $"The trajectory transform '{trajectoryTransform}' is not registered in the simulator.", nameof( trajectoryTransform ) );

            // Current doesn't need to evaluate the ephemeris. The data is already in the timestepper.
            if( bodyEntry.isAttractor )
                return _currentStateAttractors[bodyEntry.timestepperIndex];
            else
                return _currentStateFollowers[bodyEntry.timestepperIndex];
        }

        public virtual bool TryGetStateVector( double ut, ITrajectoryTransform trajectoryTransform, out TrajectoryStateVector stateVector )
        {
            if( !_bodies.TryGetValue( trajectoryTransform, out var bodyEntry ) )
                throw new ArgumentException( $"The trajectory transform '{trajectoryTransform}' is not registered in the simulator.", nameof( trajectoryTransform ) );

            var emphemeris = bodyEntry.ephemeris;
            if( emphemeris.HighUT < ut || emphemeris.LowUT > ut )
            {
                stateVector = default;
                return false;
            }

            stateVector = emphemeris.Evaluate( ut );
            return true;
        }

        public virtual void ResetStateVector( ITrajectoryTransform trajectoryTransform )
        {
            if( !_bodies.ContainsKey( trajectoryTransform ) )
                throw new ArgumentException( $"The trajectory transform '{trajectoryTransform}' is not registered in the simulator.", nameof( trajectoryTransform ) );

            _staleBodies.Add( trajectoryTransform );
            _isStaleAttractor |= trajectoryTransform.IsAttractor;

            // staleness will fix the state vector getting it from the actual trajtransform.
        }

        protected virtual void FixStale()
        {
            _attractorCache = _bodies.Keys
                .Where( t => t.IsAttractor )
                .ToArray();

            // Keep references to the old arrays to be able to lookup (via the timestepperIndex) and not reallocate everything, since not every part has changed.
            int numAttractors = _attractorCache.Length;
            int numFollowers = _bodies.Count - numAttractors;

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

            Profiler.BeginSample( "_staleBodyCountChanged" );
            if( _staleBodyCountChanged )
            {
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
            }
            Profiler.EndSample();

            Profiler.BeginSample( "_isStaleAttractor || _staleBodyCountChanged" );
            if( _isStaleAttractor || _staleBodyCountChanged )
            {
                // If an attractor has changed, we can't use the previously calculated ephemerides, so we reset them.

                int attractorIndex = 0;
                int followerIndex = 0;
                foreach( var (body, entry) in _bodies )
                {
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
                }
            }
            else
            {
                foreach( var body in _staleBodies )
                {
                    var entry = _bodies[body];
                    var index = entry.timestepperIndex;

                    // body is not an attractor because they're handled differently above.
                    _followers[index] = body.Integrator;
                    _followerAccelerationProviders[index] = body.AccelerationProviders.ToArray();
                    _currentStateFollowers[index] = body.GetBodyState();
                    _followerEphemerides[index] = entry.ephemeris;
                    entry.ephemeris.Clear();
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample( "_staleEphemerisLengthChanged" );
            if( _staleEphemerisLengthChanged )
            {
                foreach( var ephemeris in _attractorEphemerides )
                {
                    //ephemeris.SetDuration( ephemeris.LowUT + _ephemerisDuration + ephemeris.TimeResolution /* padding */, ephemeris.LowUT );
                    ephemeris.SetCapacity( _ephemerisLength );
                }
                foreach( var ephemeris in _followerEphemerides )
                {
                    ephemeris.SetCapacity( _ephemerisLength );
                }
                _staleEphemerisLengthChanged = false;
#warning TODO - reset UT (actually, keep the reference UT and ensure that the ephemerides have values between the from/to).
            }
            Profiler.EndSample();

            _isStaleAttractor = false;
            _staleBodyCountChanged = false;
            _staleBodies.Clear();
        }

        /// <summary>
        /// Prolongs the ephemerides up to the specified UT.
        /// </summary>
        public virtual void Simulate( double endUT )
        {
            Profiler.BeginSample( "TrajectorySimulator.Simulate" );
            if( _bodies.Count == 0 )
            {
                _ut = endUT;
                return;
            }

            if( _staleEphemerisLengthChanged || _staleBodies.Count > 0 )
            {
                FixStale();
            }

            if( _ut >= endUT )
            {
                // Timestepper simulated past the desired time.
                return;
            }

#warning TODO - when reversing (if an ephemeris has time points further 'forward' than endUT), override it.

#warning TODO - stale flight plan followers need to actually have their ephemerides recalculated.
            // sort of a sliding window that should have valid data in that interval

            while( endUT - _ut > 1e-4 )
            {
                if( _step > MaxStepSize )
                {
                    _step = MaxStepSize;
                }

                if( _step > (endUT - _ut) )
                {
                    _step = endUT - _ut; // don't overshoot the end time.
                }

                // Order in which the bodies are updated doesn't matter, since we're setting 'next' and that's not used in calculations until the... next step.

                double minStep = double.MaxValue;
                for( int i = 0; i < _currentStateAttractors.Length; i++ )
                {
                    ITrajectoryIntegrator integrator = _attractors[i];
                    double step = integrator.Step( new TrajectorySimulationContext( _ut, _step, _currentStateAttractors[i], _currentStateAttractors ), _attractorAccelerationProviders[i], out _nextStateAttractors[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                for( int i = 0; i < _currentStateFollowers.Length; i++ )
                {
                    ITrajectoryIntegrator integrator = _attractors[i];
                    double step = integrator.Step( new TrajectorySimulationContext( _ut, _step, _currentStateFollowers[i], _currentStateAttractors ), _followerAccelerationProviders[i], out _nextStateFollowers[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                // when ran far enough, store the points as ephemerides in the corresponding ephemeris structs.
                for( int i = 0; i < _attractorEphemerides.Length; i++ )
                {
                    _attractorEphemerides[i].InsertAdaptive( _ut, _currentStateAttractors[i] );
                }
                for( int i = 0; i < _followerEphemerides.Length; i++ )
                {
                    _followerEphemerides[i].InsertAdaptive( _ut, _currentStateFollowers[i] );
                }

                _ut += _step;
                _step = minStep;

                // Swapping the reference is enough, we don't care what (if anything) is in the 'target' structs.
                var temp = _currentStateAttractors;
                _currentStateAttractors = _nextStateAttractors;
                _nextStateAttractors = temp;

                temp = _currentStateFollowers;
                _currentStateFollowers = _nextStateFollowers;
                _nextStateFollowers = temp;
            }

            _ut = endUT; // Setting to the actual value at the end prevents accumulation of small precision errors due to repeated addition of delta-time.
            Profiler.EndSample();
        }
    }
}