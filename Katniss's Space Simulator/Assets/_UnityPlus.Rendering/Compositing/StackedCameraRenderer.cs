using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Rendering.Compositing
{
    [RequireComponent( typeof( Camera ) )]
    public class StackedCameraRenderer : MonoBehaviour
    {
        // a list of cameras, rendering one after the other.
        // each next camera uses the last written depth.
        // at the end, write to the screen.


        [field: SerializeField]
        public StackedCameraRenderer Next { get; set; }

        Shader _shader;
        Material _material;



        void Awake()
        {
            _shader = Shader.Find( "Hidden/CopyDepth" );
        }

        void Start()
        {
            // assuming everything is the same resolution:

            // 1st camera runs, with target1.
            // We copy target1 to target2. It gets cleared in doing so.
            // 2nd camera runs, with target2.
            // We copy target2 to target1. It gets cleared in doing so.
            // 3rd camera runs, with target1.
            // and so on...



            // assuming everything can have different resolution:
            // 1st camera runs, with target1.
            // We copy target1 to target2. It gets cleared and resized. Target2 has the resolution at which we want the 2nd camera to render.
            // 2nd camera runs, with target2.
            // We copy target2 to target3. It has the resolution of camera3.
            // 3rd camera runs, with target3.

            // But here's the problem...
            // Because it's sequential, we lose the detail rendered by the previous cameras every time we downscale, and don't gain it back when we upscale.

        }

        private void OnPreRender()
        {
            if( this.Next == null )
            {
                // draw to screen.
            }
        }
    }
}