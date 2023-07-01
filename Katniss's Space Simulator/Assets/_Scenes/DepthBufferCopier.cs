using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class DepthBufferCopier : MonoBehaviour
    {
        [SerializeField] RenderTexture depthBufferRT;
        [SerializeField] Material compositeMat;

        private void OnRenderImage( RenderTexture source, RenderTexture destination )
        {
            Graphics.SetRenderTarget( depthBufferRT );
            GL.Clear( false, true, Color.clear );
            Graphics.SetRenderTarget( null );

            //VFXCamera.SetTargetBuffers( depthBufferRT.colorBuffer, source.depthBuffer );
            //VFXCamera.Render();

            // presumably you have to composite the vfx cam's output back into the main image?
            // and presumably you've already assigned the VFXRenderTarget as a texture for the composite material

            Graphics.Blit( source, destination, compositeMat );
        }
    }
}
