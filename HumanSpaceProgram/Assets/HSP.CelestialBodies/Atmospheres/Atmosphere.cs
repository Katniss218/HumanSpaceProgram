using HSP.ReferenceFrames;
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

        public abstract AtmosphereData GetData( Vector3 scenePoint );

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

        public override AtmosphereData GetData( Vector3 scenePoint )
        {
            var sceneFrame = this.CelestialBody.ReferenceFrameTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame();

            var frame = this.CelestialBody.ReferenceFrameTransform.NonInertialReferenceFrame();
            var bodySpacePos = frame.InverseTransformPosition( sceneFrame.TransformPosition( scenePoint ) );

            var altitude = Math.Clamp( bodySpacePos.magnitude - this.CelestialBody.Radius, 0, Height );
            double P = SurfacePressure * Math.Exp( -altitude / ScaleHeight );

            Vector3 sceneWindVelocity = (Vector3)sceneFrame.InverseTransformVelocity( this.CelestialBody.ReferenceFrameTransform.AbsoluteVelocity + frame.GetTangentialVelocity( bodySpacePos ) );

            return new AtmosphereData( SpecificGasConstant, P, SurfaceTemperature, sceneWindVelocity );
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
        [SpatialDataProviderMode( "point" )]
        public static AtmosphereData Point( Vector3 point )
        {
            double closestDistanceSq = double.MaxValue;
            Atmosphere closest = null;

            foreach( var atmosphere in Atmosphere._activeAtmospheres )
            {
                double distSq = (atmosphere.CelestialBody.ReferenceFrameTransform.Position - point).sqrMagnitude;
                if( distSq < closestDistanceSq )
                {
                    closest = atmosphere;
                    closestDistanceSq = distSq;
                }
            }

            return closest.GetData( point );
        }

        /* [SpatialDataProviderMode( "line" )]
         public static AtmosphereData Line( (Vector3, Vector3) line )
         {
             throw new NotImplementedException();
         }*/
    }
}