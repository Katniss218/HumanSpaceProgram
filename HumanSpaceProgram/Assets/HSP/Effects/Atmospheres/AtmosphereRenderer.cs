using HSP.Core.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace HSP.CelestialBodies
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public class AtmosphereRenderer : MonoBehaviour
    {
        Shader _shader;

        [SerializeField]
        Material _material;

        [SerializeField]
        Camera _srcCam1;

        [SerializeField]
        Camera _srcCam2;

        Camera _camera;
        CommandBuffer _cmdAtmospheres;
        CommandBuffer _cmdComposition;
        CommandBuffer _cmdMergeDepth;

        Vector3 _center = Vector3.zero;
        [SerializeField]
        new Light light;

        [SerializeField]
        RenderTexture _rt;
        
        [SerializeField]
        Material _mergeDepthMat;

        [SerializeField]
        RenderTexture _mainColorRT;

        [SerializeField]
        RenderTexture _cam1DepthRT;

        [SerializeField]
        RenderTexture _cam2DepthRT;
        
        [SerializeField]
        RenderTexture _dstColorRT;

        [SerializeField]
        RenderTexture _dstDepthRT;

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
            _cmdMergeDepth = new CommandBuffer()
            {
                name = "Atmospheres - Merge Depth"
            };

            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch += OnReferenceFrameSwitch;

            _srcCam1.SetTargetBuffers( _mainColorRT.colorBuffer, _cam1DepthRT.depthBuffer );
            _srcCam2.SetTargetBuffers( _mainColorRT.colorBuffer, _cam2DepthRT.depthBuffer );
            _camera.SetTargetBuffers( _dstColorRT.colorBuffer, _dstDepthRT.depthBuffer );
        }

        void OnDestroy()
        {
            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch -= OnReferenceFrameSwitch;

            if( _rt != null )
                RenderTexture.ReleaseTemporary( _rt );

            _cmdAtmospheres?.Release();
            _cmdComposition?.Release();
            _cmdMergeDepth?.Release();
        }

        private void Update()
        {
            _srcCam1.depthTextureMode |= DepthTextureMode.Depth;
            _srcCam2.depthTextureMode |= DepthTextureMode.Depth;
            
            _camera.depthTextureMode &= ~DepthTextureMode.Depth; // NO DEPTH. this breaks (for now?).
        }

        void OnPreRender()
        {
            _material.SetVector( Shader.PropertyToID( "_Center" ), _center );
            _material.SetVector( Shader.PropertyToID( "_SunDirection" ), -light.transform.forward );
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

            this._srcCam2.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdMergeDepth );

            //this._cam1ColorRT = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 0, RenderTextureFormat.ARGB32 );
           // this._cam1DepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 24, RenderTextureFormat.Depth );
            //this._cam2ColorRT = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 0, RenderTextureFormat.ARGB32 );
            //this._cam2DepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 24, RenderTextureFormat.Depth );
            this._rt = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 0, RenderTextureFormat.ARGB32 );

            UpdateCommandBuffers();
            this._srcCam2.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdMergeDepth );
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

            /*if( _cam1ColorRT != null )
            {
                RenderTexture.ReleaseTemporary( _cam1ColorRT );
                _cam1ColorRT = null;
            }
            if( _cam1DepthRT != null )
            {
                RenderTexture.ReleaseTemporary( _cam1DepthRT );
                _cam1DepthRT = null;
            }

            if( _cam2ColorRT != null )
            {
                RenderTexture.ReleaseTemporary( _cam2ColorRT );
                _cam2ColorRT = null;
            }
            if( _cam2DepthRT != null )
            {
                RenderTexture.ReleaseTemporary( _cam2DepthRT );
                _cam2DepthRT = null;
            }*/
        }

        private void UpdateCommandBuffers()
        {
            _cmdMergeDepth.Clear();
            _cmdMergeDepth.SetGlobalTexture( "_Input1Depth", this._cam1DepthRT );
            _cmdMergeDepth.SetGlobalTexture( "_Input2Depth", this._cam2DepthRT );
            _cmdMergeDepth.SetGlobalFloat( Shader.PropertyToID( "_Input1Near" ), _srcCam1.nearClipPlane );
            _cmdMergeDepth.SetGlobalFloat( Shader.PropertyToID( "_Input1Far" ), _srcCam1.farClipPlane );
            _cmdMergeDepth.SetGlobalFloat( Shader.PropertyToID( "_Input2Near" ), _srcCam2.nearClipPlane );
            _cmdMergeDepth.SetGlobalFloat( Shader.PropertyToID( "_Input2Far" ), _srcCam2.farClipPlane );
            _cmdMergeDepth.SetGlobalFloat( Shader.PropertyToID( "_DstNear" ), _camera.nearClipPlane );
            _cmdMergeDepth.SetGlobalFloat( Shader.PropertyToID( "_DstFar" ), _camera.farClipPlane );
            _cmdMergeDepth.SetRenderTarget( _dstColorRT, _dstDepthRT );
            _cmdMergeDepth.Blit( null, BuiltinRenderTextureType.CurrentActive, _mergeDepthMat, 0 );

            _cmdAtmospheres.Clear();
            _cmdAtmospheres.SetGlobalTexture( Shader.PropertyToID( "_Texture" ), _dstColorRT ); // `_Texture` gets overriden by something else... Unity... >:{
            _cmdAtmospheres.SetGlobalTexture( Shader.PropertyToID( "_texgsfs" ), BuiltinRenderTextureType.CurrentActive ); // `_Texture` gets overriden by something else... Unity... >:{
            _cmdAtmospheres.SetRenderTarget( _rt );
            _cmdAtmospheres.Blit( null, _rt, _material, 0 );

            _cmdComposition.Clear();
            _cmdComposition.Blit( _rt, BuiltinRenderTextureType.CurrentActive );
        }
    }
}