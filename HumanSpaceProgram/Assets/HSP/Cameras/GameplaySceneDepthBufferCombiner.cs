using HSP.Core;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace HSP.Cameras
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public sealed class GameplaySceneDepthBufferCombiner : SingletonMonoBehaviour<GameplaySceneDepthBufferCombiner>
    {
        [SerializeField]
        Material _mergeDepthMat;

        [SerializeField]
        Camera _farCamera;
        [SerializeField]
        Camera _nearCamera;
        [SerializeField]
        Camera _effectCamera;

        CommandBuffer _cmdMergeDepth;

        [SerializeField]
        RenderTexture _dstColorRT;

        [SerializeField]
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
            _mergeDepthMat.SetTexture( Shader.PropertyToID( "_Input1Depth" ), GameplaySceneCameraManager.FarDepthRenderTexture, RenderTextureSubElement.Depth );
            _mergeDepthMat.SetTexture( Shader.PropertyToID( "_Input2Depth" ), GameplaySceneCameraManager.NearDepthRenderTexture, RenderTextureSubElement.Depth );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input1Near" ), _farCamera.nearClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input1Far" ), _farCamera.farClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input2Near" ), _nearCamera.nearClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_Input2Far" ), _nearCamera.farClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_DstNear" ), _effectCamera.nearClipPlane );
            _mergeDepthMat.SetFloat( Shader.PropertyToID( "_DstFar" ), _effectCamera.farClipPlane );
            _cmdMergeDepth.SetRenderTarget( _dstColorRT, _dstDepthRT );
            _cmdMergeDepth.Blit( null, BuiltinRenderTextureType.CurrentActive, _mergeDepthMat, 0 );
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_BEFORE_RENDERING, "merge_depth" )]
        private static void OnBeforeRendering()
        {
            // tex used as output for depth merging.
            instance._dstColorRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );
            instance._dstDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 32, RenderTextureFormat.Depth );
            instance._effectCamera.SetTargetBuffers( instance._dstColorRT.colorBuffer, instance._dstDepthRT.depthBuffer );

            instance._nearCamera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, instance._cmdMergeDepth );

            instance.UpdateMergeDepthCommandBuffer(); // This needs to happen *before* near camera renders, otherwise it tries to use textures that have been released.\
                                                      // that is, if the buffer is set up after the nearcam renders, it will be used on the next frame,
                                                      //   and the textures are reallocated between frames.
            instance._nearCamera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, instance._cmdMergeDepth );
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