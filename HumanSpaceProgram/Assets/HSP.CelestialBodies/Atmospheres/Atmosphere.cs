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
    [RequireComponent( typeof( CelestialBody ) )]
    public class Atmosphere : MonoBehaviour
    {
        internal static List<Atmosphere> _activeAtmospheres = new();

        public float Height { get; set; } = 100_000f;

        public Material sharedMaterial { get; set; }

        /// <summary>
        /// Gets the celestial body that this LOD sphere belongs to.
        /// </summary>
        public CelestialBody CelestialBody { get; private set; }

        internal void UpdateMaterialValues( Func<RenderTexture> ColorRenderTextureGetter, Func<RenderTexture> DepthRenderTextureGetter, Light light )
        {
            if( sharedMaterial == null )
                return;

            // The `_Texture` property name gets overriden by something else... Unity... >:{
            sharedMaterial.SetTexture( Shader.PropertyToID( "_texgsfs" ), ColorRenderTextureGetter.Invoke() );
            sharedMaterial.SetTexture( Shader.PropertyToID( "_DepthBuffer" ), DepthRenderTextureGetter.Invoke(), RenderTextureSubElement.Depth );

            sharedMaterial.SetVector( Shader.PropertyToID( "_Center" ), CelestialBody.ReferenceFrameTransform.Position );
            sharedMaterial.SetVector( Shader.PropertyToID( "_SunDirection" ), -light.transform.forward );
            sharedMaterial.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_MinRadius" ), (float)CelestialBody.Radius );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_MaxRadius" ), (float)(CelestialBody.Radius + Height) );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_InScatteringPointCount" ), 16 );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_OpticalDepthPointCount" ), 8 );
            sharedMaterial.SetFloat( Shader.PropertyToID( "_DensityFalloffPower" ), 13.7f );
        }

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            CelestialBody = this.GetComponent<CelestialBody>();
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