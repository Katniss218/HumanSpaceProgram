using KSS.Core.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KSS.CelestialBodies
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public class AtmosphereRenderer : MonoBehaviour
    {
        Shader _shader;

        [SerializeField]
        Material _material;

        Camera _camera;
        CommandBuffer _cmdAtmospheres;
        CommandBuffer _cmdComposition;

        Vector3 _center = Vector3.zero;

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

            _shader = Shader.Find( "Hidden/Atmosphere" );
            _material = new Material( _shader );

            _cmdAtmospheres = new CommandBuffer()
            {
                name = "Atmospheres - Render"
            };
            _cmdComposition = new CommandBuffer()
            {
                name = "Atmospheres - Composition"
            };

            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch += OnReferenceFrameSwitch;
        }

        void OnDestroy()
        {
            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch -= OnReferenceFrameSwitch;

            if( _rt != null )
                RenderTexture.ReleaseTemporary( _rt );

            _cmdAtmospheres?.Release();
            _cmdComposition?.Release();
        }

        private void Update()
        {
            _camera.depthTextureMode &= ~DepthTextureMode.Depth; // NO DEPTH. this breaks (for now?).
        }

        void OnPreRender()
        {
            _material.SetVector( Shader.PropertyToID( "_Center" ), _center );
            _material.SetVector( Shader.PropertyToID( "_SunDirection" ), new Vector3( 1, 0, 1 ) );
            _material.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
            _material.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
            _material.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
            _material.SetFloat( Shader.PropertyToID( "_MinRadius" ), 6371000f );
            _material.SetFloat( Shader.PropertyToID( "_MaxRadius" ), 6500000f );
            _material.SetFloat( Shader.PropertyToID( "_InScatteringPointCount" ), 16 );
            _material.SetFloat( Shader.PropertyToID( "_OpticalDepthPointCount" ), 8 );
            _material.SetFloat( Shader.PropertyToID( "_DensityFalloffPower" ), 13.7f );

            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );
            this._rt = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 0, RenderTextureFormat.ARGB32 );
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

        private void UpdateCommandBuffers()
        {
            _cmdAtmospheres.Clear();
            _cmdAtmospheres.SetGlobalTexture( Shader.PropertyToID( "_texgsfs" ), BuiltinRenderTextureType.CurrentActive ); // `_Texture` gets overriden by something else... Unity... >:{
#warning TODO - this needs a merge of 2 depth textures (near and far) and use that as the depth, since stacked depth is clipped beyond 0..1
            _cmdAtmospheres.SetRenderTarget( _rt );
            _cmdAtmospheres.Blit( null, _rt, _material, 0 );

            _cmdComposition.Clear();
            _cmdComposition.Blit( _rt, BuiltinRenderTextureType.CurrentActive );
        }
    }
}