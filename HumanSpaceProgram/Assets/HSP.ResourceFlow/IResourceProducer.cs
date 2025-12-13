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
        /// Called before the main solver step to run any internal simulation logic.
        /// </summary>
        void PreSolveUpdate( double deltaTime );

        /// <summary>
        /// Gets the total volume of fluid currently available for outflow from this producer.
        /// This is used by the solver to prevent pulling more fluid than exists in a single time step.
        /// </summary>
        /// <returns>The available volume in [m^3]. Can be PositiveInfinity for unlimited sources.</returns>
        double GetAvailableOutflowVolume();

        /// <summary>
        /// Applies the results of the network solve (inflows/outflows) to the internal state of the object.
        /// </summary>
        void ApplySolveResults( double deltaTime );

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
        /// Calculates the total mass of each substance that would flow out from a specific point over a given time interval,
        /// assuming a constant volumetric flow rate.
        /// </summary>
        /// <remarks>
        /// This method samples the producer's current contents at the specified `localPosition` to determine the substance composition and density.
        /// It then converts the requested volumetric flow into a mass transfer. The calculation is based on the current state and does not account
        /// for other inflows or outflows that might occur during the time interval.
        /// </remarks>
        /// <param name="localPosition">The point in the producer's local space from which to sample the substance(s).</param>
        /// <param name="mass">The total mass of fluid to be transferred in [kg].</param>
        /// <returns>A collection representing the total mass of each substance transferred.</returns>
        ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double mass );
    }
}