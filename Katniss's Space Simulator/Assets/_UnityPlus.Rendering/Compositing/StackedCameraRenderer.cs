using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityPlus.Rendering.Compositing
{
    [RequireComponent( typeof( Camera ) )]
    public class StackedCameraRenderer : MonoBehaviour
    {
        // a list of cameras, rendering one after the other.
        // each next camera uses the last written depth.
        // at the end, write to the screen.

        // I wonder if we can just set the later cameras to render after this one to the same buffers, and without clearing anyting. Will the depth stay?
        // - This wouldn't work with differing clipping planes, but could work if they are the same.

        /// <summary>
        /// Set to null at the end of the chain, to draw to the screen.
        /// </summary>
        [field: SerializeField]
        public StackedCameraRenderer Next { get; set; }

        Camera _camera;

        Shader _shader;
        Material _material;

        RenderTexture _colorRT;
        RenderTexture _depthRT;

        CommandBuffer _cmdCopy;

        void Awake()
        {
            this._camera = this.GetComponent<Camera>();
        }

        void OnDestroy()
        {
            if( _material != null )
            {
                Destroy( _material );
            }

            if( _colorRT != null )
                RenderTexture.ReleaseTemporary( _colorRT );
            if( _depthRT != null )
                RenderTexture.ReleaseTemporary( _depthRT );
            this._cmdCopy?.Release();
        }

        void LateUpdate()
        {
            if( this.Next == null )
            {
                return;
            }

            _camera.depthTextureMode |= DepthTextureMode.Depth; // camera needs to generate depth texture.
            //_camera.clearFlags = CameraClearFlags.Nothing;

            if( this._camera.depth >= this.Next._camera.depth )
            {
                Debug.LogWarning( $"Camera rendering order is invalid for stacking. Camera `{this.gameObject.name}` needs to have lower `depth` value than `{this.Next.gameObject.name}`." );
            }
        }

        void OnPreCull()
        {
            if( this.Next == null )
            {
                // Don't stack further.
                // The next's camera target buffers are not set, so it will draw wherever it was gonna draw (likely to the screen).
                // - this supports the final camera drawing to somewhere other than the screen too.
                return;
            }

            // Lazy initialization only when we know for sure we're gonna be stacking.
            if( this._shader == null )
            {
                this._shader = Shader.Find( "Hidden/CopyDepth" );
                this._material = new Material( this._shader );

                this._cmdCopy = new CommandBuffer()
                {
                    name = "Copy Depth Texture"
                };
            }

            this._colorRT = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 0, RenderTextureFormat.ARGB32 );
            this._depthRT = RenderTexture.GetTemporary( Screen.width, Screen.height/*_camera.pixelWidth, _camera.pixelHeight*/, 24, RenderTextureFormat.Depth );

            this._camera.SetTargetBuffers( _colorRT.colorBuffer, _depthRT.depthBuffer );

            this._material.SetTexture( "_InputColor", this._colorRT );
            this._material.SetTexture( "_InputDepth", this._depthRT );
            this._material.SetFloat( Shader.PropertyToID( "_SrcNear" ), _camera.nearClipPlane ); // shader supports varying clip planes.
            this._material.SetFloat( Shader.PropertyToID( "_SrcFar" ), _camera.farClipPlane );
            this._material.SetFloat( Shader.PropertyToID( "_DstNear" ), this.Next._camera.nearClipPlane );
            this._material.SetFloat( Shader.PropertyToID( "_DstFar" ), this.Next._camera.farClipPlane );

            // Next's camera uses this' commandbuffer.
            // Doing it in reverse is annoying because we don't know when to stop. And it's annoying for the user to set up as well.
            this.Next._camera.RemoveCommandBuffer( CameraEvent.BeforeForwardOpaque, _cmdCopy );
            UpdateCommandBuffer();
            this.Next._camera.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, _cmdCopy );
        }

        void OnPostRender()
        {
            // if doesn't work, move to the front of onprecull. Here it should allow others to reuse these textures.
            if( _colorRT != null )
            {
                RenderTexture.ReleaseTemporary( _colorRT );
                _colorRT = null;
            }
            if( _depthRT != null )
            {
                RenderTexture.ReleaseTemporary( _depthRT );
                _depthRT = null;
            }
        }

        private void UpdateCommandBuffer()
        {
            _cmdCopy.Clear();
            // Blit to the next's texture.
            if( this.Next.Next != null )
            {
                _cmdCopy.SetRenderTarget( Next._colorRT, Next._depthRT );
            }
            _cmdCopy.Blit( null, BuiltinRenderTextureType.CurrentActive, _material, 0 );
        }
    }
}