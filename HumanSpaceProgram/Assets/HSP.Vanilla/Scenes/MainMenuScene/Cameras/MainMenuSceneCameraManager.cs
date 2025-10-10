using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.GameplayScene;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene.Cameras
{
    /// <summary>
    /// Manages the multi-camera setup of the gameplay scene.
    /// </summary>
    public class MainMenuSceneCameraManager : SingletonMonoBehaviour<MainMenuSceneCameraManager>
    {
        const float ZOOM_NEAR_PLANE_MULT = 1e-8f;

        const float MIN_ZOOM_DISTANCE = 1f;

        const float NEAR_CUTOFF_DISTANCE = 1e6f; // should be enough of a conservative value. Near cam is only 100 km, not 1000.

        const float NEAR_MIN = 0.1f;
        const float NEAR_MAX = 200.0f;

        // Two-camera setup because the shadow distance is permanently tied to the far plane distance.

        [field: SerializeField]
        public Transform CameraParent { get; private set; }

        /// <summary>
        /// Used for rendering the vessels and other close / small objects, as well as shadows.
        /// </summary>
        [SerializeField]
        Camera _nearCamera;

        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
        [SerializeField]
        Camera _effectCamera;

        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
        [SerializeField]
        Camera _uiCamera;

        public static Camera NearCamera => instance._nearCamera;
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

            _nearCamera.nearClipPlane = (float)MathD.Map( zoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, NEAR_MIN, NEAR_MAX );

            _effectCamera.nearClipPlane = _effectCameraNearPlane * (1 + (zoomDist * ZOOM_NEAR_PLANE_MULT));

            _uiCamera.nearClipPlane = (float)MathD.Map( zoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, 0.5f, 100f );
            _uiCamera.farClipPlane = (float)MathD.Map( zoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, 0.5f * 10000f, 100f * 10000f );

            _nearCamera.eventMask = 0; // Setting eventMask = 0 stops the annoying mouse event errors when the camera is far away from scene origin.
            _effectCamera.eventMask = 0;
            _uiCamera.eventMask = 0;

            if( this.transform.position.magnitude > NEAR_CUTOFF_DISTANCE )
            {
                this._nearCamera.enabled = false;
            }
            else
            {
                this._nearCamera.enabled = true;
            }
        }

        void Start()
        {
            _nearCamera.clearFlags = CameraClearFlags.Skybox;
            _nearCamera.nearClipPlane = 0.1f;
            _nearCamera.farClipPlane = 100_000f;
            _nearCamera.fieldOfView = 60f;
            _nearCamera.depth = -22f;

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

            _nearCamera.cullingMask =
                  Layer.DEFAULT.ToMask()
                | Layer.Unity_TransparentFx.ToMask()
                | Layer.Unity_IgnoreRaycast.ToMask()
                | Layer.Unity_Water.ToMask()
                | Layer.PART_OBJECT.ToMask()
                | Layer.PART_OBJECT_LIGHT.ToMask()
                | Layer.VESSEL_DESIGN_HELD.ToMask();

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

        public const string CREATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.camera.create";
        public const string ACTIVATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.camera.activate";
        public const string DEACTIVATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.camera.deactivate";

        static GameObject _cameraPivot;

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_LOAD.ID, CREATE_CAMERA )]
        private static void OnMainMenuSceneLoad()
        {
            GameObject cameraPivotGameObject = new GameObject( "Camera Pivot" );
            _cameraPivot = cameraPivotGameObject;

            SceneCamera sceneCamera = cameraPivotGameObject.AddComponent<SceneCamera>();
            MainMenuSceneCameraManager cameraManager = cameraPivotGameObject.AddComponent<MainMenuSceneCameraManager>();

            GameObject cameraParentGameObject = new GameObject( "Camera Parent" );
            cameraParentGameObject.transform.SetParent( cameraPivotGameObject.transform );

            AudioListener audioListener = cameraParentGameObject.AddComponent<AudioListener>();

            GameObject nearCameraGameObject = new GameObject( "Near camera" );
            nearCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera nearCamera = nearCameraGameObject.AddComponent<Camera>();

            GameObject effectCameraGameObject = new GameObject( "Effect camera" );
            effectCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera effectCamera = effectCameraGameObject.AddComponent<Camera>();

            GameObject uiCameraGameObject = new GameObject( "UI camera" );
            uiCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera uiCamera = uiCameraGameObject.AddComponent<Camera>();

            sceneCamera.camera = nearCamera;
            cameraManager.CameraParent = cameraParentGameObject.transform;
            cameraManager._nearCamera = nearCamera;
            cameraManager._effectCamera = effectCamera;
            cameraManager._uiCamera = uiCamera;

            _cameraPivot.SetActive( false );
        }

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_ACTIVATE.ID, ACTIVATE_CAMERA )]
        private static void OnMainMenuSceneActivate()
        {
            _cameraPivot.SetActive( true );
        }

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_DEACTIVATE.ID, DEACTIVATE_CAMERA )]
        private static void OnMainMenuSceneDeactivate()
        {
            _cameraPivot.SetActive( false );
        }
    }
}