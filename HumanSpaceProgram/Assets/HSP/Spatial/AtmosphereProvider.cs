using System;
using UnityEngine;
using UnityPlus.OverridableValueProviders;

namespace HSP.Spatial
{
    public readonly struct AtmosphereData
    {
        public double SpecificGasConstant { get; }
        public double Pressure { get; }      // Pa
        public double Density { get; }      // Kg/m3
        public double Temperature { get; }   // K
        public Vector3 WindVelocity { get; } // m/s (scene space)

        public AtmosphereData( double specificGasConstant, double pressure, double temperature, Vector3 windVelocity )
        {
            this.SpecificGasConstant = specificGasConstant;
            this.Pressure = pressure;
            this.Density = pressure / (specificGasConstant * temperature);
            this.Temperature = temperature;
            this.WindVelocity = windVelocity;
        }
    }

    /// <summary>
    /// Used to query atmospheric data at specific points or along lines in space.
    /// </summary>
    public static class SpatialAtmosphere
    {
        private sealed class AtmosphereDataCombiner
        {
            public static AtmosphereData Combine( ReadOnlyMemory<AtmosphereData> mem )
            {
                var span = mem.Span;
                if( span.Length == 0 )
                    throw new InvalidOperationException( "Cannot combine zero elements." );

                // For simplicity, just take the first provider's data.
                return span[0];
            }
        }

        private static OverridableValueProviderRegistry<Vector3, AtmosphereData> _providers_point = new( AtmosphereDataCombiner.Combine );
        private static OverridableValueProviderRegistry<(Vector3, Vector3), AtmosphereData> _providers_line = new( AtmosphereDataCombiner.Combine );

        public static bool EvaluatePoint( Vector3 point, out AtmosphereData value )
        {
            return _providers_point.TryGetValue( point, out value );
        }

        public static bool EvaluateLine( Vector3 start, Vector3 end, out AtmosphereData value )
        {
            return _providers_line.TryGetValue( (start, end), out value );
        }
    }
}