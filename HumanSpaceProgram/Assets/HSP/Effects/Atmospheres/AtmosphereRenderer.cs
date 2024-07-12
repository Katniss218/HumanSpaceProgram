using HSP.Cameras;
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

            GameplaySceneCameraSystem.NearCamera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdMergeDepth );
            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );

            //this._dstColorRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );
           // this._dstDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 24, RenderTextureFormat.Depth );
            this._rt = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );
            //_camera.SetTargetBuffers( _dstColorRT.colorBuffer, _dstDepthRT.depthBuffer );

            UpdateCommandBuffers();
            GameplaySceneCameraSystem.NearCamera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdMergeDepth );
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

            /*if( _dstColorRT != null )
            {
                RenderTexture.ReleaseTemporary( _dstColorRT );
                _dstColorRT = null;
            }*/
            /*if( _dstDepthRT != null )
            {
                RenderTexture.ReleaseTemporary( _dstDepthRT );
                _dstDepthRT = null;
            }*/
        }

        private void UpdateCommandBuffers()
        {
            _cmdMergeDepth.Clear();
            _mergeDepthMat.SetTexture( Shader.PropertyToID( "_Input1Depth" ), GameplaySceneCameraSystem.FarDepthRenderTexture, RenderTextureSubElement.Depth );
            _mergeDepthMat.SetTexture( Shader.PropertyToID( "_Input2Depth" ), GameplaySceneCameraSystem.NearDepthRenderTexture, RenderTextureSubElement.Depth );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input1Near" ), GameplaySceneCameraSystem.FarCamera.nearClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input1Far" ), GameplaySceneCameraSystem.FarCamera.farClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input2Near" ), GameplaySceneCameraSystem.NearCamera.nearClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input2Far" ), GameplaySceneCameraSystem.NearCamera.farClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_DstNear" ), _camera.nearClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_DstFar" ), _camera.farClipPlane );
            _cmdMergeDepth.SetRenderTarget( _dstColorRT, _dstDepthRT );
            _cmdMergeDepth.Blit( null, BuiltinRenderTextureType.CurrentActive, _mergeDepthMat, 0 );

            _cmdAtmospheres.Clear();
            //                                     The `_Texture` property name gets overriden by something else... Unity... >:{
            _material.SetTexture( Shader.PropertyToID( "_texgsfs" ), GameplaySceneCameraSystem.ColorRenderTexture );
            _material.SetTexture( Shader.PropertyToID( "_DepthBuffer" ), _dstDepthRT, RenderTextureSubElement.Depth );
            _cmdAtmospheres.SetRenderTarget( _rt );
            _cmdAtmospheres.Blit( null, _rt, _material, 0 );

            _cmdComposition.Clear();
            _cmdComposition.Blit( _rt, (RenderTexture)null );
        }
    }
}