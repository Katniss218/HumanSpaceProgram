namespace HSP.ResourceFlow
{
    public static class IReadonlySubstanceStateCollection_Ex
    {
        private const double MIN_DENSITY_FOR_VOLUME_CALC = 1e-9;
        private const double MIN_TOTAL_MASS_FOR_FRACTION = 1e-9;
        private const double MIN_TOTAL_MOLES_FOR_FRACTION = 1e-9;
        private const double MIN_DENSITY_FOR_AVERAGING = 1e-9;
        private const double MIN_TOTAL_VOLUME_FOR_AVG_DENSITY = 1e-9;
        private const double MIN_TOTAL_MASS_FOR_AVG = 1e-9;
        private const double MIN_TOTAL_MASS_FOR_PHASE_CHECK = 1e-9;
        private const double MIN_TOTAL_MASS_FOR_AVG_VISCOSITY = 1e-9;
        private const double MIN_TOTAL_MASS_FOR_AVG_SOS = 1e-9;

        private const double DEFAULT_VISCOSITY = 1e-5;
        private const double DEFAULT_SPEED_OF_SOUND = 343.0;
        private const double DEFAULT_ADIABATIC_INDEX = 1.4;
        private const double DEFAULT_SPECIFIC_GAS_CONSTANT = 287.0;

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
                    if( density > MIN_DENSITY_FOR_VOLUME_CALC )
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
            if( totalMass <= MIN_TOTAL_MASS_FOR_FRACTION )
            {
                return 0.0;
            }

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
                if( s == substance )
                {
                    targetMoles = moles;
                }
            }

            if( totalMoles <= MIN_TOTAL_MOLES_FOR_FRACTION )
            {
                return 0.0;
            }
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
                if( density > MIN_DENSITY_FOR_AVERAGING )
                {
                    totalMass += mass;
                    totalVolume += mass / density;
                }
            }

            if( totalVolume <= MIN_TOTAL_VOLUME_FOR_AVG_DENSITY )
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
            if( totalMass <= MIN_TOTAL_MASS_FOR_AVG )
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


        public static double GetAveragePressureDerivativeWrtMass( this IReadonlySubstanceStateCollection collection, double volume, double temp )
        {
            double totalMass = collection.GetMass();
            if( totalMass <= MIN_TOTAL_MASS_FOR_AVG )
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

        public static bool IsSinglePhase( this IReadonlySubstanceStateCollection collection )
        {
            if( collection.GetMass() <= MIN_TOTAL_MASS_FOR_PHASE_CHECK )
            {
                return false;
            }

            bool first = true;
            SubstancePhase substancePhase = default;
            foreach( var (sub, _) in collection )
            {
                if( first )
                {
                    substancePhase = sub.Phase;
                    first = false;
                }
                else if( sub.Phase != substancePhase )
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsSinglePhase( this IReadonlySubstanceStateCollection collection, SubstancePhase phase )
        {
            if( collection.GetMass() <= MIN_TOTAL_MASS_FOR_PHASE_CHECK )
            {
                return false;
            }

            foreach( var (sub, _) in collection )
            {
                if( sub.Phase != phase )
                {
                    return false;
                }
            }
            return true;
        }

        public static double GetAverageViscosity( this IReadonlySubstanceStateCollection collection, double temp, double press )
        {
            double totalMass = collection.GetMass();
            if( totalMass < MIN_TOTAL_MASS_FOR_AVG_VISCOSITY )
            {
                return DEFAULT_VISCOSITY;
            }

            double weightedSum = 0;
            foreach( var (sub, mass) in collection )
            {
                weightedSum += sub.GetViscosity( temp, press ) * mass;
            }
            return weightedSum / totalMass;
        }

        public static double GetAverageSpeedOfSound( this IReadonlySubstanceStateCollection collection, double temp, double press )
        {
            double totalMass = collection.GetMass();
            if( totalMass <= MIN_TOTAL_MASS_FOR_AVG_SOS )
            {
                return DEFAULT_SPEED_OF_SOUND;
            }

            double weightedSum = 0;
            foreach( var (sub, mass) in collection )
            {
                weightedSum += sub.GetSpeedOfSound( temp, press ) * mass;
            }
            return weightedSum / totalMass;
        }

        public static double GetAverageAdiabaticIndex( this IReadonlySubstanceStateCollection collection )
        {
            double totalMass = collection.GetMass();
            if( totalMass < MIN_TOTAL_MASS_FOR_AVG )
            {
                return DEFAULT_ADIABATIC_INDEX;
            }

            double weightedSum = 0;
            foreach( var (sub, mass) in collection )
            {
                weightedSum += sub.AdiabaticIndex * mass;
            }
            return weightedSum / totalMass;
        }

        public static double GetAverageSpecificGasConstant( this IReadonlySubstanceStateCollection collection )
        {
            double totalMass = collection.GetMass();
            if( totalMass < MIN_TOTAL_MASS_FOR_AVG )
            {
                return DEFAULT_SPECIFIC_GAS_CONSTANT;
            }

            double weightedSum = 0;
            foreach( var (sub, mass) in collection )
            {
                weightedSum += sub.SpecificGasConstant * mass;
            }
            return weightedSum / totalMass;
        }
    }
}