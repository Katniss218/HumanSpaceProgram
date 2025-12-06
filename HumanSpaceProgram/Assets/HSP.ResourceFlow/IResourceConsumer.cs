using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Anything that can consume resources. E.g. rocket engine, lightbulb (electricity), vent, etc.
    /// </summary>
    public interface IResourceConsumer
    {
        /// <summary>
        /// Get or set the acceleration of the fluid contents (if any) relative to the container, in container-space, in [m/s^2].
        /// </summary>
        Vector3 FluidAcceleration { get; set; }

        /// <summary>
        /// Get or set the angular velocity of the fluid contents (if any) relative to the container, in container-space, in [rad/s].
        /// </summary>
        Vector3 FluidAngularVelocity { get; set; }

        /// <summary>
        /// Get or set the total inflow per 1 [s].
        /// </summary>
        ISubstanceStateCollection Inflow { get; set; }

        /// <summary>
        /// Gets the available volumetric capacity for inflow into this consumer for the given timestep, in [m^3].
        /// This is used by the solver to prevent overfilling capacity-limited objects (like tanks) and to respect
        /// the demand rate of rate-limited consumers (like engines).
        /// </summary>
        /// <param name="dt">The duration of the timestep, in [s].</param>
        /// <returns>The available capacity in [m^3]. Can be PositiveInfinity for unlimited-demand consumers.</returns>
        double GetAvailableInflowVolume( double dt );

        /// <summary>
        /// Applies the calculated inflows and outflows to the internal contents of the object.
        /// </summary>
        void ApplyFlows( double deltaTime );

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container.
        /// </summary>
        /// <remarks>
        /// Takes the Inflow into account, if a tank.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        FluidState Sample( Vector3 localPosition, double holeArea );
    }
}