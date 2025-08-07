using UnityEngine;

namespace HSP.Trajectories
{
    public interface ITrajectoryStepProvider
    {
        /// <summary>
        /// Clones the step provider and assigns the specified simulation context to the clone.
        /// </summary>
        public ITrajectoryStepProvider Clone( ITrajectoryTransform self, IReadonlyTrajectorySimulator simulator );

        /// <summary>
        /// Gets the acceleration at the specified UT, using its internal simulation context.
        /// </summary>
        public Vector3Dbl GetAcceleration( TrajectorySimulationContext context );

        /// <summary>
        /// Gets the mass (NOT mass derivative) at the specified UT, using its internal simulation context.
        /// </summary>
        public double? GetMass( TrajectorySimulationContext context ); // useful for maneuver nodes I guess. We Also need one for staging events?
    }
}