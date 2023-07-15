using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityPlus.Compositing
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class Compositor : MonoBehaviour
    {
        [Serializable]
        public struct Pass
        {
            [field: SerializeField]
            public CompositeRenderer Renderer { get; set; }
            [field: SerializeField]
            public Material CopyMaterial { get; set; }
        }

        // cameras render to render textures, and do it in order.
        // compositor combines these textures and renders them to the screen.

        // another component should probably run between the camera passes and copy the depth buffer between the RTs.
        [field: SerializeField]
        public Pass[] Passes { get; set; }

        [field: SerializeField]
        RenderTexture _rtOutput;

        private void Awake()
        {
            _rtOutput = new RenderTexture( Screen.currentResolution.width, Screen.currentResolution.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt );
            _rtOutput.Create();
        }

        void OnEnable()
        {
            Camera.onPostRender += OnPostRenderCallback;
        }

        void OnDisable()
        {
            Camera.onPostRender -= OnPostRenderCallback;
        }

        int _camerasRendered;

        void OnPostRenderCallback( Camera camera )
        {
            _camerasRendered++;

            if( _camerasRendered == Camera.allCamerasCount )
            {
                _camerasRendered = 0;

                OnPostRenderAll();
            }
        }

        void OnPostRenderAll()
        {
#warning TODo - doesn't work.
            foreach( var c in Passes )
            {
                Graphics.Blit( c.Renderer.texture, _rtOutput, c.CopyMaterial );
            }

            Graphics.Blit( _rtOutput, (RenderTexture)null );
        }
    }
}