using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a physical substance/resource.
    /// </summary>
    public interface ISubstance
    {
        // Basic properties.

        string ID { get; }

        string DisplayName { get; }

        /// <summary>
        /// The color of the substance, used when drawing UIs.
        /// </summary>
        Color DisplayColor { get; }

        /// <summary>
        /// Categorization tags (e.g., "Fuel", "Oxidizer", "Corrosive").
        /// </summary>
        string[] Tags { get; }



        /// <summary>
        /// Gets the phase of this substance (gas / liquid / solid / etc).
        /// </summary>
        SubstancePhase Phase { get; }

        /// <summary>
        /// Gets the molar mass, in [kg/mol].
        /// </summary>
        double MolarMass { get; }

        /// <summary>
        /// Gets the specific gas constant, in [J/(kg*K)]. (Only applicable for gases).
        /// </summary>
        double SpecificGasConstant { get; }

        /// <summary>
        /// Gets the temperature at which the substance ignites, in [K]. <br/>
        /// Null if non-flammable.
        /// </summary>
        double? FlashPoint { get; }


        // Physical properties.

        /// <summary>
        /// Computes the bulk modulus (a measure of compressibility), in [Pa], at a given state.
        /// </summary>
        double GetBulkModulus( double temperature, double pressure );

        /// <summary>
        /// Computes the pressure, in [Pa].
        /// </summary>
        double GetPressure( double temperature, double density );

        /// <summary>
        /// Computes the analytical derivative of pressure with respect to mass for a fixed volume and temperature.
        /// </summary>
        /// <param name="volume">The fixed volume of the container, in [m^3].</param>
        /// <param name="temperature">The fixed temperature of the substance, in [K].</param>
        /// <returns>The rate of change of pressure with respect to mass (∂P/∂m), in [Pa/kg].</returns>
        double GetPressureDerivativeWrtMass( double volume, double temperature );

        /// <summary>
        /// Computes the density, in [kg/m^3].
        /// </summary>
        double GetDensity( double temperature, double pressure );

        /// <summary>
        /// Computes the dynamic viscosity, in [Pa*s].
        /// </summary>
        double GetViscosity( double temperature, double pressure );

        /// <summary>
        /// Computes the speed of sound in the substance, in [m/s].
        /// </summary>
        double GetSpeedOfSound( double temperature, double pressure );

        /// <summary>
        /// Computes the thermal conductivity, in [W/(m*K)].
        /// </summary>
        double GetThermalConductivity( double temperature, double pressure );

        /// <summary>
        /// Computes the specific heat capacity (Cp), in [J/(kg*K)].
        /// </summary>
        double GetSpecificHeatCapacity( double temperature, double pressure );

        /// <summary>
        /// Computes the latent heat of vaporization, in [J/kg].
        /// </summary>
        double GetLatentHeatOfVaporization();

        /// <summary>
        /// Computes the latent heat of fusion, in [J/kg].
        /// </summary>
        double GetLatentHeatOfFusion();


        // Thermochemistry.

        // double GetSpecificEnthalpy( double temperature, double pressure );
        // double GetGibbsFreeEnergy( double temperature, double pressure ); // in [J/kg]


        // phase changes.

        /// <summary>
        /// Computes the vapor pressure, in [Pa], for a given temperature.
        /// </summary>
        double GetVaporPressure( double temperature );

        /// <summary>
        /// Computes the boiling point, in [K], for a given pressure.
        /// </summary>
        double GetBoilingPoint( double pressure );
    }
}