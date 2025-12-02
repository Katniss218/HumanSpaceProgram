using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Anything that can produce resources from external or internal sources. <br/>
    /// E.g. tank, drilling machine, air intake, etc.
    /// </summary>
    public interface IResourceProducer
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
        /// Get or set the total outflow per 1 [s].
        /// </summary>
        ISubstanceStateCollection Outflow { get; set; }

        /// <summary>
        /// Applies the calculated inflows and outflows to the internal contents of the object.
        /// </summary>
        void ApplyFlows( double deltaTime );

        /// <summary>
        /// Calculates the pressure acting at any given point inside the container.
        /// </summary>
        /// <remarks>
        /// Takes the Outflow into account, if a tank.
        /// </remarks>
        /// <param name="localPosition">The local position of the point to sample, in [m].</param>
        /// <param name="holeArea">The area of the hole, in [m^2].</param>
        FluidState Sample( Vector3 localPosition, double holeArea );

        /// <summary>
        /// Calculates the amount of resources that would flow out if a portal of a given flowrate was created at the specified position, and held open for the specified amount of time.
        /// </summary>
        /// <remarks>
        /// It should take into account the amount of resources available (not including Inflow/Outflow) in the tank.
        /// </remarks>
        /// <param name="flowRate">Volumetric flow rate, in [m^3/s].</param>
        ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double flowRate, double dt );
    }
}