using HSP.Core;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityPlus.AssetManagement;

namespace HSP.GameplayScene.Cameras
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

        [HSPEventListener( HSPEvent.STARTUP_MAINMENU, "vanilla.mainmenuscene_camera" )]
        private static void OnGameplaySceneLoad()
        {
            GameObject cameraPivotGameObject = new GameObject( "Camera Pivot" );

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
        }

        [HSPEventListener( HSPEvent.STARTUP_MAINMENU, "vanilla.mainmenuscene_postprocessing", After = new[] { "vanilla.mainmenuscene_camera" } )]
        private static void CreatePostProcessingLayers()
        {
            void SetupPPL( PostProcessLayer layer )
            {
                layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                layer.temporalAntialiasing.jitterSpread = 0.65f;
                layer.temporalAntialiasing.stationaryBlending = 0.99f;
                layer.temporalAntialiasing.motionBlending = 0.25f;
                layer.temporalAntialiasing.sharpness = 0.1f;
                layer.volumeLayer = Layer.POST_PROCESSING.ToMask();
                layer.volumeTrigger = layer.transform;
                layer.stopNaNPropagation = true;

                // This is required, for some stupid reason.
                var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::com.unity.postprocessing/PostProcessing/PostProcessResources" );
                layer.Init( postProcessResources );
                layer.InitBundles();
            }

            PostProcessLayer nearPPL = instance._nearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL );

            PostProcessLayer uiPPL = instance._uiCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }
    }
}