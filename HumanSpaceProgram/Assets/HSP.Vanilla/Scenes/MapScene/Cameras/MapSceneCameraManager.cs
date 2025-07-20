using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Scenes.MapScene.Cameras
{
    public class MapSceneCameraManager : SingletonMonoBehaviour<MapSceneCameraManager>
    {
        const float ZOOM_NEAR_PLANE_MULT = 1e-8f;

        const float MIN_ZOOM_DISTANCE = 1f;

        const float NEAR_CUTOFF_DISTANCE = 1e6f; // should be enough of a conservative value. Near cam is only 100 km, not 1000.

        const float NEAR_MIN = 0.1f;
        const float NEAR_MAX = 200.0f;

        [field: SerializeField]
        public Transform CameraParent { get; private set; }

        [SerializeField]
        Camera _farCamera;

        [SerializeField]
        Camera _effectCamera;

        [SerializeField]
        Camera _uiCamera;

        public static Camera FarCamera => instance._farCamera;
        public static Camera EffectCamera => instance._effectCamera;
        public static Camera UICamera => instance._uiCamera;

        float _effectCameraNearPlane;

        private void AdjustCameras()
        {
            float zoomDist = this.CameraParent.position.magnitude;

            // helps to make the shadow look nicer.
            QualitySettings.shadowDistance = 2550.0f + 1.3f * zoomDist;

            //
            // When a camera is far away from scene origin, it needs to have appropriately scaled near and far plane values (roughly in the ballpark of how far away it currently is),
            //   otherwise the camera starts throwing "position out of view frustum" exceptions.
            //
            // This can be mitigated either by increasing the clipping planes when highly zoomed out, or by turning the camera off.
            //

            _effectCamera.nearClipPlane = _effectCameraNearPlane * (1 + (zoomDist * ZOOM_NEAR_PLANE_MULT));

            _uiCamera.nearClipPlane = (float)MathD.Map( zoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, 0.5f, 100f );
            _uiCamera.farClipPlane = (float)MathD.Map( zoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, 0.5f * 10000f, 100f * 10000f );
        }

        void Start()
        {
            _farCamera.clearFlags = CameraClearFlags.Skybox;
            _farCamera.nearClipPlane = 90_000f;
            _farCamera.farClipPlane = 1e18f;
            _farCamera.fieldOfView = 60f;
            _farCamera.depth = -25f;

            _effectCamera.clearFlags = CameraClearFlags.Nothing;
            _effectCamera.nearClipPlane = 10f;
            _effectCamera.farClipPlane = 1e15f;
            _effectCamera.fieldOfView = 60f;
            _effectCamera.depth = 90;

            _uiCamera.clearFlags = CameraClearFlags.Nothing;
            _uiCamera.nearClipPlane = 0.5f;
            _uiCamera.farClipPlane = 5000f;
            _uiCamera.fieldOfView = 60f;
            _uiCamera.depth = 99;

            _farCamera.cullingMask =
                  Layer.MAP.ToMask()
                | Layer.Unity_TransparentFx.ToMask()
                | Layer.Unity_IgnoreRaycast.ToMask()
                | Layer.Unity_Water.ToMask();

            _effectCamera.cullingMask = 0;

            _uiCamera.cullingMask =
                  Layer.Unity_UI.ToMask()
                | Layer.SCENE_UI.ToMask();

            _effectCameraNearPlane = this._effectCamera.nearClipPlane;
        }

        void LateUpdate()
        {
            AdjustCameras();
        }

        public const string CREATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".map_scene.camera.create";
        public const string ACTIVATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".map_scene.camera.activate";
        public const string DEACTIVATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".map_scene.camera.deactivate";

        static GameObject _cameraPivot;

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, CREATE_CAMERA )]
        private static void OnGameplaySceneLoad()
        {
            GameObject cameraPivotGameObject = new GameObject( "Camera Pivot" );
            _cameraPivot = cameraPivotGameObject;

            SceneCamera sceneCamera = cameraPivotGameObject.AddComponent<SceneCamera>();

            MapSceneCameraManager cameraManager = cameraPivotGameObject.AddComponent<MapSceneCameraManager>();
            MapSceneOrbitingCameraController cameraController = cameraPivotGameObject.AddComponent<MapSceneOrbitingCameraController>();

            GameObject cameraParentGameObject = new GameObject( "Camera Parent" );
            cameraParentGameObject.transform.SetParent( cameraPivotGameObject.transform );

            AudioListener audioListener = cameraParentGameObject.AddComponent<AudioListener>();

            GameObject farCameraGameObject = new GameObject( "Far camera" );
            farCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera farCamera = farCameraGameObject.AddComponent<Camera>();

            GameObject effectCameraGameObject = new GameObject( "Effect camera" );
            effectCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera effectCamera = effectCameraGameObject.AddComponent<Camera>();

            GameObject uiCameraGameObject = new GameObject( "UI camera" );
            uiCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera uiCamera = uiCameraGameObject.AddComponent<Camera>();

            sceneCamera.camera = uiCamera;
            cameraManager.CameraParent = cameraParentGameObject.transform;
            cameraManager._farCamera = farCamera;
            cameraManager._effectCamera = effectCamera;
            cameraManager._uiCamera = uiCamera;

            cameraController.CameraParent = cameraParentGameObject.transform;
            cameraController.ZoomDist = 12_000_000f;

            Skybox skybox = farCameraGameObject.AddComponent<Skybox>();
            skybox.material = AssetRegistry.Get<Material>( "builtin::HSP._DevUtils/skybox" );

            _cameraPivot.SetActive( false );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, ACTIVATE_CAMERA )]
        private static void OnGameplaySceneActivate()
        {
            _cameraPivot.SetActive( true );
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_DEACTIVATE.ID, DEACTIVATE_CAMERA )]
        private static void OnGameplaySceneDeactivate()
        {
            _cameraPivot.SetActive( false );
        }
    }
}