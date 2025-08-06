using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.Trajectories
{
    public interface IReadonlyTrajectorySimulator
    {
        public IReadOnlyList<ITrajectoryTransform> Attractors { get; }

        public TrajectoryStateVector GetCurrentStateVector( ITrajectoryTransform trajectoryTransform );

        public void SetCurrentStateVector( ITrajectoryTransform trajectoryTransform, TrajectoryStateVector stateVector );
    }

    public class TrajectorySimulator : IReadonlyTrajectorySimulator
    {
        public class Entry
        {
            public bool isAttractor;
            public int timestepperIndex; // either indexes to the attractor or follower arrays, depending on the isAttractor.
            public Ephemeris ephemeris;
        }

        // timestepper

        protected ITrajectoryIntegrator[] _attractors;
        protected IAccelerationProvider[][] _attractorAccelerationProviders;

        protected ITrajectoryIntegrator[] _followers;
        protected IAccelerationProvider[][] _followerAccelerationProviders;

        protected TrajectoryStateVector[] _currentStateAttractors;
        protected TrajectoryStateVector[] _nextStateAttractors;

        protected TrajectoryStateVector[] _currentStateFollowers;
        protected TrajectoryStateVector[] _nextStateFollowers;
#error TODO - we need getters for the current state so that the acceleration providers can use them. Potentially wrap them in a lightweight struct as readonly lists?
        protected double _ut;
        protected double _step;

        protected Ephemeris[] _attractorEphemerides;
        protected Ephemeris[] _followerEphemerides;


        //

        public virtual IReadOnlyList<ITrajectoryTransform> Attractors => _attractorCache;
        ITrajectoryTransform[] _attractorCache;
        Dictionary<ITrajectoryTransform, Entry> _bodies;

        HashSet<ITrajectoryTransform> _staleBodies;
        bool _isStaleAttractor;
        bool _staleBodyCountChanged;

        public TrajectorySimulator( double ut )
        {
            this._ut = ut;
            _isStaleAttractor = true; // will set up everything on first step.
            _staleBodyCountChanged = true;
        }

        public virtual bool AddBody( ITrajectoryTransform transform, Ephemeris ephemeris )
        {
            bool wasAdded = _bodies.TryAdd( transform, new Entry() { timestepperIndex = -1, isAttractor = transform.IsAttractor, ephemeris = ephemeris } );
            if( !wasAdded )
                return false;

            _staleBodies.Add( transform );
            _isStaleAttractor |= transform.IsAttractor;
            _staleBodyCountChanged = true;
            return true;
        }

        public virtual bool SetEphemeris( ITrajectoryTransform transform, Ephemeris ephemeris )
        {
            if( !_bodies.TryGetValue( transform, out var entry ) )
                return false;

            entry.ephemeris = ephemeris;
            _staleBodies.Add( transform );
            _isStaleAttractor |= transform.IsAttractor;
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

        public virtual void MarkBodyDirty( ITrajectoryTransform transform )
        {
            _staleBodies.Add( transform );
            _isStaleAttractor |= transform.IsAttractor;
        }

        public virtual void Clear()
        {
            _staleBodyCountChanged = true;
            throw new NotImplementedException();
        }

        public virtual TrajectoryStateVector GetCurrentStateVector( ITrajectoryTransform trajectoryTransform )
        {
            throw new NotImplementedException();

        }

        public virtual void SetCurrentStateVector( ITrajectoryTransform trajectoryTransform, TrajectoryStateVector stateVector )
        {
            throw new NotImplementedException();

            if( trajectoryTransform.IsAttractor )
            {
                _isStaleAttractor = true;
                // reset ephemerides for all.
            }
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

            if( _staleBodyCountChanged )
            {
                _attractors = new ITrajectoryIntegrator[numAttractors];
                _followers = new ITrajectoryIntegrator[numFollowers];
                _attractorAccelerationProviders = new IAccelerationProvider[numAttractors][];
                _followerAccelerationProviders = new IAccelerationProvider[numFollowers][];

                _currentStateAttractors = new TrajectoryStateVector[numAttractors];
                _nextStateAttractors = new TrajectoryStateVector[numAttractors];
                _currentStateFollowers = new TrajectoryStateVector[numFollowers];
                _nextStateFollowers = new TrajectoryStateVector[numFollowers];

                _attractorEphemerides = new Ephemeris[numAttractors];
                _followerEphemerides = new Ephemeris[numFollowers];
            }

            if( _isStaleAttractor )
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

#warning TODO update ephemeris length if changed.


                foreach( var transform in _staleBodies )
                {
                    var ephemeris = _bodies[transform];

                    // fix ephemeris possibly being out ofsync with new pos/vel/etc.

                    // later we simulate from the point where all ephemerides arecalculated, or from 'now' if there is one that hasn't been calculated.
                }
            }

            _isStaleAttractor = false;
            _staleBodyCountChanged = false;
            _staleBodies.Clear();
        }

        /// <summary>
        /// Prolongs the ephemerides up to the specified UT.
        /// </summary>
        public virtual void Simulate( double endUT )
        {
            if( _staleBodies.Count > 0 )
            {
                FixStale();
            }

#warning TODO - ensure that the simulation runs long enough to update every ephemeris. And clean up the time stepper arrays

            // run forward or backward, depending on endUT
            // theoretical max length of the ephemeris is fixed

            while( _ut < endUT )
            {
                // prolong
                double minStep = double.MaxValue;
                for( int i = 0; i < _currentStateAttractors.Length; i++ )
                {
                    var body = _attractors[i];
                    double step = body.Step( _ut, _step, _currentStateAttractors[i], _attractorAccelerationProviders[i], out _nextStateAttractors[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                for( int i = 0; i < _currentStateFollowers.Length; i++ )
                {
                    var body = _attractors[i];
                    double step = body.Step( _ut, _step, _currentStateFollowers[i], _followerAccelerationProviders[i], out _nextStateFollowers[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                _ut += _step;
                _step = minStep;

#warning TODO - all ephemerides here should have the same length? not necessarily! 
                // all attractors will always have the same length.
                // followers need to be at most as long as attractors, but can be shorter.


                // when ran far enough, store the points as ephemerides in the corresponding ephemeris structs.
                for( int i = 0; i < _attractorEphemerides.Length; i++ )
                {
#warning TODO - might need to interpolate and/or append multiple data points if step was larger than the ephemeris' time resolution.
                    if( _ut >= _attractorEphemerides[i].EndUT + _attractorEphemerides[i].TimeResolution )
                    {
                        _attractorEphemerides[i].AppendToFront( _currentStateAttractors[i] );
                    }
                }



                var temp = _currentStateAttractors;
                _currentStateAttractors = _nextStateAttractors;
                _nextStateAttractors = temp;

                temp = _currentStateFollowers;
                _currentStateFollowers = _nextStateFollowers;
                _nextStateFollowers = temp;
            }

            _ut = endUT; // Setting to the actual value prevents accumulation of small precision errors due to repeated addition.

        }
    }
}