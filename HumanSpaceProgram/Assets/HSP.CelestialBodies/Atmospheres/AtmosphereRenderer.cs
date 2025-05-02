using HSP.CelestialBodies.Atmospheres;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityPlus.Serialization;

namespace HSP.CelestialBodies
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [RequireComponent( typeof( Camera ) )]
    public class AtmosphereRenderer : SingletonMonoBehaviour<AtmosphereRenderer>
    {
        Camera _camera;
        CommandBuffer _cmdAtmospheres;
        CommandBuffer _cmdComposition;

        [SerializeField]
        new public Light light { get; set; }

        [SerializeField]
        RenderTexture _rt;

        public Func<RenderTexture> ColorRenderTextureGetter { get; set; }
        public Func<RenderTexture> DepthRenderTextureGetter { get; set; }

        void Awake()
        {
            _camera = this.GetComponent<Camera>();

            _cmdAtmospheres = new CommandBuffer()
            {
                name = "HSP - Atmospheres - Render"
            };
            _cmdComposition = new CommandBuffer()
            {
                name = "HSP - Atmospheres - Composition"
            };
        }

        void OnDestroy()
        {
            if( _rt != null )
                RenderTexture.ReleaseTemporary( _rt );

            _cmdAtmospheres?.Release();
            _cmdComposition?.Release();
        }

        void Update()
        {
            _camera.depthTextureMode &= ~DepthTextureMode.Depth; // NO DEPTH. this breaks (for now?).
        }

        void OnPreRender()
        {
            if( Atmosphere._activeAtmospheres.Count == 0 )
                return;

            this._rt = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );

            foreach( var atmosphere in Atmosphere._activeAtmospheres )
            {
                atmosphere.UpdateMaterialValues( ColorRenderTextureGetter, DepthRenderTextureGetter, light );
            }

            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );

            UpdateCommandBuffers();
            this._camera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdAtmospheres );
            this._camera.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _cmdComposition );
        }

        void OnPostRender()
        {
            // if doesn't work, move to the front of onprecull. Here it should allow others to reuse these textures.
            if( _rt != null )
            {
                RenderTexture.ReleaseTemporary( _rt );
                _rt = null;
            }
        }

        public void UpdateCommandBuffers()
        {
            _cmdAtmospheres.Clear();
            _cmdAtmospheres.SetRenderTarget( _rt );

            foreach( var atmosphere in Atmosphere._activeAtmospheres )
            {
                if( atmosphere.material == null )
                    continue;

                _cmdAtmospheres.Blit( null, _rt, atmosphere.material, 0 );
            }

            _cmdComposition.Clear();
            _cmdComposition.Blit( _rt, (RenderTexture)null );
        }

        [MapsInheritingFrom( typeof( AtmosphereRenderer ) )]
        public static SerializationMapping AtmosphereRendererMapping()
        {
            return new MemberwiseSerializationMapping<AtmosphereRenderer>();
        }
    }
}