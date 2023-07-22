using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityPlus.Compositing
{
    public class DepthAndColorCopierOnePass : MonoBehaviour
    {
        [SerializeField]
        Camera _sourceCam;

        [SerializeField]
        Camera _destinationCam;

        [SerializeField]
        Shader _copyShader;

        [SerializeField]
        Material _copyMaterial;

        [SerializeField]
        RenderTexture srcColorRT;

        [SerializeField]
        RenderTexture srcDepthRT;

        [SerializeField]
        RenderTexture dstColorRT;

        [SerializeField]
        RenderTexture dstDepthRT;

        void Start()
        {
            srcColorRT = new RenderTexture( _sourceCam.pixelWidth, _sourceCam.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None );
            srcColorRT.Create();
            srcDepthRT = new RenderTexture( _sourceCam.pixelWidth, _sourceCam.pixelHeight, GraphicsFormat.None, GraphicsFormat.D32_SFloat_S8_UInt );
            srcDepthRT.Create();

            dstColorRT = new RenderTexture( _destinationCam.pixelWidth, _destinationCam.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None );
            dstColorRT.Create();
            dstDepthRT = new RenderTexture( _destinationCam.pixelWidth, _destinationCam.pixelHeight, GraphicsFormat.None, GraphicsFormat.D32_SFloat_S8_UInt );
            dstDepthRT.Create();

            if( _destinationCam.depthTextureMode == DepthTextureMode.None )
                _destinationCam.depthTextureMode = DepthTextureMode.Depth;

            _sourceCam.SetTargetBuffers( srcColorRT.colorBuffer, srcDepthRT.depthBuffer );
            _destinationCam.SetTargetBuffers( dstColorRT.colorBuffer, dstDepthRT.depthBuffer );

            _copyMaterial = new Material( _copyShader );

            _copyMaterial.SetTexture( "_InputColor", srcColorRT );
            _copyMaterial.SetTexture( "_InputDepth", srcDepthRT );

            CommandBuffer cmd = new CommandBuffer()
            {
                name = "Copy Depth And Color"
            };
            cmd.SetRenderTarget( dstColorRT, dstDepthRT );
            cmd.Blit( null, BuiltinRenderTextureType.CurrentActive, _copyMaterial, 0 );

            _destinationCam.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, cmd );
        }

        void LateUpdate()
        {
            _copyMaterial.SetFloat( Shader.PropertyToID( "_SrcNear" ), _sourceCam.nearClipPlane );
            _copyMaterial.SetFloat( Shader.PropertyToID( "_SrcFar" ), _sourceCam.farClipPlane );

            _copyMaterial.SetFloat( Shader.PropertyToID( "_DstNear" ), _destinationCam.nearClipPlane );
            _copyMaterial.SetFloat( Shader.PropertyToID( "_DstFar" ), _destinationCam.farClipPlane );
        }
    }
}