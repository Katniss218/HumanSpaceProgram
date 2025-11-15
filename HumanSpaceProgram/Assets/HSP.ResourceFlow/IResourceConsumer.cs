using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Anything that can consume resources. E.g. rocket engine, lightbulb (electricity), vent, etc.
    /// </summary>
    public interface IResourceConsumer
    {
        Vector3 Acceleration { get; set; } // in tank-space, acceleration of tank relative to fluid.
        Vector3 AngularVelocity { get; set; }

        /// <summary>
        /// Get or set the total inflow per 1 [s].
        /// </summary>
        SubstanceStateCollection Inflow { get; }

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container, as well as what species will want to `flow` out of the container.
        /// </summary>
        /// <remarks>
        /// If possible, the pressure should be extrapolated, if the position falls out of bounds.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="localAcceleration">The local acceleration vector, in [m/s^2].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea );
    }
}