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
    public class CompositeRenderer : MonoBehaviour
    {
        [field: SerializeField]
        public Camera Camera { get; private set; }

        [field: SerializeField]
        public RenderTexture sourceTexture;

        [field: SerializeField]
        public Texture2D source2;

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

            Camera.SetTargetBuffers( texture.colorBuffer, texture.depthBuffer ); // just doing Camera.targetTexture isn't enough.
           
            _mat = new Material( Shader );
            if( _mat != null )
            {
                CommandBuffer c = new CommandBuffer();
                c.name = "Composite Depth Merge";
                //c.SetGlobalTexture( "_garbage", source2 );
                c.Blit( null, texture, _mat, 0 );
                Camera.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, c ); // this does run the shader, it works, but doesn't read the texture correctly.
            }
        }
         
        void Start()
        {
            if( Shader == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }

            //_mat = new Material( Shader );
            //_mat.SetTexture( "_garbage", sourceTexture );
        }

        void OnPreRender()
        {
            if( _mat != null )
            {
                _mat.SetTexture( "_garbage", sourceTexture );
                //Graphics.Blit( null, texture, _mat );
            }
        }
    }
}