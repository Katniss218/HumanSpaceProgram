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
    }
}