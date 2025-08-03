using System.Collections.Generic;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents anything that can influence a body's trajectory.
    /// </summary>
    public interface ITrajectoryIntegrator
    {
        /// <summary>
        /// Steps the trajectory forward by the specified time step.
        /// </summary>
        /// <param name="step">The time step</param>
        /// <param name="self">The state vector of this body at T.</param>
        /// <param name="accelerationProviders"></param>
        /// <param name="nextSelf">The resulting state vector of this body at T+step.</param>
        /// <returns>The desired time step to use next time. Return <paramref name="step"/> if the implementation doesn't support variable time stepping.</returns>
        public double Step( double step, TrajectoryBodyState self, IEnumerable<IAccelerationProvider> accelerationProviders, out TrajectoryBodyState nextSelf );
    }
}