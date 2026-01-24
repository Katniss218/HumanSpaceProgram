namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents the capability of a resource flow component to provide its physical "stiffness".
    /// </summary>
    public interface IStiffnessProvider
    {
        /// <summary>
        /// Gets the derivative of the component's potential with respect to a change in its contained volume. <br/>
        /// This serves as a measure of the component's "stiffness". A high value indicates a rigid component <br/>
        /// where a small change in volume causes a large change in pressure/potential. <br/>
        /// Units: [(J/kg) / m^3] which simplifies to [m^2 / (s^2 * m^3)] = [1 / (m * s^2)]
        /// </summary>
        /// <remarks>
        /// For a liquid, this is approximately equivalent to dP/dM.
        /// </remarks>
        double GetPotentialDerivativeWrtVolume();
    }
}