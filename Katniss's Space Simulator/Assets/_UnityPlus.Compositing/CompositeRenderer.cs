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
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class CompositeRenderer : MonoBehaviour
    {
        [field: SerializeField]
        public Camera Camera { get; private set; }

        [field: SerializeField]
        public RenderTexture sourceTexture;

        [field: SerializeField]
        public RenderTexture texture;

        [field: SerializeField]
        public Shader Shader { get; set; }

        [field: SerializeField]
        Material _mat;

#warning TODO - use Compositor's RT???

        void Awake()
        {
            Camera = this.GetComponent<Camera>();
            texture = new RenderTexture( Screen.currentResolution.width, Screen.currentResolution.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt );
            texture.Create();

            if( Camera.depthTextureMode == DepthTextureMode.None )
                Camera.depthTextureMode = DepthTextureMode.Depth;

            Camera.SetTargetBuffers( texture.colorBuffer, texture.depthBuffer );

            CommandBuffer c = new CommandBuffer();
            c.Blit( sourceTexture, texture, _mat ); // last opt parameter 'pass' specifies which pass from the shader to call
            c.name = "Composite Depth Merge";
            Camera.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, c );
        }

        void Start()
        {
            if( Shader == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }

            _mat = new Material( Shader );

        }

        void OnPreRender()
        {
            if( _mat != null )
            {
                Graphics.Blit( sourceTexture, texture, _mat );
            }

#warning TODO - the camera doesn't seem to care about the depth buffer in this texture.
        }
    }
}