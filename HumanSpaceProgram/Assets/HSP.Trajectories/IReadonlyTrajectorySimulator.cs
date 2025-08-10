using System;

namespace HSP.Trajectories
{
    public interface IReadonlyTrajectorySimulator
    {
        /// <summary>
        /// The current simulation time, in [s] since the epoch.
        /// </summary>
        public double UT { get; }

        /// <summary>
        /// Gets all attractors that are taking part in the simulation.
        /// </summary>
        public ReadOnlySpan<ITrajectoryTransform> Attractors { get; }

        /// <summary>
        /// Gets the timestepper index of the given attractor.
        /// </summary>
        public int GetAttractorIndex( ITrajectoryTransform trajectoryTransform );

        /// <summary>
        /// Gets the state vector at time <see cref="IReadonlyTrajectorySimulator.UT"/>
        /// </summary>
        public TrajectoryStateVector GetCurrentStateVector( ITrajectoryTransform trajectoryTransform );

        /// <summary>
        /// Gets the state vector at the specified time.
        /// </summary>
        public bool TryGetStateVector( double ut, ITrajectoryTransform trajectoryTransform, out TrajectoryStateVector stateVector );

        /// <summary>
        /// Resets the simulation data for the specified body. Sets the new state vector to the values returned by the body.
        /// </summary>
        public void ResetStateVector( ITrajectoryTransform trajectoryTransform );
    }
}