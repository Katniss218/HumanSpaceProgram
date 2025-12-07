namespace HSP.ResourceFlow
{
    public static class IReadonlySubstanceStateCollection_Ex
    {
        /// <summary>
        /// Calculates the total volume of all substances in a collection at a given state.
        /// </summary>
        /// <param name="collection">The substance collection.</param>
        /// <param name="temperature">The temperature at which to evaluate density, in [K].</param>
        /// <param name="pressure">The pressure at which to evaluate density, in [Pa].</param>
        /// <returns>The total volume in [m^3].</returns>
        public static double GetVolume( this IReadonlySubstanceStateCollection collection, double temperature, double pressure )
        {
            double totalVolume = 0;
            foreach( (ISubstance s, double mass) in collection )
            {
                if( mass > 0 )
                {
                    double density = s.GetDensity( temperature, pressure );
                    if( density > 1e-9 )
                    {
                        totalVolume += mass / density;
                    }
                }
            }
            return totalVolume;
        }

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
        /// Gets the molar fraction (0.0 to 1.0) of a specific substance.
        /// Returns 0 if total amount of moles is effectively zero.
        /// </summary>
        public static double GetMolarFraction( this IReadonlySubstanceStateCollection collection, ISubstance substance )
        {
            double totalMoles = 0.0;
            double targetMoles = 0.0;

            foreach( (ISubstance s, double mass) in collection )
            {
                double moles = s.ToMoles( mass );
                totalMoles += moles;
                if( s == substance ) targetMoles = moles;
            }

            if( totalMoles <= 1e-9 )
                return 0.0;
            return targetMoles / totalMoles;
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
                double density = s.GetDensity( temperature, pressure );
                if( density > 1e-9 )
                {
                    totalMass += mass;
                    totalVolume += mass / density;
                }
            }

            if( totalVolume <= 1e-9 )
            {
                return 0.0;
            }

            return totalMass / totalVolume;
        }

        /// <summary>
        /// Calculates the mass-weighted average bulk modulus of all substances in the collection.
        /// </summary>
        /// <param name="collection">The substance collection.</param>
        /// <param name="temperature">The temperature at which to evaluate the bulk modulus, in [K].</param>
        /// <param name="pressure">The pressure at which to evaluate the bulk modulus, in [Pa].</param>
        /// <returns>The average bulk modulus in [Pa].</returns>
        public static double GetAverageBulkModulus( this IReadonlySubstanceStateCollection collection, double temperature, double pressure )
        {
            double totalMass = collection.GetMass();
            if( totalMass <= 1e-9 )
            {
                return 0.0;
            }

            double weightedSum = 0;
            foreach( (ISubstance s, double mass) in collection )
            {
                weightedSum += s.GetBulkModulus( temperature, pressure ) * mass;
            }
            return weightedSum / totalMass;
        }


        public static double GetAveragePressureDerivativeWrtMass( this ISubstanceStateCollection collection, double volume, double temp )
        {
            double totalMass = collection.GetMass();
            if( totalMass <= 1e-9 )
            {
                return 0.0;
            }

            double weightedSum = 0.0;
            foreach( (ISubstance s, double mass) in collection )
            {
                weightedSum += s.GetPressureDerivativeWrtMass( volume, temp ) * mass;
            }
            return weightedSum / totalMass;
        }
    }
}