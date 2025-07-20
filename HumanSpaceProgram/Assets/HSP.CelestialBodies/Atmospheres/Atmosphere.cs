using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityPlus.Serialization;

namespace HSP.CelestialBodies.Atmospheres
{
    /// <summary>
    /// Add this to a celestial body to draw an atmosphere at its location.
    /// </summary>
    [RequireComponent( typeof( ICelestialBody ) )]
    public class Atmosphere : MonoBehaviour
    {
        internal static List<Atmosphere> _activeAtmospheres = new();

        /// <summary>
        /// The height of the atmosphere above the celestial body's surface.
        /// </summary>
        public float Height { get; set; } = 100_000f;

        /// <summary>
        /// The material used to render the atmosphere.
        /// </summary>
        public Material sharedMaterial { get; set; }

        /// <summary>
        /// Clone of <see cref="sharedMaterial"/>, contains runtime properties for this atmosphere.
        /// </summary>
        internal Material material { get; private set; }

        /// <summary>
        /// Gets the celestial body that this LOD sphere belongs to.
        /// </summary>
        public ICelestialBody CelestialBody { get; private set; }

        internal void UpdateMaterialValues( Func<RenderTexture> ColorRenderTextureGetter, Func<RenderTexture> DepthRenderTextureGetter, Light light )
        {
            if( sharedMaterial == null )
                return;

            if( material == null )
                material = new Material( sharedMaterial );

            // The `_Texture` property name gets overriden by something else... Unity... >:{
            material.SetTexture( Shader.PropertyToID( "_texgsfs" ), ColorRenderTextureGetter.Invoke() );
            material.SetTexture( Shader.PropertyToID( "_DepthBuffer" ), DepthRenderTextureGetter.Invoke(), RenderTextureSubElement.Depth );

            material.SetVector( Shader.PropertyToID( "_Center" ), CelestialBody.ReferenceFrameTransform.Position );
            material.SetVector( Shader.PropertyToID( "_SunDirection" ), -light.transform.forward );
            material.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
            material.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
            material.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
            material.SetFloat( Shader.PropertyToID( "_MinRadius" ), (float)CelestialBody.Radius );
            material.SetFloat( Shader.PropertyToID( "_MaxRadius" ), (float)(CelestialBody.Radius + Height) );
            material.SetFloat( Shader.PropertyToID( "_InScatteringPointCount" ), 16 );
            material.SetFloat( Shader.PropertyToID( "_OpticalDepthPointCount" ), 8 );
            material.SetFloat( Shader.PropertyToID( "_DensityFalloffPower" ), 13.7f );
        }

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
                .WithMember( "height", o => o.Height )
                .WithMember( "shared_material", ObjectContext.Asset, o => o.sharedMaterial );
        }
    }
}