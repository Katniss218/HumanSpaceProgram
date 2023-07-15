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
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class DepthTextureCopier : MonoBehaviour
    {
        [field: SerializeField]
        public RenderTexture SourceRenderTexture { get; set; }

        [field: SerializeField]
        public RenderTexture TargetRenderTexture { get; set; }

        [field: SerializeField]
        public float TargetNearPlane { get; set; }
        [field: SerializeField]
        public float TargetFarPlane { get; set; }

        [field: SerializeField]
        public Shader Shader { get; set; }

        [field: SerializeField]
        Material _mat;

        Camera _camera;

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            TargetRenderTexture = new RenderTexture( Screen.currentResolution.width, Screen.currentResolution.height, GraphicsFormat.None, GraphicsFormat.D32_SFloat_S8_UInt );
            TargetRenderTexture.Create();
        }

        void Start()
        {
            if( Shader == null )
            {
                Debug.LogWarning( $"You need to assign the '{nameof( Shader )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                return;
            }
            if( SourceRenderTexture == null )
            {
                if( _camera.targetTexture == null )
                {
                    Debug.LogWarning( $"You need to assign the '{nameof( SourceRenderTexture )}' to the {nameof( DepthTextureCopier )} '{this.gameObject.name}'." );
                    return;
                }

                SourceRenderTexture = _camera.targetTexture;
            }

            _mat = new Material( Shader );
            _mat.SetFloat( Shader.PropertyToID( "_InputMin" ), _camera.nearClipPlane );
            _mat.SetFloat( Shader.PropertyToID( "_InputMax" ), _camera.farClipPlane );
            _mat.SetFloat( Shader.PropertyToID( "_OutputMin" ), TargetNearPlane );
            _mat.SetFloat( Shader.PropertyToID( "_OutputMax" ), TargetFarPlane );
        }

        void OnPostRender()
        {
            if( SourceRenderTexture != null && _mat != null )
            {
                Graphics.Blit( SourceRenderTexture, TargetRenderTexture, _mat );
            }
        }
    }
}
