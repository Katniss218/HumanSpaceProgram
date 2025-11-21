using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public static class ISubstance_Ex
    {
        public static double ToMoles( this ISubstance self, double mass )
        {
            if( self.MolarMass <= 0.0 )
                return 0.0;
            return mass * self.MolarMass;
        }

        public static double ToMass( this ISubstance self, double moles )
        {
            if( self.MolarMass <= 0.0 )
                return 0.0;
            return moles * self.MolarMass;
        }

        // mass / volume / pressure

        public static double GetVolume( this ISubstance self, double mass, double pressure, double temperature )
        {
            double density = self.GetDensity( temperature, pressure );
            if( density <= 0.0 )
                throw new ArgumentException();
            return mass / density;
        }

        public static double GetMass( this ISubstance self, double volume, double pressure, double temperature )
        {
            double density = self.GetDensity( temperature, pressure );
            return density * volume;
        }

        public static double GetPressure( this ISubstance self, double mass, double volume, double temperature )
        {
            double density = mass / volume;
            return self.GetPressure( temperature, density );
        }


        /// <summary>
        /// Calculates the dynamic pressure of a flowing liquid (per unit volume).
        /// </summary>
        /// <param name="density">The density of the fluid, in [kg/m^3].</param>
        /// <param name="velocity">The velocity of the fluid, in [m/s].</param>
        /// <returns>The dynamic pressure in [Pa].</returns>
        public static double GetDynamicPressure( double density, double velocity )
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
        public static double GetStaticPressure( double density, double height, double acceleration )
        {
            return acceleration * density * height;
        }

        public static SubstancePhase GetEquilibriumPhase( ISubstance substance, double temperature, double pressure )
        {
            double pSat = substance.GetVaporPressure( temperature );
            if( double.IsNaN( pSat ) )
            {
                // No meaningful vapor pressure known - assume condensed (liquid/solid).
                return SubstancePhase.Liquid;
            }

            if( pressure <= pSat )
                return SubstancePhase.Gas;
            return SubstancePhase.Liquid;
        }
    }
}