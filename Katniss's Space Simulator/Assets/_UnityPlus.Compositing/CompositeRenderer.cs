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
    public class CompositeRenderer : MonoBehaviour
    {
        Camera _camera;

        [field: SerializeField]
        public RenderTexture RenderTexture;

#warning TODO - use Compositor's RT???

        void Awake()
        {
            _camera = this.GetComponent<Camera>();
            RenderTexture = new RenderTexture( Screen.currentResolution.width, Screen.currentResolution.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt );
            RenderTexture.Create();
            _camera.targetTexture = RenderTexture;
        }

        public bool UsesCamera( Camera cam )
        {
            return _camera == cam;
        }
    }
}