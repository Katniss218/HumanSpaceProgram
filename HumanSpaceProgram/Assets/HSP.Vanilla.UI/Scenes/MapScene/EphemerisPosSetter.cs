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
            this._lineRenderer.raycastTarget = false;
        }

        Vector2[] _screenSpacePoints;

        void FixedUpdate()
        {
            var sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();

            Vector2 HalfRes = new Vector2( Screen.width * 0.5f, Screen.height * 0.5f );
            for( int i = 0; i < _points.Length; i++ )
            {
                var point = _points[i];
                var pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( point );
                _screenSpacePoints[i] = (Vector2)camera.WorldToScreenPoint( pos ) - HalfRes;
            }
            _lineRenderer.Points = _screenSpacePoints;
#warning TODO - this needs to go to a canvas with 'constant pixel size' scaling mode. else the positions are wrong.
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            //ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, null, _origin );
            //ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, null, QuaternionDbl.identity );
        }
    }
}