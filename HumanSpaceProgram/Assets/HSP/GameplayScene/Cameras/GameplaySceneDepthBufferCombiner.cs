using HSP.Core;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace HSP.GameplayScene.Cameras
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public sealed class GameplaySceneDepthBufferCombiner : SingletonMonoBehaviour<GameplaySceneDepthBufferCombiner>
    {
        [field: SerializeField]
        public Material MergeDepthMat { get; set; }

        [field: SerializeField]
        public Camera FarCamera { get; set; }
        [field: SerializeField]
        public Camera NearCamera { get; set; }
        [field: SerializeField]
        public Camera EffectCamera { get; set; }

        CommandBuffer _cmdMergeDepth;

        RenderTexture _dstColorRT;
        RenderTexture _dstDepthRT;

        public static RenderTexture CombinedDepthRenderTexture => instance._dstDepthRT;

        void Awake()
        {
            _cmdMergeDepth = new CommandBuffer()
            {
                name = "HSP - Merge Depth"
            };
        }

        void OnDestroy()
        {
            _cmdMergeDepth?.Release();
        }

        private void UpdateMergeDepthCommandBuffer()
        {
            _cmdMergeDepth.Clear();
            MergeDepthMat.SetTexture( Shader.PropertyToID( "_Input1Depth" ), GameplaySceneCameraManager.FarDepthRenderTexture, RenderTextureSubElement.Depth );
            MergeDepthMat.SetTexture( Shader.PropertyToID( "_Input2Depth" ), GameplaySceneCameraManager.NearDepthRenderTexture, RenderTextureSubElement.Depth );
            MergeDepthMat.SetFloat( Shader.PropertyToID( "_Input1Near" ), FarCamera.nearClipPlane );
            MergeDepthMat.SetFloat( Shader.PropertyToID( "_Input1Far" ), FarCamera.farClipPlane );
            MergeDepthMat.SetFloat( Shader.PropertyToID( "_Input2Near" ), NearCamera.nearClipPlane );
            MergeDepthMat.SetFloat( Shader.PropertyToID( "_Input2Far" ), NearCamera.farClipPlane );
            MergeDepthMat.SetFloat( Shader.PropertyToID( "_DstNear" ), EffectCamera.nearClipPlane );
            MergeDepthMat.SetFloat( Shader.PropertyToID( "_DstFar" ), EffectCamera.farClipPlane );
            _cmdMergeDepth.SetRenderTarget( _dstColorRT, _dstDepthRT );
            if( instance.NearCamera.enabled )
                _cmdMergeDepth.Blit( null, BuiltinRenderTextureType.CurrentActive, MergeDepthMat, 0 );
            else
                _cmdMergeDepth.Blit( null, BuiltinRenderTextureType.CurrentActive, MergeDepthMat, 1 );
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_BEFORE_RENDERING, "merge_depth" )]
        private static void OnBeforeRendering()
        {
            // tex used as output for depth merging.
            instance._dstColorRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );
            instance._dstDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 32, RenderTextureFormat.Depth );
            instance.EffectCamera.SetTargetBuffers( instance._dstColorRT.colorBuffer, instance._dstDepthRT.depthBuffer );

            instance.EffectCamera.RemoveCommandBuffer( CameraEvent.BeforeForwardOpaque, instance._cmdMergeDepth );

            instance.UpdateMergeDepthCommandBuffer(); // This needs to happen *before* near camera renders, otherwise it tries to use textures that have been released.\
                                                      // that is, if the buffer is set up after the nearcam renders, it will be used on the next frame,
                                                      //   and the textures are reallocated between frames.
            instance.EffectCamera.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, instance._cmdMergeDepth );
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_RENDERING, "merge_depth" )]
        private static void OnAfterRendering()
        {
            // tex used as output for depth merging.
            if( instance._dstColorRT != null )
            {
                RenderTexture.ReleaseTemporary( instance._dstColorRT );
                instance._dstColorRT = null;
            }
            if( instance._dstDepthRT != null )
            {
                RenderTexture.ReleaseTemporary( instance._dstDepthRT );
                instance._dstDepthRT = null;
            }
        }
    }
}