using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class AtmosphereRendererTest : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ImageEffectOpaque]
    void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        if( mat == null )
        {
            Graphics.Blit( source, destination );
            return;
        }

        Graphics.Blit( source, destination, mat );
    }
}
