using HSP.GameplayScene.Cameras;
using HSP.Core.ReferenceFrames;
using UnityEngine;
using UnityEngine.Rendering;

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

        Vector3 _center = Vector3.zero;
        [SerializeField]
        new public Light light { get; set; }

        [SerializeField]
        RenderTexture _rt;

        void OnReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            Vector3Dbl oldAirfPos = data.OldFrame.TransformPosition( _center );
            _center = (Vector3)data.NewFrame.InverseTransformPosition( oldAirfPos );
        }

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

            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch += OnReferenceFrameSwitch;
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
            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch -= OnReferenceFrameSwitch;

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
            this._rt = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );

            //                                     The `_Texture` property name gets overriden by something else... Unity... >:{
            _atmosphereMaterial.SetTexture( Shader.PropertyToID( "_texgsfs" ), GameplaySceneCameraManager.ColorRenderTexture );
            _atmosphereMaterial.SetTexture( Shader.PropertyToID( "_DepthBuffer" ), GameplaySceneDepthBufferCombiner.CombinedDepthRenderTexture, RenderTextureSubElement.Depth );

            _atmosphereMaterial.SetVector( Shader.PropertyToID( "_Center" ), _center );
            _atmosphereMaterial.SetVector( Shader.PropertyToID( "_SunDirection" ), -light.transform.forward );
            _atmosphereMaterial.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_MinRadius" ), 6371000f );
            _atmosphereMaterial.SetFloat( Shader.PropertyToID( "_MaxRadius" ), 6500000f );
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
    }
}