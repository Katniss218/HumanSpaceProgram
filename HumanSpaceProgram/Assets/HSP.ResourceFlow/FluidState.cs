using System;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// State information about an unspecified fluid.
    /// </summary>
    [Serializable]
    public struct FluidState
    {
        /// <summary>
        /// Gets or sets the pressure.
        /// </summary>
        [field: SerializeField]
        public float Pressure { get; set; }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        [field: SerializeField]
        public float Temperature { get; set; }

        /// <summary>
        /// Gets or sets the magnitude of the velocity.
        /// </summary>
        [field: SerializeField]
        public float Velocity { get; set; }

        /// <summary>
        /// Checks whether or not this fluid state describes a perfect vacuum.
        /// </summary>
        public bool IsVacuum => (this.Pressure == 0.0f && this.Temperature == 0.0f && this.Velocity == 0.0f);

        /// <summary>
        /// Returns the fluid state for a perfect vacuum.
        /// </summary>
        public static FluidState Vacuum => new FluidState()
            {
                Pressure = 0.0f,
                Temperature = 0.0f,
                Velocity = 0.0f
            };

        public FluidState( float pressure, float temperature, float velocity )
        {
            this.Pressure = pressure;
            this.Temperature = temperature;
            this.Velocity = velocity;
        }
    }
}