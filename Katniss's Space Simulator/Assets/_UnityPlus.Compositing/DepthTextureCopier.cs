using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityPlus.Compositing
{
    [RequireComponent( typeof( Camera ) )]
    //[ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class DepthTextureCopier : MonoBehaviour
    {
        [field: SerializeField]
        public RenderTexture SourceRenderTexture { get; set; }

        [field: SerializeField]
        public RenderTexture TargetRenderTexture { get; set; }

        [field: SerializeField]
        public float TargetNearPlane { get; set; } = 0;
        [field: SerializeField]
        public float TargetFarPlane { get; set; } = 1;

        [field: SerializeField]
        public Shader Shader { get; set; }

        [field: SerializeField]
        Material _mat;

        Camera _camera;


        [field: SerializeField]
        public CompositeRenderer RendererWithTheCommandbuffer { get; private set; }

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            if( Shader == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }
            if( SourceRenderTexture == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( SourceRenderTexture )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }
            if( TargetRenderTexture != null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( TargetRenderTexture )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }

            _camera.targetTexture = SourceRenderTexture;

            _mat = new Material( Shader );
            _mat.SetFloat( Shader.PropertyToID( "_InputMin" ), _camera.nearClipPlane );
            _mat.SetFloat( Shader.PropertyToID( "_InputMax" ), _camera.farClipPlane );
            _mat.SetFloat( Shader.PropertyToID( "_OutputMin" ), TargetNearPlane );
            _mat.SetFloat( Shader.PropertyToID( "_OutputMax" ), TargetFarPlane );

            if( RendererWithTheCommandbuffer != null )
            {
                RendererWithTheCommandbuffer.sourceTexture = TargetRenderTexture;
            }
        }

        void Start()
        {

        }

        void OnPreRender()
        {
            if( SourceRenderTexture != null && _mat != null )
            {
                //_camera.targetTexture = SourceRenderTexture;
            }
        }

        void OnPostRender()
        {
            if( SourceRenderTexture != null && _mat != null && _camera.targetTexture != null )
            {
                Graphics.Blit( SourceRenderTexture, TargetRenderTexture, _mat );
                //_camera.targetTexture = null;
            }
        }
    }
}
