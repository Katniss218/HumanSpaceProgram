using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus.Rendering.ContactShadows
{
    [RequireComponent( typeof( Camera ) )]
    public class ContactShadowsRenderer : MonoBehaviour
    {
        [field: SerializeField]
        Light _light;

        public Light Light
        {
            get => _light;
            set
            {
                // need to remove here since after the value changes, it would leak the buffer.
                _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdShadows ); // hopefully this won't throw if buffer is not added yet.
                _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdComposition );
                _light = value;
            }
        }

        [field: SerializeField]
        public float ShadowDistance { get; set; } = 500;

        [field: SerializeField]
        public float Thickness { get; set; } = 0.3f; // Treats all screen space objects as this thick.

        [field: SerializeField]
        public float RayLength { get; set; } = 1f; // Maximum distance that a ray can travel.

        [field: SerializeField]
        public float Bias { get; set; } = 0.04f; // Subtract this much from the pixel's depth (linear, meters).

        [field: SerializeField]
        public int SampleCount { get; set; } = 10; // Number of samples per ray.

        Camera _camera;

        // Command buffers are for the light (!!!) not the camera
        CommandBuffer _cmdShadows;
        CommandBuffer _cmdComposition;

        [SerializeField]
        RenderTexture _shadowRT;

        [SerializeField]
        Shader _shader;
        [SerializeField]
        Material _material;

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            _shader = Shader.Find( "Hidden/ContactShadows" );
            _material = new Material( _shader );

            _cmdShadows = new CommandBuffer()
            {
                name = "Contact Shadows - Render"
            };
            _cmdComposition = new CommandBuffer()
            {
                name = "Contact Shadows - Composition"
            };
        }

        void OnDestroy()
        {
            if( _material != null )
            {
                if( Application.isPlaying )
                    Destroy( _material );
                else
                    DestroyImmediate( _material );
            }

            if( _shadowRT != null )
                RenderTexture.ReleaseTemporary( _shadowRT );

            _cmdShadows?.Release();
            _cmdComposition?.Release();
        }

        void Update()
        {
            _camera.depthTextureMode |= DepthTextureMode.Depth; // required.
        }

        void OnPreCull()
        {
            if( _shadowRT != null )
            {
                RenderTexture.ReleaseTemporary( _shadowRT );
                _shadowRT = null;
            }

            if( _light == null )
            {
                return;
            }

            _shadowRT = RenderTexture.GetTemporary( _camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.R8 );

            _material.SetVector( Shader.PropertyToID( "_LightDir" ), this.transform.InverseTransformDirection( -this._light.transform.forward ) );

            _material.SetFloat( Shader.PropertyToID( "_ShadowStrength" ), this._light.shadowStrength );
            _material.SetFloat( Shader.PropertyToID( "_ShadowDistance" ), this.ShadowDistance );
            _material.SetFloat( Shader.PropertyToID( "_RayLength" ), this.RayLength );
            _material.SetFloat( Shader.PropertyToID( "_Thickness" ), this.Thickness );
            _material.SetFloat( Shader.PropertyToID( "_Bias" ), this.Bias );
            _material.SetInteger( Shader.PropertyToID( "_SampleCount" ), this.SampleCount );

            _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdShadows ); // hopefully this won't throw if buffer is not added yet.
            _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdComposition );
            UpdateRenderCommandBuffer();
            UpdateCompositeCommandBuffer();
            // light command buffer will be called on every camera that sees that light?
            _light.AddCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdShadows );
            _light.AddCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdComposition );

        }

        void OnDisable()
        {
            _light?.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdShadows ); // hopefully this won't throw if buffer is not added yet.
            _light?.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdComposition );
        }

        void UpdateRenderCommandBuffer()
        {
            _cmdShadows.Clear();
            _cmdShadows.SetGlobalTexture( Shader.PropertyToID( "_ShadowMask" ), BuiltinRenderTextureType.CurrentActive ); // shadow mask?
            _cmdShadows.SetRenderTarget( _shadowRT );
            _cmdShadows.Blit( null, _shadowRT, _material, 0 );
        }

        void UpdateCompositeCommandBuffer()
        {
            _cmdComposition.Clear();
            _cmdComposition.Blit( _shadowRT, BuiltinRenderTextureType.CurrentActive );
        }
    }
}