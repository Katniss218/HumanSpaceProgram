using HSP.Spatial;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.CelestialBodies.Atmospheres
{
    [RequireComponent( typeof( ICelestialBody ) )]
    public abstract class Atmosphere : MonoBehaviour
    {
        internal static List<Atmosphere> _activeAtmospheres = new();

        /// <summary>
        /// The height of the atmosphere above the celestial body's surface.
        /// </summary>
        public float Height { get; set; } = 100_000f;

        /// <summary>
        /// Gets the celestial body that this LOD sphere belongs to.
        /// </summary>
        public ICelestialBody CelestialBody { get; protected set; }

        protected virtual void Awake()
        {
            CelestialBody = this.GetComponent<ICelestialBody>();
        }

        protected virtual void OnEnable()
        {
            _activeAtmospheres.Add( this );
        }

        protected virtual void OnDisable()
        {
            _activeAtmospheres.Remove( this );
        }

        public abstract AtmosphereData GetData( float altitude );

        [MapsInheritingFrom( typeof( Atmosphere ) )]
        public static SerializationMapping AtmosphereMapping()
        {
            return new MemberwiseSerializationMapping<Atmosphere>()
                .WithMember( "height", o => o.Height );
        }
    }

    /// <summary>
    /// A simple atmospheric model that uses exponential falloff over scale-height, clamped at a maximum height.
    /// </summary>
    [RequireComponent( typeof( ICelestialBody ) )]
    public class ExponentialScaleHeightAtmosphere : Atmosphere
    {
        /// <summary>
        /// The specific gas constant R of the atmosphere, in [J/(kg·K)].
        /// </summary>
        public float SpecificGasConstant { get; set; } = 287.05f;

        /// <summary>
        /// The scale height of the atmosphere, in [m].
        /// </summary>
        /// <remarks>
        /// This is the height over which the atmospheric pressure decreases by a factor of e (approximately 2.71828).
        /// </remarks>
        public float ScaleHeight { get; set; } = 8000f;

        /// <summary>
        /// The surface pressure of the atmosphere, in [Pa].
        /// </summary>
        public float SurfacePressure { get; set; } = 101325f;

        /// <summary>
        /// The surface temperature of the atmosphere, in [K].
        /// </summary>
        public float SurfaceTemperature { get; set; } = 288.15f;

        public override AtmosphereData GetData( float altitude )
        {
            altitude = Mathf.Clamp( altitude, 0, Height );
            float P = SurfacePressure * Mathf.Exp( -altitude / ScaleHeight );

            return new AtmosphereData( SpecificGasConstant, P, SurfaceTemperature, Vector3.zero );
        }


        [MapsInheritingFrom( typeof( ExponentialScaleHeightAtmosphere ) )]
        public static SerializationMapping ExponentialScaleHeightAtmosphereMapping()
        {
            return new MemberwiseSerializationMapping<ExponentialScaleHeightAtmosphere>()
                .WithMember( "height", o => o.Height )
                .WithMember( "specific_gas_constant", o => o.SpecificGasConstant )
                .WithMember( "scale_height", o => o.ScaleHeight )
                .WithMember( "surface_pressure", o => o.SurfacePressure )
                .WithMember( "surface_temperature", o => o.SurfaceTemperature );
        }
    }

    [SpatialDataProvider( typeof( SpatialAtmosphere ), "default_atmosphere_provider" )]
    public static class AtmosphereProvider
    {
        [SpatialValueProviderMode( "point" )]
        public static AtmosphereData Point( Vector3 point )
        {
            // Find the most influential atmosphere at the specified position.
            AtmosphereData result = default;
            double highestInfluence = 0f;

            foreach( var atmosphere in Atmosphere._activeAtmospheres )
            {
                Vector3 toPosition = point - atmosphere.CelestialBody.ReferenceFrameTransform.Position;
                double altitude = toPosition.magnitude - atmosphere.CelestialBody.Radius;

                if( altitude < 0f || altitude > atmosphere.Height )
                    continue;

                // Simple linear influence based on altitude.
                double influence = 1f - (altitude / atmosphere.Height);
                if( influence > highestInfluence )
                {
                    highestInfluence = influence;

                    // Simple exponential atmosphere model.
                    double pressure = Math.Exp( -altitude / 8000f ) * 101325f; // Pa
                    double temperature = 288.15f - (0.0065f * altitude); // K
                    Vector3 windVelocity = Vector3.zero; // No wind for now.

                    result = new AtmosphereData()
                    {
                        SpecificGasConstant = 287.05, // J/(kg·K) for dry air
                        Pressure = pressure,
                        Temperature = temperature,
                        WindVelocity = windVelocity
                    };
                }
            }

            return result;
        }

       /* [SpatialValueProviderMode( "line" )]
        public static AtmosphereData Line( (Vector3, Vector3) line )
        {
            throw new NotImplementedException();
        }*/
    }
}