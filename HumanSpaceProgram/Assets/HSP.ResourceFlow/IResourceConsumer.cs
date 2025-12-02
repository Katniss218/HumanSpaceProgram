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
        /// The desired volumetric flow rate into this consumer, in [m^3/s].
        /// The solver will attempt to meet this demand.
        /// A value of PositiveInfinity means demand is unlimited.
        /// </summary>
        double Demand { get; set; }

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