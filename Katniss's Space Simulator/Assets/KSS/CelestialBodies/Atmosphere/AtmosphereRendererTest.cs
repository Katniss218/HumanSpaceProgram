using KSS.Core.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent( typeof( Camera ) )]
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class AtmosphereRendererTest : MonoBehaviour
{
    public Shader atmosphereShader;

    [SerializeField]
    Material _mat;

    Camera _camera;

    void OnReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
    {
        int id = Shader.PropertyToID( "_Center" );
        Vector3 oldSceneCenter = _mat.GetVector( id );

        Vector3Dbl oldAirfPos = data.OldFrame.TransformPosition( oldSceneCenter );

        _mat.SetVector( id, data.NewFrame.InverseTransformPosition( oldAirfPos ) );
    }

    void Awake()
    {
        _camera = this.GetComponent<Camera>();
        _mat = new Material( atmosphereShader );
        _mat.SetVector( Shader.PropertyToID( "_Center" ), new Vector3( 0, 0, 0 ) );
        _mat.SetVector( Shader.PropertyToID( "_SunDirection" ), new Vector3( 1, 0, 1 ) );
        _mat.SetVector( Shader.PropertyToID( "_ScatteringWavelengths" ), new Vector3( 675, 530, 400 ) );
        _mat.SetFloat( Shader.PropertyToID( "_ScatteringStrength" ), 128 );
        _mat.SetFloat( Shader.PropertyToID( "_TerminatorFalloff" ), 32 );
        _mat.SetFloat( Shader.PropertyToID( "_MinRadius" ), 6371000f );
        _mat.SetFloat( Shader.PropertyToID( "_MaxRadius" ), 6500000f );
        _mat.SetFloat( Shader.PropertyToID( "_InScatteringPointCount" ), 16 );
        _mat.SetFloat( Shader.PropertyToID( "_OpticalDepthPointCount" ), 8 );
        _mat.SetFloat( Shader.PropertyToID( "_DensityFalloffPower" ), 13.7f );
    }

    private void OnEnable()
    {
        SceneReferenceFrameManager.OnAfterReferenceFrameSwitch += OnReferenceFrameSwitch;
    }

    private void OnDisable()
    {
        SceneReferenceFrameManager.OnAfterReferenceFrameSwitch -= OnReferenceFrameSwitch;
    }

    [ImageEffectOpaque]
    void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        if( _mat == null )
        {
            Graphics.Blit( source, destination );
            return;
        }

        Graphics.Blit( source, destination, _mat );
    }
}
