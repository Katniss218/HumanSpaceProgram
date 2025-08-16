using HSP.ReferenceFrames;
using HSP.Time;
using HSP.UI;
using HSP.Vanilla.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    [RequireComponent( typeof( UILineRenderer ) )]
    public class EphemerisPosSetter : MonoBehaviour, IReferenceFrameSwitchResponder
    {
        private ISceneReferenceFrameProvider _sceneReferenceFrameProvider;
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider
        {
            get => _sceneReferenceFrameProvider;
            set
            {
                if( _sceneReferenceFrameProvider == value )
                    return;

                _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
                _sceneReferenceFrameProvider = value;
                _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            }
        }

        Vector3Dbl _origin;
        Vector3Dbl _lastPos;
        QuaternionDbl _lastRot;
        Transform _referenceTransform;
        Vector3Dbl[] _points;
        int _prevPointCount = -1;
        public new Camera camera;
        UILineRenderer _lineRenderer;

        public IEnumerable<Vector3Dbl> Points
        {
            get => _points.Cast<Vector3Dbl>();
            set
            {
                if( value == null )
                {

                    return;
                }
                _points = value.ToArray();
                _screenSpacePoints = new Vector2[_points.Length];
            }
        }

        void Awake()
        {
            this._lineRenderer = GetComponent<UILineRenderer>();
            this._lineRenderer.Thickness = 3;
        }

        Vector2[] _screenSpacePoints;

        void FixedUpdate()
        {
            var sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT );

            Vector2 HalfRes = new Vector2( Screen.width * 0.5f, Screen.height * 0.5f );
            for( int i = 0; i < _points.Length; i++ )
            {
                var point = _points[i];
                var pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( point );
                _screenSpacePoints[i] = (Vector2)camera.WorldToScreenPoint( pos ) - HalfRes;
            }
            _lineRenderer.Points = _screenSpacePoints;
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            //ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, null, _origin );
            //ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, null, QuaternionDbl.identity );
        }
    }
}