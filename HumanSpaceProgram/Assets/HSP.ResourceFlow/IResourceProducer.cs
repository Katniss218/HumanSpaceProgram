using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Anything that can produce resources from external or internal sources. <br/>
    /// E.g. tank, drilling machine, air intake, etc.
    /// </summary>
    public interface IResourceProducer
    {
        Vector3 Acceleration { get; set; } // in tank-space, acceleration of tank relative to fluid.
        Vector3 AngularVelocity { get; set; }

        /// <summary>
        /// Get or set the total outflow per 1 [s].
        /// </summary>
        SubstanceStateCollection Outflow { get; }

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container, as well as what species will want to `flow` out of the container.
        /// </summary>
        /// <remarks>
        /// If possible, the pressure should be extrapolated, if the position falls out of bounds.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        FluidState Sample( Vector3 localPosition, Vector3 localAcceleration, float holeArea );

        /// <summary>
        /// Calculates the amount of resources that would flow out in 1 [s] is an orifice of given area was created at the specified position.
        /// </summary>
        IReadonlySubstanceStateCollection SampleSubstances( Vector3 localPosition, float flowRate, float dt );
    }
}