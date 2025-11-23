using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Bulk state information about an unspecified fluid or a mixture of fluids.
    /// </summary>
    [Serializable]
    public struct FluidState
    {
        /// <summary>
        /// Gets or sets the pressure.
        /// </summary>
        [field: SerializeField]
        public double Pressure { get; set; }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        [field: SerializeField]
        public double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the magnitude of the velocity.
        /// </summary>
        [field: SerializeField]
        public double Velocity { get; set; }

        /// <summary>
        /// Returns the fluid state for a perfect vacuum.
        /// </summary>
        public static FluidState Vacuum => new FluidState()
            {
                Pressure = 0.0f,
                Temperature = 0.0f,
                Velocity = 0.0f
            };

        public FluidState( double pressure, double temperature, double velocity )
        {
            this.Pressure = pressure;
            this.Temperature = temperature;
            this.Velocity = velocity;
        }

        public override string ToString()
        {
            return $"P={Pressure} Pa, T={Temperature} K, V={Velocity} m/s";
        }
    }
}