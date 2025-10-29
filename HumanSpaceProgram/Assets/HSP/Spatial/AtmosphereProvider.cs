using System;
using UnityEngine;
using UnityPlus.OverridableValueProviders;

namespace HSP.Spatial
{
    public readonly struct AtmosphereData
    {
        public float SpecificGasConstant { get; }
        public float Pressure { get; }      // Pa
        public float Density { get; }      // Pa
        public float Temperature { get; }   // K
        public Vector3 WindVelocity { get; } // m/s (scene space)

        public AtmosphereData( float specificGasConstant, float pressure, float temperature, Vector3 windVelocity )
        {
            this.SpecificGasConstant = specificGasConstant;
            this.Pressure = pressure;
            this.Density = pressure / (specificGasConstant * temperature);
            this.Temperature = temperature;
            this.WindVelocity = windVelocity;
        }
    }

    public static class SpatialAtmosphere
    {
        private class AtmosphereDataCombiner
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

    public class Test
    {
        public void Test2()
        {
            // spatial called like:

            if( SpatialAtmosphere.EvaluatePoint( new Vector3( 0, 1000, 0 ), out AtmosphereData data ) )
            {
                Debug.Log( $"Atmosphere at point: Pressure={data.Pressure}, Temperature={data.Temperature}" );
            }
        }
    }
}