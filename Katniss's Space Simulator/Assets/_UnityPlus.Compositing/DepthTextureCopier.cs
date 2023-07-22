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
    public class DepthTextureCopier : MonoBehaviour
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
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }
            if( CameraRenderTexture == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( CameraRenderTexture )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }
            if( ColorDepthTexture == null )
            { 
                Debug.LogWarning( $"You need to assign the '{nameof( ColorDepthTexture )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }

            _camera.targetTexture = CameraRenderTexture;

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

            if( CameraRenderTexture != null && _camera.targetTexture != null )
            {
                Graphics.Blit( CameraRenderTexture, ColorDepthTexture, _mat );
            }
        }
    }
}