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
        [field: SerializeField]
        public double Pressure { get; set; }

        /// <summary>
        /// Gets or sets the temperature, in [K].
        /// </summary>
        [field: SerializeField]
        public double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the magnitude of the velocity, in [m/s].
        /// </summary>
        [field: SerializeField]
        public double Velocity { get; set; }

        /// <summary>
        /// Gets or sets the potential energy of the fluid surface, in [J/kg].
        /// </summary>
        public double FluidSurfacePotential { get; set; }

        /// <summary>
        /// Returns the fluid state for a perfect vacuum.
        /// </summary>
        public static FluidState Vacuum => new FluidState()
        {
            Pressure = 0.0f,
            Temperature = 0.0f,
            Velocity = 0.0f,
            FluidSurfacePotential = double.NegativeInfinity
        };

        public static FluidState STP => new FluidState()
        {
            Pressure = 101325.0,
            Temperature = 273.15,
            Velocity = 0.0,
            FluidSurfacePotential = 0.0
        };

        public FluidState( double pressure, double temperature, double velocity )
        {
            this.Pressure = pressure;
            this.Temperature = temperature;
            this.Velocity = velocity;
            this.FluidSurfacePotential = 0.0;
        }

        public override string ToString()
        {
            return $"P={Pressure} Pa, T={Temperature} K, V={Velocity} m/s";
        }


        [MapsInheritingFrom( typeof( FluidState ) )]
        public static SerializationMapping FluidStateMapping()
        {
            return new MemberwiseSerializationMapping<FluidState>()
                .WithMember( "pressure", o => o.Pressure )
                .WithMember( "temperature", o => o.Temperature )
                .WithMember( "velocity", o => o.Velocity )
                .WithMember( "fluid_surface_potential", o => o.FluidSurfacePotential );
        }
    }
}