using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityPlus.Compositing
{
    public class DepthAndColorCopier : MonoBehaviour
    {
        /// <summary>
        /// The camera that rendered the depth and color.
        /// </summary>
        [field: SerializeField]
        public Camera SourceCam { get; set; }

        /// <summary>
        /// The camera that will render later with the copied depth and color.
        /// </summary>
        [field: SerializeField]
        public Camera TargetCam { get; set; }

        /// <summary>
        /// The shader to use when copying.
        /// </summary>
        [field: SerializeField]
        public Shader CopyShader { get; set; }

        Material _copyMaterial;

        RenderTexture _srcColorRT; // source of the depth and color.
        RenderTexture _srcDepthRT;

        [SerializeField]
        RenderTexture _tgtColorRT; // where to put the depth and color.
        RenderTexture _tgtDepthRT;

        void Start()
        {
            _srcColorRT = new RenderTexture( SourceCam.pixelWidth, SourceCam.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None );
            _srcColorRT.Create();
            _srcDepthRT = new RenderTexture( SourceCam.pixelWidth, SourceCam.pixelHeight, GraphicsFormat.None, GraphicsFormat.D32_SFloat_S8_UInt );
            _srcDepthRT.Create();

            _tgtColorRT = new RenderTexture( TargetCam.pixelWidth, TargetCam.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None );
            _tgtColorRT.Create();
            _tgtDepthRT = new RenderTexture( TargetCam.pixelWidth, TargetCam.pixelHeight, GraphicsFormat.None, GraphicsFormat.D32_SFloat_S8_UInt );
            _tgtDepthRT.Create();

            if( TargetCam.depthTextureMode == DepthTextureMode.None )
                TargetCam.depthTextureMode = DepthTextureMode.Depth;

            SourceCam.SetTargetBuffers( _srcColorRT.colorBuffer, _srcDepthRT.depthBuffer ); // for multi-camera, this needs to be cleared and swapped for each next camera.
            TargetCam.SetTargetBuffers( _tgtColorRT.colorBuffer, _tgtDepthRT.depthBuffer );

            _copyMaterial = new Material( CopyShader );

            _copyMaterial.SetTexture( "_InputColor", _srcColorRT );
            _copyMaterial.SetTexture( "_InputDepth", _srcDepthRT );

            CommandBuffer cmd = new CommandBuffer()
            {
                name = "Copy Depth And Color"
            };
            cmd.SetRenderTarget( _tgtColorRT, _tgtDepthRT );
            cmd.Blit( null, BuiltinRenderTextureType.CurrentActive, _copyMaterial, 0 );

            TargetCam.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, cmd );
        }

        void LateUpdate()
        {
            _copyMaterial.SetFloat( Shader.PropertyToID( "_SrcNear" ), SourceCam.nearClipPlane );
            _copyMaterial.SetFloat( Shader.PropertyToID( "_SrcFar" ), SourceCam.farClipPlane );

            _copyMaterial.SetFloat( Shader.PropertyToID( "_DstNear" ), TargetCam.nearClipPlane );
            _copyMaterial.SetFloat( Shader.PropertyToID( "_DstFar" ), TargetCam.farClipPlane );
        }
    }
}