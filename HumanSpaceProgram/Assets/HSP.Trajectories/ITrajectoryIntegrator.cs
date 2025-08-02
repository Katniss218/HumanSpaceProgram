using System.Collections.Generic;

namespace HSP.Trajectories
{
    public interface ITrajectoryIntegrator
    {
        /// <summary>
        /// Steps the trajectory forward by the specified time step.
        /// </summary>
        /// <param name="step">The time step</param>
        /// <param name="self">The state vector of this body at T.</param>
        /// <param name="accelerationProviders"></param>
        /// <param name="nextSelf">The resulting state vector of this body at T+step.</param>
        /// <returns>The desired time step to use next time.</returns>
        public double? Step( double step, TrajectoryBodyState self, IEnumerable<IAccelerationProvider> accelerationProviders, out TrajectoryBodyState nextSelf );
    }
}