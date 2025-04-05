using HSP.CelestialBodies.Surfaces;
using HSP.ReferenceFrames;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityPlus.Serialization;

namespace HSP.CelestialBodies
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public class AtmosphereRenderer : SingletonMonoBehaviour<AtmosphereRenderer>
    {
        [SerializeField]
        Shader _atmosphereShader;
        public Shader AtmosphereShader
        {
            get => _atmosphereShader;
            set
            {
                _atmosphereShader = value;
                _atmosphereMaterial = new Material( _atmosphereShader );
            }
        }

        Material _atmosphereMaterial;

        Camera _camera;
        CommandBuffer _cmdAtmospheres;
        CommandBuffer _cmdComposition;

        public static CelestialBody Body;

        [SerializeField]
        new public Light light { get; set; }

        public float Height { get; set; } = 140_000;

        [SerializeField]
        RenderTexture _rt;

        public Func<RenderTexture> ColorRenderTextureGetter { get; set; }
        public Func<RenderTexture> DepthRenderTextureGetter { get; set; }

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            _cmdAtmospheres = new CommandBuffer()
            {
                name = "HSP - Atmospheres - Render"
            };
            _cmdComposition = new CommandBuffer()
            {
                name = "HSP - Atmospheres - Composition"
            };
        }

        void OnEnable()
        {
            if( _atmosphereShader == null )
                _atmosphereShader = Shader.Find( "Hidden/Atmosphere" );

            if( _atmosphereMaterial == null )
                _atmosphereMaterial = new Material( AtmosphereShader );
        }

        void OnDestroy()
        {
            if( _rt != null )
                RenderTexture.ReleaseTemporary( _rt );

            _cmdAtmospheres?.Release();
            _cmdComposition?.Release();
        }

        void Update()
        {
            _camera.depthTextureMode &= ~DepthTextureMode.Depth; // NO DEPTH. this breaks (for now?).
        }

        void OnPreRender()
        {
            if( instance._atmosphereMaterial == null )
                return;
            if( Body == null )
                return;

            this._rt = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );

            //                                     The `_Texture` property name gets overriden by something else... Unity... >:{
            _atmosphereMaterial.SetTexture( Shader.PropertyToID( "_texgsfs" ), ColorRenderTextureGetter.Invoke() );
            _atmosphereMaterial.SetTexture( Shader.PropertyToID( "_DepthBuffer" ), DepthRenderTextureGetter.Invoke(), RenderTextureSubElement.Depth );

            _atmosphereMaterial.SetVector( Shader.PropertyToID( "_Center" ), Body.ReferenceFrameTransform.Position );
            _atmosphereMaterial.SetVector( Shader.PropertyToID( "_SunDirection" ), -light.transform.forward );
            _atmosphereMaterial.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_MinRadius" ), (float)Body.Radius );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_MaxRadius" ), (float)(Body.Radius + Height) );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_InScatteringPointCount" ), 16 );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_OpticalDepthPointCount" ), 8 );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_DensityFalloffPower" ), 13.7f );

            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );

            UpdateCommandBuffers();
            this._camera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );
        }

        void OnPostRender()
        {
            // if doesn't work, move to the front of onprecull. Here it should allow others to reuse these textures.
            if( _rt != null )
            {
                RenderTexture.ReleaseTemporary( _rt );
                _rt = null;
            }
        }

        public void UpdateCommandBuffers()
        {
            _cmdAtmospheres.Clear();
            _cmdAtmospheres.SetRenderTarget( _rt );
            _cmdAtmospheres.Blit( null, _rt, _atmosphereMaterial, 0 );

            _cmdComposition.Clear();
            _cmdComposition.Blit( _rt, (RenderTexture)null );
        }

#warning TODO - add a renderer per planet with atmosphere, build one cmdbuffer using each of them.
        [MapsInheritingFrom( typeof( AtmosphereRenderer ) )]
        public static SerializationMapping AtmosphereRendererMapping()
        {
            return new MemberwiseSerializationMapping<AtmosphereRenderer>();
        }
    }
}