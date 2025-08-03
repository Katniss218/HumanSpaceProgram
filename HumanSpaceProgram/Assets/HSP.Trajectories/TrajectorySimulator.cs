using System;
using System.Collections.Generic;

namespace HSP.Trajectories
{
    public interface IReadonlyTrajectorySimulator
    {
        TrajectoryBodyState GetCurrentStateVector( TrajectoryTransform trajectoryTransform );

        void SetCurrentStateVector( TrajectoryTransform trajectoryTransform, TrajectoryBodyState stateVector );
    }

    public class TrajectorySimulator : IReadonlyTrajectorySimulator
    {
        // Timestepper (simulator):
        private ITrajectoryIntegrator[] _attractors;
        private IAccelerationProvider[][] _attractorAccelerationProviders;
        private TrajectoryBodyState[] _currentAttractors;
        private TrajectoryBodyState[] _nextAttractors;
        private ITrajectoryIntegrator[] _followers;
        private IAccelerationProvider[][] _followerAccelerationProviders;
        private TrajectoryBodyState[] _currentFollowers;
        private TrajectoryBodyState[] _nextFollowers;
        private double _ut;

        private double _step;

        // Calculated ephemerides:
        Ephemeris[] _attractorEphemerides;
        Ephemeris[] _followerEphemerides;

        public virtual IReadOnlyList<TrajectoryTransform> Attractors => ;

        public TrajectorySimulator( double ut )
        {

        }

        public virtual void Reset( Ephemeris[] attractorEphemerides, Ephemeris[] followerEphemerides, double ut )
        {
#warning TODO - instead of ephemerides, we need full data
            // assume the ephemerides are the 'ground truth', sync the timestepper to them.
            throw new NotImplementedException();
        }

        public virtual TrajectoryBodyState GetCurrentStateVector( TrajectoryTransform trajectoryTransform )
        {
            throw new NotImplementedException();

        }

        public virtual void SetCurrentStateVector( TrajectoryTransform trajectoryTransform, TrajectoryBodyState stateVector )
        {
            throw new NotImplementedException();

            if( trajectoryTransform.IsAttractor )
            {
                // reset ephemerides for all.
            }
        }

        public virtual void AddBody( TrajectoryTransform transform )
        {
            throw new NotImplementedException();

        }

        public virtual void RemoveBody( TrajectoryTransform transform )
        {
            throw new NotImplementedException();

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
}