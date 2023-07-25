// Experimental implementation of contact shadows for Unity
// https://github.com/keijiro/ContactShadows

using UnityEngine;
using UnityEngine.Rendering;

namespace PostEffects
{
    [ExecuteInEditMode]
    [RequireComponent( typeof( Camera ) )]
    public sealed class ContactShadows : MonoBehaviour
    {
        [SerializeField] Light _light;
        [SerializeField, Range( 0, 5 )] float _maxDepthDifference = 0.5f;
        [SerializeField, Range( 4, 32 )] int _sampleCount = 16;
        [SerializeField, Range( 0, 1 )] float _temporalFilter = 0.5f;
        [SerializeField] bool _downsample = false;


        [SerializeField, HideInInspector] Shader _shader;
        [SerializeField, HideInInspector] NoiseTextureSet _noiseTextures;

        Material _material;
        RenderTexture _prevMaskRT1;
        RenderTexture _prevMaskRT2;
        CommandBuffer _cmdShadows;
        CommandBuffer _cmdComposition;

        // We track the VP matrix without using previousViewProjectionMatrix
        // because it's not available for use in OnPreCull.
        Matrix4x4 _previousVP = Matrix4x4.identity;

        void OnDestroy()
        {
            // Release temporary objects.
            if( _material != null )
            {
                if( Application.isPlaying )
                    Destroy( _material );
                else
                    DestroyImmediate( _material );
            }

            if( _prevMaskRT1 != null )
                RenderTexture.ReleaseTemporary( _prevMaskRT1 );
            if( _prevMaskRT2 != null )
                RenderTexture.ReleaseTemporary( _prevMaskRT2 );

            if( _cmdShadows != null )
                _cmdShadows.Release();
            if( _cmdComposition != null )
                _cmdComposition.Release();
        }

        void OnPreCull()
        {
            // Update the temporary objects and build the command buffers for
            // the target light.

            UpdateTempObjects();

            if( _light != null )
            {
                BuildUpdatedCommandBuffer();
                _light.AddCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdShadows );
                _light.AddCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdComposition );
            }
        }

        void OnPreRender()
        {
            // We can remove the command buffer before starting render in this
            // camera. Actually this should be done in OnPostRender, but it
            // crashes for some reasons. So, we do this in OnPreRender instead.

            if( _light != null )
            {
                _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdShadows );
                _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _cmdComposition );
                _cmdShadows.Clear();
                _cmdComposition.Clear();
            }
        }

        void Update()
        {
            // We require the camera depth texture.
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        // Calculates the view-projection matrix for GPU use.
        static Matrix4x4 CalculateVPMatrix()
        {
            var cam = Camera.current;
            var p = cam.nonJitteredProjectionMatrix;
            var v = cam.worldToCameraMatrix;
            return GL.GetGPUProjectionMatrix( p, true ) * v;
        }

        // Get the screen dimensions.
        Vector2Int GetScreenSize()
        {
            var cam = Camera.current;
            var div = _downsample ? 2 : 1;
            return new Vector2Int( cam.pixelWidth / div, cam.pixelHeight / div );
        }

        // Update the temporary objects for the current frame.
        void UpdateTempObjects()
        {
            if( _prevMaskRT2 != null )
            {
                RenderTexture.ReleaseTemporary( _prevMaskRT2 );
                _prevMaskRT2 = null;
            }

            // Do nothing below if the target light is not set.
            if( _light == null )
                return;

            // Lazy initialization of temporary objects.
            if( _material == null )
            {
                _material = new Material( _shader );
                _material.hideFlags = HideFlags.DontSave;
            }

            if( _cmdShadows == null )
            {
                _cmdShadows = new CommandBuffer()
                {
                    name = "Contact Shadows Ray Tracing"
                };
                _cmdComposition = new CommandBuffer()
                {
                    name = "Contact Shadows Temporal Filter"
                };
            }
            else
            {
                _cmdShadows.Clear();
                _cmdComposition.Clear();
            }

            // Update the common shader parameters.
            _material.SetFloat( "_MaxDepthDifference", _maxDepthDifference );
            _material.SetInt( "_SampleCount", _sampleCount );

            float convergence = Mathf.Pow( 1 - _temporalFilter, 2 );
            _material.SetFloat( "_Convergence", convergence );

            // Calculate the light vector in the view space.
            _material.SetVector( "_LightVector",
                transform.InverseTransformDirection( -_light.transform.forward )
               /* * _light.shadowBias / (_sampleCount - 1.5f) */
            );

            // Noise texture and its scale factor
            Texture2D noiseTexture = _noiseTextures.GetTexture();
            Vector2 noiseScale = (Vector2)GetScreenSize() / noiseTexture.width;
            _material.SetVector( "_NoiseScale", noiseScale );
            _material.SetTexture( "_NoiseTex", noiseTexture );

            // "Reproject into the previous view" matrix
            _material.SetMatrix( "_Reprojection", _previousVP * transform.localToWorldMatrix );
            _previousVP = CalculateVPMatrix();
        }

        /// <summary>
        /// Build the command buffer for the current frame.
        /// </summary>
        void BuildUpdatedCommandBuffer()
        {
            // Allocate the temporary shadow mask RT.
            Vector2Int maskSize = GetScreenSize();
            RenderTextureFormat maskFormat = RenderTextureFormat.R8;
            RenderTexture tempMaskRT = RenderTexture.GetTemporary( maskSize.x, maskSize.y, 0, maskFormat );

            if( _temporalFilter == 0 ) // no temporal smoothing
            {
                // Do raytracing and output to the temporary shadow mask RT.
                _cmdShadows.SetGlobalTexture( Shader.PropertyToID( "_ShadowMask" ), BuiltinRenderTextureType.CurrentActive );
                _cmdShadows.SetRenderTarget( tempMaskRT );
                _cmdShadows.DrawProcedural( Matrix4x4.identity, _material, 0, MeshTopology.Triangles, 3 );
            }
            else
            {
                // Do raytracing and output to the unfiltered mask RT.
                int unfilteredMaskID = Shader.PropertyToID( "_UnfilteredMask" );
                _cmdShadows.SetGlobalTexture( Shader.PropertyToID( "_ShadowMask" ), BuiltinRenderTextureType.CurrentActive );
                _cmdShadows.GetTemporaryRT( unfilteredMaskID, maskSize.x, maskSize.y, 0, FilterMode.Point, maskFormat );
                _cmdShadows.SetRenderTarget( unfilteredMaskID );
                _cmdShadows.DrawProcedural( Matrix4x4.identity, _material, 0, MeshTopology.Triangles, 3 );

                // Apply the temporal filter and output to the temporary shadow mask RT.
                _cmdShadows.SetGlobalTexture( Shader.PropertyToID( "_PrevMask" ), _prevMaskRT1 );
                _cmdShadows.SetRenderTarget( tempMaskRT );
                _cmdShadows.DrawProcedural( Matrix4x4.identity, _material, 1 + (Time.frameCount & 1), MeshTopology.Triangles, 3 );
            }

            if( _downsample )
            {
                // Downsample enabled: Use upsampler for the composition.
                _cmdComposition.SetGlobalTexture( Shader.PropertyToID( "_TempMask" ), tempMaskRT );
                _cmdComposition.DrawProcedural( Matrix4x4.identity, _material, 3, MeshTopology.Triangles, 3 );
            }
            else
            {
                // No downsample: Use simple blit.
                _cmdComposition.Blit( tempMaskRT, BuiltinRenderTextureType.CurrentActive );
            }

            // Update the filter history.
            _prevMaskRT2 = _prevMaskRT1;
            _prevMaskRT1 = tempMaskRT;
        }
    }
}