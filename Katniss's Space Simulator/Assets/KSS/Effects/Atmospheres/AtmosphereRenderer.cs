using KSS.Core.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KSS.CelestialBodies
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public class AtmosphereRenderer : MonoBehaviour
    {
        Shader _shader;

        [SerializeField]
        Material _material;

        Camera _camera;
        CommandBuffer _cmdAtmospheres;
        CommandBuffer _cmdComposition;

        Vector3 _center = Vector3.zero;

        [SerializeField]
        RenderTexture _rt;

        void OnReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            Vector3Dbl oldAirfPos = data.OldFrame.TransformPosition( _center );
            _center = data.NewFrame.InverseTransformPosition( oldAirfPos );
        }

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            _shader = Shader.Find( "Hidden/Atmosphere" );
            _material = new Material( _shader );

            _rt = new RenderTexture( _camera.pixelWidth, _camera.pixelHeight, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None );

            _cmdAtmospheres = new CommandBuffer()
            {
                name = "Atmospheres - Render"
            };
            _cmdComposition = new CommandBuffer()
            {
                name = "Atmospheres - Composition"
            };

            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch += OnReferenceFrameSwitch;
        }

        private void Update()
        {
            _camera.depthTextureMode &= ~DepthTextureMode.Depth; // NO DEPTH. this breaks (for now?).
        }

        void OnDestroy()
        {
            SceneReferenceFrameManager.OnAfterReferenceFrameSwitch -= OnReferenceFrameSwitch;
            _cmdAtmospheres?.Release();
            _cmdComposition?.Release();
        }

        void OnPreRender()
        {
            _material.SetVector( Shader.PropertyToID( "_Center" ), _center );
            _material.SetVector( Shader.PropertyToID( "_SunDirection" ), new Vector3( 1, 0, 1 ) );
            _material.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
            _material.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
            _material.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
            _material.SetFloat( Shader.PropertyToID( "_MinRadius" ), 6371000f );
            _material.SetFloat( Shader.PropertyToID( "_MaxRadius" ), 6500000f );
            _material.SetFloat( Shader.PropertyToID( "_InScatteringPointCount" ), 16 );
            _material.SetFloat( Shader.PropertyToID( "_OpticalDepthPointCount" ), 8 );
            _material.SetFloat( Shader.PropertyToID( "_DensityFalloffPower" ), 13.7f );

            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );
            UpdateCommandBuffers();
            this._camera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );
        }

        private void UpdateCommandBuffers()
        {
            _cmdAtmospheres.Clear();
            _cmdAtmospheres.SetGlobalTexture( Shader.PropertyToID( "_texgsfs" ), BuiltinRenderTextureType.CurrentActive );
            _cmdAtmospheres.SetGlobalTexture( Shader.PropertyToID( "_CameraDepthTexture" ), BuiltinRenderTextureType.Depth );
            _cmdAtmospheres.SetRenderTarget( _rt );
            _cmdAtmospheres.Blit( null, _rt, _material, 0 );

            _cmdComposition.Clear();
            _cmdComposition.Blit( _rt, BuiltinRenderTextureType.CurrentActive );
        }
    }
}