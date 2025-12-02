namespace HSP.ResourceFlow
{
    public static class IReadonlySubstanceStateCollection_Ex
    {
        /// <summary>
        /// Gets the mass fraction (0.0 to 1.0) of a specific substance.
        /// Returns 0 if total mass is effectively zero.
        /// </summary>
        public static double GetMassFraction( this IReadonlySubstanceStateCollection collection, ISubstance substance )
        {
            double totalMass = collection.GetMass();
            if( totalMass <= 1e-9 )
                return 0.0;

            if( collection.TryGet( substance, out double mass ) )
            {
                return mass / totalMass;
            }
            return 0.0;
        }

        /// <summary>
        /// Calculates the average density of the liquid components in a substance collection.
        /// </summary>
        /// <param name="collection">The substance collection.</param>
        /// <param name="temperature">The temperature at which to evaluate density, in [K].</param>
        /// <param name="pressure">The pressure at which to evaluate density, in [Pa].</param>
        /// <returns>The mass-weighted average density in [kg/m^3].</returns>
        public static double GetAverageDensity( this IReadonlySubstanceStateCollection collection, double temperature, double pressure )
        {
            double totalMass = 0;
            double totalVolume = 0;

            foreach( (ISubstance s, double mass) in collection )
            {
                if( s.Phase == SubstancePhase.Liquid || s.Phase == SubstancePhase.Solid )
                {
                    double density = s.GetDensity( temperature, pressure );
                    if( density > 1e-9 )
                    {
                        totalMass += mass;
                        totalVolume += mass / density;
                    }
                }
            }

            if( totalVolume <= 1e-9 )
            {
                return 0.0;
            }

            return totalMass / totalVolume;
        }
    }
}