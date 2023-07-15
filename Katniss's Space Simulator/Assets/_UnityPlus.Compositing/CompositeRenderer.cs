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
        [field: SerializeField]
        public Camera Camera { get; private set; }

        [field: SerializeField]
        public RenderTexture texture;

#warning TODO - use Compositor's RT???

        void Awake()
        {
            Camera = this.GetComponent<Camera>();
            texture = new RenderTexture( Screen.currentResolution.width, Screen.currentResolution.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt );
            texture.Create();
            Camera.targetTexture = texture;
        }
    }
}