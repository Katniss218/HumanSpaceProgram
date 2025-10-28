using HSP.Spatial;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.CelestialBodies.Atmospheres
{
    /// <summary>
    /// Add this to a celestial body to draw an atmosphere at its location.
    /// </summary>
    [RequireComponent( typeof( ICelestialBody ) )]
    public class Atmosphere : MonoBehaviour
    {
#warning TODO - curve-based atmosphere
#warning TODO - scaleheight-based atmosphere
        internal static List<Atmosphere> _activeAtmospheres = new();

        /// <summary>
        /// The height of the atmosphere above the celestial body's surface.
        /// </summary>
        public float Height { get; set; } = 100_000f;

        /// <summary>
        /// Gets the celestial body that this LOD sphere belongs to.
        /// </summary>
        public ICelestialBody CelestialBody { get; private set; }

        void Awake()
        {
            CelestialBody = this.GetComponent<ICelestialBody>();
        }

        void OnEnable()
        {
            _activeAtmospheres.Add( this );
        }

        void OnDisable()
        {
            _activeAtmospheres.Remove( this );
        }


        [MapsInheritingFrom( typeof( Atmosphere ) )]
        public static SerializationMapping AtmosphereMapping()
        {
            return new MemberwiseSerializationMapping<Atmosphere>()
                .WithMember( "height", o => o.Height );
        }

        [SpatialValueProvider( "atmosphere" )] // keyed by tuple (param type, return type)
        private static AtmosphereData ProvideAtmosphereData( Vector3 position )
        {
#warning TODO - allow multiple providers for the same key, based on priority/sum/combine all?
            // basically needs to allow other types of atmosphere to also register themsleves as providers. and coexist side-by-side.


            // Find the most influential atmosphere at the specified position.
            AtmosphereData result = default;
            float highestInfluence = 0f;

            foreach( var atmosphere in _activeAtmospheres )
            {
                Vector3Dbl toPosition = (Vector3Dbl)position - atmosphere.CelestialBody.Position;
                float altitude = (float)toPosition.magnitude - atmosphere.CelestialBody.Radius;

                if( altitude < 0f || altitude > atmosphere.Height )
                    continue;

                // Simple linear influence based on altitude.
                float influence = 1f - (altitude / atmosphere.Height);
                if( influence > highestInfluence )
                {
                    highestInfluence = influence;

                    // Simple exponential atmosphere model.
                    float pressure = Mathf.Exp( -altitude / 8000f ) * 101325f; // Pa
                    float temperature = 288.15f - (0.0065f * altitude); // K
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
    }
}