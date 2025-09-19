//using HSP.ReferenceFrames;
//using HSP.UI;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace HSP.Vanilla.UI.Scenes.MapScene
//{
//    [RequireComponent( typeof( UILineRenderer ) )]
//    public class EphemerisPosSetter : MonoBehaviour, IReferenceFrameSwitchResponder
//    {
//        private ISceneReferenceFrameProvider _sceneReferenceFrameProvider;
//        public ISceneReferenceFrameProvider SceneReferenceFrameProvider
//        {
//            get => _sceneReferenceFrameProvider;
//            set
//            {
//                if( _sceneReferenceFrameProvider == value )
//                    return;

//                _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
//                _sceneReferenceFrameProvider = value;
//                _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
//            }
//        }

//        public new Camera camera;
//        UILineRenderer _lineRenderer;

//        public List<Vector2> Points
//        {
//            set
//            {
//                if( value == null )
//                {
//                    return;
//                }

//                Vector2 HalfRes = new Vector2( Screen.width * 0.5f, Screen.height * 0.5f );
//                _screenSpacePoints = new Vector2[value.Count];
//                for( int i = 0; i < _screenSpacePoints.Length; i++ )
//                {
//                    _screenSpacePoints[i] = value[i] - HalfRes;
//                }
//            }
//        }

//        void Awake()
//        {
//            this._lineRenderer = GetComponent<UILineRenderer>();
//            this._lineRenderer.Thickness = 3;
//            this._lineRenderer.raycastTarget = false;
//        }

//        Vector2[] _screenSpacePoints;

//        void LateUpdate()
//        {
//            _lineRenderer.Points = _screenSpacePoints;
//#warning TODO - this needs to go to a canvas with 'constant pixel size' scaling mode. else the positions are wrong.
//        }

//        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
//        {
//            //ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, null, _origin );
//            //ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, null, QuaternionDbl.identity );
//        }
//    }
//}