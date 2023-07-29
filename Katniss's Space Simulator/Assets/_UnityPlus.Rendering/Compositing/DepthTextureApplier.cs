using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityPlus.Compositing
{
    [RequireComponent( typeof( Camera ) )]
    //[ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class DepthTextureApplier : MonoBehaviour
    {
        [field: SerializeField]
        public Shader Shader { get; set; }

        public RenderTexture colorDepthTexture;
        public RenderTexture targetTexture;

        Camera _camera;
        Material _mat;

        void Awake()
        {
            _camera = this.GetComponent<Camera>();
        }

        void Start()
        {
            if( Shader == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureExtractor )} '{this.gameObject.name}'." );
                return;
            }

            targetTexture = new RenderTexture( _camera.pixelWidth, _camera.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt );
            targetTexture.Create();

            if( _camera.depthTextureMode == DepthTextureMode.None )
                _camera.depthTextureMode = DepthTextureMode.Depth;

            _camera.SetTargetBuffers( targetTexture.colorBuffer, targetTexture.depthBuffer ); // just doing Camera.targetTexture isn't enough.

            // Create material first.
            _mat = new Material( Shader );
            _mat.SetTexture( "_garbage", colorDepthTexture );

            // Apply the commandbuffer that uses the material.
            CommandBuffer depthApplyBuffer = new CommandBuffer()
            {
                name = "Depth Texture Apply"
            };
            depthApplyBuffer.Blit( null, targetTexture, _mat, 0 );
            _camera.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, depthApplyBuffer );
        }

        void OnPostRender()
        {
            Graphics.Blit( targetTexture, (RenderTexture)null );
        }
    }
}