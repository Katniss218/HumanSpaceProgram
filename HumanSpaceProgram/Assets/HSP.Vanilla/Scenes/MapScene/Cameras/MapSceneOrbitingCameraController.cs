using HSP.Input;
using HSP.ReferenceFrames;
using HSP.Time;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.MapScene.Cameras
{
    [RequireComponent( typeof( IReferenceFrameTransform ) )]
    public class MapSceneOrbitingCameraController : SingletonMonoBehaviour<MapSceneOrbitingCameraController>
    {
        /// <summary>
        /// The camera will focus on this object.
        /// </summary>
        public Transform ReferenceObject { get; set; }

        IReferenceFrameTransform _referenceFrameTransform;

        [SerializeField]
        float _zoomDist = 5;
        public float ZoomDist
        {
            get => _zoomDist;
            set
            {
                _zoomDist = value;
                SyncZoomDist();
            }
        }

        const float MOVE_MULTIPLIER = 3.0f;
        const float ZOOM_MULTIPLIER = 0.15f;

        const float MIN_ZOOM_DISTANCE = 1f;
        const float MAX_ZOOM_DISTANCE = 1e25f;

        bool _isRotating;

        private void UpdateZoomLevel()
        {
            if( UnityEngine.Input.mouseScrollDelta.y > 0 )
                _zoomDist -= _zoomDist * ZOOM_MULTIPLIER;
            else if( UnityEngine.Input.mouseScrollDelta.y < 0 )
                _zoomDist += _zoomDist * ZOOM_MULTIPLIER;

            SyncZoomDist();
        }

        private void SyncZoomDist()
        {
            _zoomDist = Mathf.Clamp( _zoomDist, MIN_ZOOM_DISTANCE, MAX_ZOOM_DISTANCE );

            Vector3 targetPos = (ReferenceObject == null)
                ? (Vector3)_referenceFrameTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT ).InverseTransformPosition( Vector3.zero )
                : ReferenceObject.position;
            this.transform.position = targetPos + (-transform.forward) * _zoomDist;
            MapSceneCameraManager.instance.zoomDistance  = _zoomDist;
        }

        private Vector3 GetUpDir()
        {
            Vector3 upDir = MapSceneReferenceFrameManager.ReferenceFrame.InverseTransformDirection( Vector3.up );

            return upDir;
        }

        private void UpdateOrientation( Vector3 upDir )
        {
            Vector3 rightDir = Vector3.ProjectOnPlane( this.transform.right, upDir ).normalized;

            float mouseX = UnityEngine.Input.GetAxis( "Mouse X" );
            float mouseY = UnityEngine.Input.GetAxis( "Mouse Y" );

            this.transform.rotation = Quaternion.AngleAxis( -mouseY * MOVE_MULTIPLIER, rightDir ) * this.transform.rotation;
            this.transform.rotation = Quaternion.LookRotation( this.transform.forward, upDir );
            this.transform.rotation = Quaternion.AngleAxis( mouseX * MOVE_MULTIPLIER, upDir ) * this.transform.rotation;
        }

        void Awake()
        {
            _referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
        }

        void Start()
        {
            SyncZoomDist();
        }

        void Update()
        {
            if( !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
            {
                UpdateZoomLevel();
            }

            Vector3 upDir = GetUpDir();

            if( _isRotating )
            {
                UpdateOrientation( upDir );
            }
            else
            {
                this.transform.rotation = Quaternion.LookRotation( this.transform.forward, upDir );
            }
        }

        void FixedUpdate() // Setting in fixed update will provide a proper frame switch at the end of it, if the map camera is far away from (0,0,0)
        {
            Vector3 targetPos = (ReferenceObject == null)
                ? (Vector3)_referenceFrameTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( Vector3.zero )
                : ReferenceObject.position;

            this.transform.position = targetPos + (-transform.forward) * _zoomDist;
        }

        void LateUpdate()
        {
            Vector3 targetPos = (ReferenceObject == null)
                ? (Vector3)_referenceFrameTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( Vector3.zero )
                : ReferenceObject.position;

            this.transform.position = targetPos + (-transform.forward) * _zoomDist;
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( Input.InputChannel.SECONDARY_DOWN, InputChannelPriority.HIGH, Input_MouseDown );
            HierarchicalInputManager.AddAction( Input.InputChannel.SECONDARY_UP, InputChannelPriority.HIGH, Input_MouseUp );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( Input.InputChannel.SECONDARY_DOWN, Input_MouseDown );
            HierarchicalInputManager.RemoveAction( Input.InputChannel.SECONDARY_UP, Input_MouseUp );
        }

        private bool Input_MouseDown( float val )
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            _isRotating = true;
            return true;
        }

        private bool Input_MouseUp( float val )
        {
            _isRotating = false;
            return true;
        }

        public const string FOLLOW_MAP_FOCUS = HSPEvent.NAMESPACE_HSP + ".map_scene.camera.focus";

        [HSPEventListener( HSPEvent_AFTER_MAP_FOCUS_CHANGED.ID, FOLLOW_MAP_FOCUS )]
        private static void OnMapFocusChanged( IMapFocusable focus )
        {
            instance.ReferenceObject = focus.transform;
        }
    }
}