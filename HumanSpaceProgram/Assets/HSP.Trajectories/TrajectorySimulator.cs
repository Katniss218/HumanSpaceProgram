using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace HSP.Trajectories
{
    public interface IReadonlyTrajectorySimulator
    {
        public IReadOnlyList<ITrajectoryTransform> Attractors => ;

        public TrajectoryBodyState GetCurrentStateVector( ITrajectoryTransform trajectoryTransform );

        public void SetCurrentStateVector( ITrajectoryTransform trajectoryTransform, TrajectoryBodyState stateVector );
    }

    public class TrajectorySimulatorTimestepper
    {
        protected readonly TrajectorySimulator _simulator;

        protected ITrajectoryIntegrator[] _attractors;
        protected IAccelerationProvider[][] _attractorAccelerationProviders;

        protected ITrajectoryIntegrator[] _followers;
        protected IAccelerationProvider[][] _followerAccelerationProviders;

        protected TrajectoryBodyState[] _currentAttractors;
        protected TrajectoryBodyState[] _nextAttractors;

        protected TrajectoryBodyState[] _currentFollowers;
        protected TrajectoryBodyState[] _nextFollowers;

        protected double _ut;
        protected double _step;

        public TrajectorySimulatorTimestepper( TrajectorySimulator simulator, double ut )
        {
            this._simulator = simulator;
            this._ut = ut;
        }

        /// <summary>
        /// Prolongs the ephemerides up to the specified UT.
        /// </summary>
        public virtual void Simulate( double endUT )
        {
#warning TODO - ensure that the simulation runs long enough to update every ephemeris. And clean up the time stepper arrays

            // run forward or backward, depending on endUT
            // theoretical max length of the ephemeris is fixed

            while( _ut < endUT )
            {
                // prolong
                double minStep = double.MaxValue;
                for( int i = 0; i < _currentAttractors.Length; i++ )
                {
                    var body = _attractors[i];
                    double step = body.Step( _step, _currentAttractors[i], _attractorAccelerationProviders[i], out _nextAttractors[i] );

                    if( step < minStep )
                    {
                        minStep = step;
                    }
                }

                for( int i = 0; i < _currentFollowers.Length; i++ )
                {
                    var body = _attractors[i];
                    double step = body.Step( _step, _currentFollowers[i], _followerAccelerationProviders[i], out _nextFollowers[i] );

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
                        _attractorEphemerides[i].AppendToFront( _currentAttractors[i] );
                    }
                }



                var temp = _currentAttractors;
                _currentAttractors = _nextAttractors;
                _nextAttractors = temp;

                temp = _currentFollowers;
                _currentFollowers = _nextFollowers;
                _nextFollowers = temp;
            }

            _ut = endUT; // Setting to the actual value prevents accumulation of small precision errors due to repeated addition.
        }
    }

    public class TrajectorySimulator : IReadonlyTrajectorySimulator
    {
        protected TrajectorySimulatorTimestepper _timestepper;

        ITrajectoryTransform[] _attractorCache;
        HashSet<ITrajectoryTransform> _bodies;

        Ephemeris[] _attractorEphemerides;
        Ephemeris[] _followerEphemerides;

        public virtual IReadOnlyList<ITrajectoryTransform> Attractors => _attractorCache;

        HashSet<ITrajectoryTransform> _staleBodies;
        bool _isStaleAttractor;

        public TrajectorySimulator( double ut )
        {
            _timestepper = new TrajectorySimulatorTimestepper( this, ut );
        }

        public virtual void AddBody( ITrajectoryTransform transform )
        {
            bool ret = _bodies.Add( transform );

            if( ret )
            {
                _staleBodies.Add( transform );
                _isStaleAttractor |= transform.IsAttractor;
            }
        }

        public virtual void RemoveBody( ITrajectoryTransform transform )
        {
            bool ret = _bodies.Remove( transform );

            if( ret )
            {
                _staleBodies.Add( transform );
                _isStaleAttractor |= transform.IsAttractor;
            }
        }

        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public virtual void Reset( Ephemeris[] attractorEphemerides, Ephemeris[] followerEphemerides, double ut )
        {
#warning TODO - instead of ephemerides, we need full data
            // assume the ephemerides are the 'ground truth', sync the timestepper to them.
            throw new NotImplementedException();
        }

        public virtual TrajectoryBodyState GetCurrentStateVector( ITrajectoryTransform trajectoryTransform )
        {
            throw new NotImplementedException();

        }

        public virtual void SetCurrentStateVector( ITrajectoryTransform trajectoryTransform, TrajectoryBodyState stateVector )
        {
            throw new NotImplementedException();

            if( trajectoryTransform.IsAttractor )
            {
                // reset ephemerides for all.
            }
        }

        protected virtual void FixStale()
        {
            if( _attractorChanged )
            {
                Reset( bodyList );

#warning TODO - reset bodies and ephemeris of existing bodies.
            }
            else
            {
#warning TODO update ephemeris length if changed.
                foreach( var transform in _staleList )
                {
                    if( _transforms.Contains( transform ) )
                        simulator.AddBody( transform );
                    else
                        simulator.RemoveBody( transform );
                }
            }

            _attractorCache = _bodies
                .Where( t => t.IsAttractor )
                .ToArray();

            _isStaleAttractor = false;
            _staleBodies.Clear();
        }

        public virtual void Simulate( double endUT )
        {
            if( _staleBodies.Count > 0 )
            {
                FixStale();
            }
            _timestepper.Simulate( endUT );
        }
    }
}