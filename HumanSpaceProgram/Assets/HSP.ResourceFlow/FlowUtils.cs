
namespace HSP.ResourceFlow
{
    public static class FlowUtils
    {
        /// <summary>
        /// Calculates the dynamic pressure of a flowing liquid (per unit volume).
        /// </summary>
        /// <param name="density">The density of the fluid, in [kg/m^3].</param>
        /// <param name="velocity">The velocity of the fluid, in [m/s].</param>
        /// <returns>The dynamic pressure in [Pa].</returns>
        public static float GetDynamicPressure( float density, float velocity )
        {
            return 0.5f * density * (velocity * velocity);
        }

        /// <summary>
        /// Calculates the static (hydrostatic) pressure at a given depth of liquid.
        /// </summary>
        /// <param name="density">The density of the fluid, in [kg/m^3].</param>
        /// <param name="height">The height of the fluid column (measured along the acceleration vector), in [m].</param>
        /// <param name="acceleration">The magnitude of the acceleration vector, in [m/s^2].</param>
        /// <returns>The static pressure in [Pa]</returns>
        public static float GetStaticPressure( float density, float height, float acceleration )
        {
            return acceleration * density * height;
        }

        /// <summary>
        /// Clamps the flow based on the volume and maximum volume in a container, and the timestep.
        /// </summary>
        /// <remarks>
        /// Volume-limited system.
        /// </remarks>
        public static void ClampMaxVolume( SubstanceStateCollection inflow, float volume, float maxVolume, float dt )
        {
            float inflowVol = inflow.GetVolume();
            float remainingVolume = maxVolume - volume;

            if( (inflowVol * dt) > remainingVolume )
            {
                inflow.SetVolume( remainingVolume / dt );
            }
        }

        /// <remarks>
        /// Volumetric flow-limited system.
        /// </remarks>
        public static void ClampMaxVolumeFlow( SubstanceStateCollection inflow, float volume, float maxVolume )
        {
            float inflowVol = inflow.GetVolume();
            float remainingVolume = maxVolume - volume;

            if( inflowVol > remainingVolume )
            {
                inflow.SetVolume( remainingVolume );
            }
        }

        /// <summary>
        /// Clamps the flow based on the mass and maximum mass in a container, and the timestep.
        /// </summary>
        /// <remarks>
        /// Mass-limited system.
        /// </remarks>
        public static void ClampMaxMass( SubstanceStateCollection inflow, float mass, float maxMass, float dt )
        {
            float inflowMass = inflow.GetMass();
            float remainingMass = maxMass - mass;

            if( (inflowMass * dt) > remainingMass )
            {
                inflow.SetMass( remainingMass / dt );
            }
        }

        /// <remarks>
        /// Mass flow-limited system.
        /// </remarks>
        public static void ClampMaxMassFlow( SubstanceStateCollection inflow, float massFlow, float maxMassFlow )
        {
            float inflowMass = inflow.GetMass();
            float remainingMass = maxMassFlow - massFlow;

            if( inflowMass > remainingMass )
            {
                inflow.SetMass( remainingMass );
            }
        }
    }
}