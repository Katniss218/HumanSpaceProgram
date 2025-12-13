using System;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Provides a comprehensive set of static utility methods for solving common fluid dynamics
    /// equations related to both the linearized potential-based model used by the main solver,
    /// and the non-linear pressure-based model used for components like engine injectors.
    /// </summary>
    public static class FlowEquations
    {
        private const double ZERO_FLOW_TOLERANCE = 1e-9;
        private const double MIN_REYNOLDS_FOR_FRICTION = 1.0;
        private const double HIGH_CONDUCTANCE_FALLBACK = 1e9;

        /// <summary>
        /// Calculates the Reynolds number for a given mass flow rate in a pipe.
        /// </summary>
        /// <param name="massFlowRate">Absolute mass flow rate in [kg/s].</param>
        /// <param name="diameter">Hydraulic diameter of the pipe in [m].</param>
        /// <param name="viscosity">Dynamic viscosity of the fluid in [Pa*s].</param>
        /// <returns>The dimensionless Reynolds number.</returns>
        public static double GetReynoldsNumber( double massFlowRate, double diameter, double viscosity )
        {
            if( diameter < ZERO_FLOW_TOLERANCE || viscosity < ZERO_FLOW_TOLERANCE )
            {
                return 0.0;
            }
            return (4.0 * Math.Abs( massFlowRate )) / (Math.PI * diameter * viscosity);
        }

        /// <summary>
        /// Calculates the Darcy friction factor using the Blasius correlation for smooth pipes.
        /// Valid for Reynolds numbers between 4000 and 100,000.
        /// </summary>
        /// <param name="reynoldsNumber">The Reynolds number.</param>
        /// <returns>The Darcy friction factor.</returns>
        public static double GetDarcyFrictionFactor( double reynoldsNumber )
        {
            if( reynoldsNumber < MIN_REYNOLDS_FOR_FRICTION )
            {
                return 0.3164; // Fallback for zero flow
            }
            // Blasius correlation: f = 0.3164 * Re^-0.25
            return 0.3164 * Math.Pow( reynoldsNumber, -0.25 );
        }

        /// <summary>
        /// Calculates the mass flow conductance for laminar flow (Re < 2300) based on the Hagen-Poiseuille equation.
        /// </summary>
        /// <param name="density">Fluid density in [kg/m^3].</param>
        /// <param name="area">Pipe cross-sectional area in [m^2].</param>
        /// <param name="length">Pipe length in [m].</param>
        /// <param name="viscosity">Fluid dynamic viscosity in [Pa*s].</param>
        /// <returns>Mass flow conductance in [kg*s/m^2].</returns>
        public static double GetLaminarMassConductance( double density, double area, double length, double viscosity )
        {
            double denominator = 8.0 * Math.PI * viscosity * length;
            if( denominator < ZERO_FLOW_TOLERANCE )
            {
                return double.PositiveInfinity;
            }
            return (density * density * area * area) / denominator;
        }

        /// <summary>
        /// Calculates the effective linearized mass flow conductance for turbulent flow (Re > 4000).
        /// </summary>
        /// <param name="density">Fluid density in [kg/m^3].</param>
        /// <param name="area">Pipe cross-sectional area in [m^2].</param>
        /// <param name="diameter">Pipe hydraulic diameter in [m].</param>
        /// <param name="length">Pipe length in [m].</param>
        /// <param name="frictionFactor">The Darcy friction factor for the flow.</param>
        /// <param name="lastMassFlowRate">The absolute mass flow rate from the previous step in [kg/s].</param>
        /// <returns>Effective mass flow conductance in [kg*s/m^2].</returns>
        public static double GetTurbulentMassConductance( double density, double area, double diameter, double length, double frictionFactor, double lastMassFlowRate )
        {
            double denominator = frictionFactor * length * Math.Max( Math.Abs( lastMassFlowRate ), ZERO_FLOW_TOLERANCE );
            if( denominator < ZERO_FLOW_TOLERANCE )
            {
                // For near-zero flow, a very large conductance can be returned to allow flow to start.
                // The solver's relaxation will handle the initial large step.
                return HIGH_CONDUCTANCE_FALLBACK;
            }
            return (2.0 * density * density * area * area * diameter) / denominator;
        }

        /// <summary>
        /// Calculates the maximum possible mass flow rate through a pipe at sonic velocity (Mach 1).
        /// </summary>
        /// <param name="density">The density of the gas at the pipe inlet in [kg/m^3].</param>
        /// <param name="area">The cross-sectional area of the pipe in [m^2].</param>
        /// <param name="speedOfSound">The speed of sound in the gas in [m/s].</param>
        /// <returns>The maximum (choked) mass flow rate in [kg/s].</returns>
        public static double GetChokedMassFlow( double density, double area, double speedOfSound )
        {
            return density * area * speedOfSound;
        }
    }
}