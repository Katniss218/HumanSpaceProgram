using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityPlus.Compositing
{
    /// <summary>
    /// Copies the depth texture from a RenderTexture to a separate texture.
    /// </summary>
    [RequireComponent( typeof( Camera ) )]
    //[ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class DepthTextureExtractor : MonoBehaviour
    {
        [field: SerializeField]
        public Shader Shader { get; set; }

        [field: SerializeField]
        public RenderTexture CameraRenderTexture { get; set; }

        [field: SerializeField]
        public RenderTexture ColorDepthTexture { get; set; }

        [field: SerializeField]
        public DepthTextureApplier RendererWithTheCommandbuffer { get; private set; }

        Camera _camera;
        Material _mat;

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            if( Shader == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureExtractor )} '{this.gameObject.name}'." );
                return;
            }

            CameraRenderTexture = new RenderTexture( _camera.pixelWidth, _camera.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt );
            CameraRenderTexture.Create();

            ColorDepthTexture = new RenderTexture( _camera.pixelWidth, _camera.pixelHeight, GraphicsFormat.R32_SFloat, GraphicsFormat.None );
            ColorDepthTexture.Create();

            _camera.SetTargetBuffers( CameraRenderTexture.colorBuffer, CameraRenderTexture.depthBuffer );

            if( RendererWithTheCommandbuffer != null )
            {
                RendererWithTheCommandbuffer.colorDepthTexture = ColorDepthTexture;
            }
        }

        void OnPostRender()
        {
            if( _mat == null )
            {
                _mat = new Material( Shader );
            }

            if( CameraRenderTexture != null ) // copy the depth to a texture to await copy. This needs to be done under this camera, otherwise the projection variables are wrong.
            {
                Graphics.Blit( CameraRenderTexture, ColorDepthTexture, _mat );
            }
        }
    }
}