using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Bulk state information about an unspecified fluid or a mixture of fluids.
    /// </summary>
    [Serializable]
    public struct FluidState
    {
        /// <summary>
        /// Gets or sets the pressure, in [Pa].
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// Gets or sets the temperature, in [K].
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the magnitude of the velocity, in [m/s].
        /// </summary>
        public double Velocity { get; set; }

        /// <summary>
        /// Gets or sets the total potential energy of the fluid surface, in [J/kg].
        /// This combines geometric potential and pressure potential based on the fluid's own density.
        /// </summary>
        public double FluidSurfacePotential { get; set; }

        /// <summary>
        /// Gets or sets the purely geometric potential energy at this point, in [J/kg].
        /// Derived from gravity, centrifugal force, etc. Independent of pressure.
        /// </summary>
        public double GeometricPotential { get; set; }

        /// <summary>
        /// Gets the fluid state for a perfect vacuum.
        /// </summary>
        public static readonly FluidState Vacuum = new FluidState()
        {
            Pressure = 0.0f,
            Temperature = 0.0f,
            Velocity = 0.0f,
            FluidSurfacePotential = -1e12,
            GeometricPotential = -1e12
        };

        /// <summary>
        /// Gets the fluid state for standard temperature and pressure (STP).
        /// </summary>
        public static readonly FluidState STP = new FluidState()
        {
            Pressure = 101325.0,
            Temperature = 273.15,
            Velocity = 0.0,
            FluidSurfacePotential = 0.0,
            GeometricPotential = 0.0
        };

        public FluidState( double pressure, double temperature, double velocity )
        {
            this.Pressure = pressure;
            this.Temperature = temperature;
            this.Velocity = velocity;
            this.FluidSurfacePotential = 0.0;
            this.GeometricPotential = 0.0;
        }

        public override string ToString()
        {
            return $"P={Pressure} Pa, T={Temperature} K, V={Velocity} m/s";
        }


        [MapsInheritingFrom( typeof( FluidState ) )]
        public static IDescriptor FluidStateMapping()
        {
            return new MemberwiseDescriptor<FluidState>()
                .WithMember( "pressure", o => o.Pressure )
                .WithMember( "temperature", o => o.Temperature )
                .WithMember( "velocity", o => o.Velocity )
                .WithMember( "fluid_surface_potential", o => o.FluidSurfacePotential )
                .WithMember( "geometric_potential", o => o.GeometricPotential );
        }
    }
}