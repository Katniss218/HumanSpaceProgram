using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace HSP.Trajectories
{
    public sealed class TrajectorySimulator2// : IReadonlyTrajectorySimulator
    {
        // Timestepper stuff (individual arrays).

        /// <summary>
        /// Gets or sets the maximum step size for the simulation, in [s]. <br/>
        /// A single step will never exceed this value.
        /// </summary>
        public double MaxStepSize { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the target high UT for the simulation, in [s].
        /// </summary>
        public double TargetUT { get; set; } = 100.0;

        /// <summary>
        /// Gets the UT of the highest time where the entire simulation is valid.
        /// </summary>
        public double HighUT => _headUT;
        public double LowUT => _tailUT;
        private double _headUT;
        private double _tailUT; // usually equal to TimeManager.UT, but store it here because that can change during the frame/thread safety.

        private Ephemeris2[] _attractorEphemerides;
        private Ephemeris2[] _followerEphemerides;

        // Storage stuff.

        private readonly struct Entry
        {
            public readonly bool isAttractor;
            public readonly int timestepperIndex;
            public readonly Ephemeris2 ephemeris;
        }

        private Dictionary<ITrajectoryTransform, Entry> _bodies = new();

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

        public TrajectoryStateVector GetStateVector( double ut, ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

            return entry.ephemeris.Evaluate( ut, Ephemeris2.Side.IncreasingUT );
        }

        public double GetHighUT( ITrajectoryTransform transform )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );

            if( !_bodies.TryGetValue( transform, out var entry ) )
                throw new ArgumentException( $"The transform is not part of the simulation.", nameof( transform ) );

            return entry.ephemeris.HighUT;
        }

        /// <summary>
        /// Ensures that the ephemerides are simulated at least up to the given time.
        /// </summary>
        public void EnsureSimulated( double headUT )
        {
            if( _headUT >= headUT )
            {
                return;
            }

            Simulate( headUT );
        }

        /// <summary>
        /// Marks the body as stale, meaning that the state vector stored in the simulation, and the ephemerides no longer match the actual body's state vector in the game.
        /// </summary>
        private void MarkStale( ITrajectoryTransform transform )
        {

        }

        private void FixStale()
        {

        }

#warning  TODO - async simulation. Pausable/resumable simulation (assuming nothing went out of sync in between).

        private void Simulate( double targetHeadUT )
        {
            // simulate some bodies. use the already simulated bodies (if any) to retrieve state vectors from ephemerides.
            // only followers can be simulated using ephemerides.

            // use a heavy per-step integrator with larger step size.

            Entry[] _bodiesToSimulate;
            Entry[] _bodiesToGetEphemeris;

            const double EPSILON = 1e-4;

            while( targetHeadUT - _headUT > EPSILON )
            {
                // state vectors from evaluated ephemerides need to be populated into the timestepper arrays as well (it's easier that way).
            }
        }
    }
}