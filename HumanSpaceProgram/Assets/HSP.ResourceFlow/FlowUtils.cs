using System;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Provides a set of static utility methods for common calculations related to resource flow,
    /// engine performance, and pipe geometry.
    /// </summary>
    public static class FlowUtils
    {
        /// <summary>
        /// Standard gravity constant (g₀), used for Isp to exhaust velocity conversions. [m/s²]
        /// </summary>
        public const double StandardGravity = 9.80665;

        /// <summary>
        /// Converts specific impulse (Isp) into effective exhaust velocity.
        /// </summary>
        /// <param name="specificImpulse">The specific impulse in [s].</param>
        /// <returns>The effective exhaust velocity in [m/s].</returns>
        public static double GetExhaustVelocityFromIsp( double specificImpulse )
        {
            return specificImpulse * StandardGravity;
        }

        /// <summary>
        /// Calculates the required mass flow rate to produce a given thrust at a specific Isp.
        /// </summary>
        /// <param name="thrust">The desired thrust in [N].</param>
        /// <param name="specificImpulse">The engine's specific impulse in [s].</param>
        /// <returns>The required mass flow rate in [kg/s].</returns>
        public static double GetMassFlowFromThrust( double thrust, double specificImpulse )
        {
            if( specificImpulse < 1e-9 )
                return double.PositiveInfinity; // Avoid division by zero, signal impossibility

            double exhaustVelocity = GetExhaustVelocityFromIsp( specificImpulse );
            if( exhaustVelocity < 1e-9 )
                return double.PositiveInfinity;

            return thrust / exhaustVelocity;
        }

        /// <summary>
        /// Calculates the thrust produced by a given mass flow rate at a specific Isp.
        /// </summary>
        /// <param name="massFlow">The mass flow rate in [kg/s].</param>
        /// <param name="specificImpulse">The engine's specific impulse in [s].</param>
        /// <returns>The produced thrust in [N].</returns>
        public static double GetThrustFromMassFlow( double massFlow, double specificImpulse )
        {
            double exhaustVelocity = GetExhaustVelocityFromIsp( specificImpulse );
            return massFlow * exhaustVelocity;
        }

        /// <summary>
        /// Converts a volumetric flow rate to a mass flow rate.
        /// </summary>
        /// <param name="volumeFlow">Volumetric flow rate in [m³/s].</param>
        /// <param name="density">Fluid density in [kg/m³].</param>
        /// <returns>Mass flow rate in [kg/s].</returns>
        public static double GetMassFlowFromVolumeFlow( double volumeFlow, double density )
        {
            return volumeFlow * density;
        }

        /// <summary>
        /// Converts a mass flow rate to a volumetric flow rate.
        /// </summary>
        /// <param name="massFlow">Mass flow rate in [kg/s].</param>
        /// <param name="density">Fluid density in [kg/m³].</param>
        /// <returns>Volumetric flow rate in [m³/s].</returns>
        public static double GetVolumeFlowFromMassFlow( double massFlow, double density )
        {
            if( density < 1e-9 )
                return double.PositiveInfinity;
            return massFlow / density;
        }

        /// <summary>
        /// Calculates the cross-sectional area of a pipe from its diameter.
        /// </summary>
        /// <param name="diameter">The pipe's inner diameter in [m].</param>
        /// <returns>The cross-sectional area in [m²].</returns>
        public static double GetAreaFromDiameter( double diameter )
        {
            double radius = diameter / 2.0;
            return Math.PI * radius * radius;
        }

        /// <summary>
        /// Calculates the inner diameter of a pipe from its cross-sectional area.
        /// </summary>
        /// <param name="area">The pipe's cross-sectional area in [m²].</param>
        /// <returns>The inner diameter in [m].</returns>
        public static double GetDiameterFromArea( double area )
        {
            if( area < 0 ) 
                return 0.0;
            return Math.Sqrt( 4 * area / Math.PI );
        }

        /// <summary>
        /// Calculates the actual mass that can be transferred in a single step, given a proposed amount
        /// and the constraints of the source and sink.
        /// </summary>
        /// <param name="proposedMass">The desired mass to transfer in [kg].</param>
        /// <param name="sourceAvailableMass">The total mass available from the source in [kg].</param>
        /// <param name="sinkAvailableMass">The remaining mass capacity of the sink in [kg].</param>
        /// <returns>The clamped, actual mass that can be safely transferred in [kg].</returns>
        public static double CalculateLimitedMassTransfer( double proposedMass, double sourceAvailableMass, double sinkAvailableMass )
        {
            // Clamp by what the source can provide.
            double massFromSource = Math.Min( proposedMass, sourceAvailableMass );
            // Further clamp by what the sink can accept.
            double finalMass = Math.Min( massFromSource, sinkAvailableMass );

            return Math.Max( 0, finalMass ); // Ensure result is not negative.
        }

        /// <summary>
        /// Calculates the average velocity of a fluid inside a pipe for a given flow rate.
        /// </summary>
        /// <param name="massFlow">The mass flow rate in [kg/s].</param>
        /// <param name="density">The density of the fluid in [kg/m³].</param>
        /// <param name="pipeDiameter">The inner diameter of the pipe in [m].</param>
        /// <returns>The fluid velocity in [m/s].</returns>
        public static double GetFluidVelocity( double massFlow, double density, double pipeDiameter )
        {
            if( density < 1e-9 )
                return double.PositiveInfinity;
            double area = GetAreaFromDiameter( pipeDiameter );
            if( area < 1e-9 ) 
                return double.PositiveInfinity;
            return massFlow / (density * area);
        }

        /// <summary>
        /// Converts a pressure value into an equivalent height of a static fluid column ("head").
        /// </summary>
        /// <param name="pressure">The pressure in [Pa].</param>
        /// <param name="density">The density of the fluid in [kg/m³].</param>
        /// <param name="gravity">The acceleration due to gravity in [m/s²]. Use FlowUtils.StandardGravity for Earth standard.</param>
        /// <returns>The equivalent fluid head in [m].</returns>
        public static double GetHeadFromPressure( double pressure, double density, double gravity )
        {
            if( density < 1e-9 || gravity < 1e-9 ) 
                return 0.0;
            return pressure / (density * gravity);
        }

        /// <summary>
        /// Converts a static fluid column height ("head") into an equivalent pressure.
        /// </summary>
        /// <param name="head">The fluid head in [m].</param>
        /// <param name="density">The density of the fluid in [kg/m³].</param>
        /// <param name="gravity">The acceleration due to gravity in [m/s²]. Use FlowUtils.StandardGravity for Earth standard.</param>
        /// <returns>The equivalent pressure in [Pa].</returns>
        public static double GetPressureFromHead( double head, double density, double gravity )
        {
            return head * density * gravity;
        }

        /// <summary>
        /// Calculates the pressure drop across a pipe for a given mass flow rate.
        /// This method automatically determines the flow regime (laminar or turbulent).
        /// </summary>
        /// <param name="massFlow">The mass flow rate in [kg/s].</param>
        /// <param name="density">The fluid density in [kg/m³].</param>
        /// <param name="viscosity">The fluid dynamic viscosity in [Pa·s].</param>
        /// <param name="pipeLength">The length of the pipe in [m].</param>
        /// <param name="pipeDiameter">The inner diameter of the pipe in [m].</param>
        /// <returns>The pressure drop in [Pa].</returns>
        public static double GetPressureDrop( double massFlow, double density, double viscosity, double pipeLength, double pipeDiameter )
        {
            if( massFlow < 1e-9 || pipeDiameter < 1e-9 ) 
                return 0.0;

            double reynolds = GetReynoldsNumber( massFlow, pipeDiameter, viscosity );

            if( reynolds < 2300 ) // Laminar Flow
            {
                // From Hagen-Poiseuille: ΔP = (128 * μ * L * Q_vol) / (π * D^4)
                double volumeFlow = GetVolumeFlowFromMassFlow( massFlow, density );
                return (128.0 * viscosity * pipeLength * volumeFlow) / (Math.PI * Math.Pow( pipeDiameter, 4 ));
            }
            else // Turbulent (or Transitional, approximated as turbulent)
            {
                // From Darcy-Weisbach: ΔP = f * (L/D) * (ρ * v^2 / 2)
                double frictionFactor = GetDarcyFrictionFactor( reynolds );
                double velocity = GetFluidVelocity( massFlow, density, pipeDiameter );
                return frictionFactor * (pipeLength / pipeDiameter) * (density * velocity * velocity / 2.0);
            }
        }

        /// <summary>
        /// Estimates the required pipe diameter to achieve a target mass flow rate with a maximum allowable pressure drop.
        /// NOTE: This method uses an approximation for the turbulent friction factor and is best used for initial design estimates.
        /// </summary>
        /// <param name="targetMassFlow">The desired mass flow rate in [kg/s].</param>
        /// <param name="density">The fluid density in [kg/m³].</param>
        /// <param name="viscosity">The fluid dynamic viscosity in [Pa·s].</param>
        /// <param name="pipeLength">The length of the pipe in [m].</param>
        /// <param name="maxPressureDrop">The maximum acceptable pressure drop in [Pa].</param>
        /// <param name="assumedFrictionFactor">An assumed Darcy friction factor for turbulent flow. A value between 0.015 and 0.03 is typical for smooth pipes.</param>
        /// <returns>The estimated required pipe diameter in [m].</returns>
        public static double GetRequiredPipeDiameter( double targetMassFlow, double density, double viscosity, double pipeLength, double maxPressureDrop, double assumedFrictionFactor = 0.02 )
        {
            if( targetMassFlow < 1e-9 || maxPressureDrop < 1e-9 ) return 0.0;

            // First, check what the Reynolds number would be for a starting guess to decide on the flow regime.
            double initialDiameterGuess = 0.1; // 10cm is a reasonable starting point
            double reynolds = GetReynoldsNumber( targetMassFlow, initialDiameterGuess, viscosity );

            if( reynolds < 2300 ) // Assume Laminar
            {
                // ΔP = (128 * μ * L * Q) / (π * D^4) => D^4 = (128 * μ * L * Q) / (π * ΔP)
                double volumeFlow = GetVolumeFlowFromMassFlow( targetMassFlow, density );
                double D4 = (128.0 * viscosity * pipeLength * volumeFlow) / (Math.PI * maxPressureDrop);
                return Math.Pow( D4, 0.25 );
            }
            else // Assume Turbulent
            {
                // ΔP = (8 * f * L * ṁ^2) / (ρ * π^2 * D^5) => D^5 = (8 * f * L * ṁ^2) / (ρ * π^2 * ΔP)
                double numerator = 8.0 * assumedFrictionFactor * pipeLength * targetMassFlow * targetMassFlow;
                double denominator = density * Math.PI * Math.PI * maxPressureDrop;
                if( denominator < 1e-9 ) return double.PositiveInfinity;
                return Math.Pow( numerator / denominator, 0.2 );
            }
        }

        /// <summary>
        /// A simple description of a pipe's geometry for flow balancing calculations.
        /// </summary>
        public struct PipeDefinition
        {
            public double Length;
            public double Diameter;
        }

        /// <summary>
        /// Calculates the approximate distribution of total flow among a set of parallel pipes, assuming turbulent flow.
        /// This is useful for balancing multiple feed lines from a single source to a single sink.
        /// </summary>
        /// <param name="pipes">An array of pipe definitions representing the parallel feed lines.</param>
        /// <returns>An array of doubles, where each element is the fraction (0.0 to 1.0) of the total flow that will pass through the corresponding pipe.</returns>
        public static double[] CalculateParallelFlowDistribution( PipeDefinition[] pipes )
        {
            if( pipes == null || pipes.Length == 0 )
            {
                return Array.Empty<double>();
            }

            double[] relativeConductances = new double[pipes.Length];
            double totalRelativeConductance = 0;

            for( int i = 0; i < pipes.Length; i++ )
            {
                // Assuming turbulent flow, the mass flow rate ṁ is approximately proportional to D^(19/7) / L.
                // However, a simpler and common engineering approximation is that pressure drop is proportional to 1/D^5.
                // This means conductance is proportional to D^5 / L. We'll use this for simplicity.
                if( pipes[i].Length < 1e-9 || pipes[i].Diameter < 1e-9 )
                {
                    relativeConductances[i] = 0;
                }
                else
                {
                    double conductance = Math.Pow( pipes[i].Diameter, 5 ) / pipes[i].Length;
                    relativeConductances[i] = conductance;
                    totalRelativeConductance += conductance;
                }
            }

            double[] distribution = new double[pipes.Length];
            if( totalRelativeConductance > 1e-9 )
            {
                for( int i = 0; i < pipes.Length; i++ )
                {
                    distribution[i] = relativeConductances[i] / totalRelativeConductance;
                }
            }

            return distribution;
        }

        /// <summary>
        /// Calculates the Oxidizer-to-Fuel mass ratio (O/F) from a propellant mixture definition.
        /// </summary>
        /// <param name="propellantMixture">The collection defining the propellant masses or mass parts.</param>
        /// <param name="oxidizer">The oxidizer substance.</param>
        /// <param name="fuel">The fuel substance.</param>
        /// <returns>The O/F mass ratio, or 0 if components are missing.</returns>
        public static double GetMixtureRatio( IReadonlySubstanceStateCollection propellantMixture, ISubstance oxidizer, ISubstance fuel )
        {
            if( propellantMixture == null || oxidizer == null || fuel == null )
                return 0.0;

            if( propellantMixture.TryGet( oxidizer, out double oxidizerMass ) &&
                propellantMixture.TryGet( fuel, out double fuelMass ) )
            {
                if( fuelMass > 1e-9 )
                    return oxidizerMass / fuelMass;
            }

            return 0.0;
        }

        /// <summary>
        /// Calculates the mass rate of propellant boil-off due to heat input.
        /// </summary>
        /// <param name="heatInput">The rate of heat entering the tank in [W] (Joules/second).</param>
        /// <param name="latentHeatOfVaporization">The latent heat of vaporization of the substance in [J/kg].</param>
        /// <returns>The mass of propellant that boils off per second in [kg/s].</returns>
        public static double CalculateBoiloffMassRate( double heatInput, double latentHeatOfVaporization )
        {
            if( latentHeatOfVaporization < 1e-9 )
                return double.PositiveInfinity;

            return heatInput / latentHeatOfVaporization;
        }

        /// <summary>
        /// Estimates the solar heat absorbed by a tank surface.
        /// </summary>
        /// <param name="solarFlux">The solar flux at the object's location in [W/m²].</param>
        /// <param name="exposedArea">The cross-sectional area of the tank exposed to the sun in [m²].</param>
        /// <param name="absorptivity">The surface absorptivity of the tank material (0.0 to 1.0).</param>
        /// <returns>The absorbed heat rate in [W].</returns>
        public static double CalculateSolarHeatInput( double solarFlux, double exposedArea, double absorptivity = 0.8 )
        {
            return solarFlux * exposedArea * Math.Clamp( absorptivity, 0.0, 1.0 );
        }

        /// <summary>
        /// Calculates the pressure at which a thin-walled cylindrical or spherical tank will burst.
        /// Uses Barlow's formula for hoop stress.
        /// </summary>
        /// <param name="materialTensileStrength">The ultimate tensile strength of the tank material in [Pa].</param>
        /// <param name="radius">The inner radius of the tank in [m].</param>
        /// <param name="wallThickness">The thickness of the tank wall in [m].</param>
        /// <returns>The burst pressure in [Pa].</returns>
        public static double CalculateBurstPressure( double materialTensileStrength, double radius, double wallThickness )
        {
            if( radius < 1e-9 )
                return 0.0;

            // Barlow's formula: P = (2 * S * t) / D  or  P = (S * t) / r
            return (materialTensileStrength * wallThickness) / radius;
        }

        /// <summary>
        /// Calculates the Net Positive Suction Head Available (NPSHa) at a pump's inlet.
        /// This is a measure of the pressure margin above the fluid's vapor pressure.
        /// A pump will cavitate if NPSHa is less than its required NPSH.
        /// </summary>
        /// <param name="inletPressure">The absolute pressure at the pump inlet in [Pa].</param>
        /// <param name="vaporPressure">The vapor pressure of the fluid at its current temperature in [Pa].</param>
        /// <param name="density">The density of the fluid in [kg/m^3].</param>
        /// <returns>The NPSH available in meters of head [m].</returns>
        public static double CalculateNPSH_Available( double inletPressure, double vaporPressure, double density )
        {
            if( density < 1e-9 )
                return 0.0;

            double pressureHead = (inletPressure - vaporPressure) / (density * StandardGravity);
            return Math.Max( 0, pressureHead );
        }

        /// <summary>
        /// Calculates the power required to run a pump.
        /// </summary>
        /// <param name="massFlow">The mass flow rate through the pump in [kg/s].</param>
        /// <param name="headAdded">The specific energy (potential head) added by the pump in [J/kg].</param>
        /// <param name="efficiency">The pump's efficiency (0.0 to 1.0). Default is 0.75 (75%).</param>
        /// <returns>The required electrical power in [W].</returns>
        public static double CalculatePumpPower( double massFlow, double headAdded, double efficiency = 0.75 )
        {
            if( efficiency < 1e-9 )
                return double.PositiveInfinity;

            // Power [W] = mass flow [kg/s] * specific energy [J/kg] / efficiency
            double fluidPower = massFlow * headAdded;
            return Math.Max( 0, fluidPower / efficiency );
        }

        // Internal helper to calculate Reynolds number, duplicating logic from FlowEquations for utility use.
        private static double GetReynoldsNumber( double massFlowRate, double diameter, double viscosity )
        {
            if( diameter < 1e-9 || viscosity < 1e-9 )
                return 0.0;
            return (4.0 * Math.Abs( massFlowRate )) / (Math.PI * diameter * viscosity);
        }

        // Internal helper to calculate Darcy friction factor using Blasius correlation.
        private static double GetDarcyFrictionFactor( double reynoldsNumber )
        {
            if( reynoldsNumber < 4000 )
                return 0.03164; // Fallback for low/transitional Re
            return 0.3164 * Math.Pow( reynoldsNumber, -0.25 );
        }
    }
}