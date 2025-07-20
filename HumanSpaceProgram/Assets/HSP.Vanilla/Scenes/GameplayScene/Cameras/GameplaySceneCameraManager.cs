using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.Scenes.GameplayScene.Cameras
{
    /// <summary>
    /// Invoked before the first camera starts rendering.
    /// </summary>
    public static class HSPEvent_BEFORE_GAMEPLAY_SCENE_RENDERING
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplayscene.rendering.before";
    }

    /// <summary>
    /// Invoked after the last camera has finished rendering.
    /// </summary>
    public static class HSPEvent_AFTER_GAMEPLAY_SCENE_RENDERING
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplayscene.rendering.after";
    }

    /// <summary>
    /// Manages the multi-camera setup of the gameplay scene.
    /// </summary>
    public class GameplaySceneCameraManager : SingletonMonoBehaviour<GameplaySceneCameraManager>
    {
        public class BeforeRenderEventCaller : MonoBehaviour
        {
            void OnPreRender()
            {
                instance._farCamera.clearFlags = CameraClearFlags.Skybox;
                instance._nearCamera.clearFlags = CameraClearFlags.Depth;

                int width = Screen.fullScreen ? Screen.currentResolution.width : Screen.width;
                int height = Screen.fullScreen ? Screen.currentResolution.height : Screen.height;

                //instance._colorRT = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 4 ); Doesn't work, adding MSAA for some reason turns it black.
                instance._colorRT = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.ARGB32 );
                instance._farDepthRT = RenderTexture.GetTemporary( width, height, 24, RenderTextureFormat.Depth );
                instance._nearDepthRT = RenderTexture.GetTemporary( width, height, 24, RenderTextureFormat.Depth );

                instance._farCamera.SetTargetBuffers( instance._colorRT.colorBuffer, instance._farDepthRT.depthBuffer );
                instance._nearCamera.SetTargetBuffers( instance._colorRT.colorBuffer, instance._nearDepthRT.depthBuffer );

                HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_GAMEPLAY_SCENE_RENDERING.ID );
            }
        }

        public class AfterRenderEventCaller : MonoBehaviour
        {
            void OnPostRender()
            {
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_GAMEPLAY_SCENE_RENDERING.ID );

                if( instance._colorRT != null )
                {
                    RenderTexture.ReleaseTemporary( instance._colorRT );
                    instance._colorRT = null;
                }
                if( instance._farDepthRT != null )
                {
                    RenderTexture.ReleaseTemporary( instance._farDepthRT );
                    instance._farDepthRT = null;
                }
                if( instance._nearDepthRT != null )
                {
                    RenderTexture.ReleaseTemporary( instance._nearDepthRT );
                    instance._nearDepthRT = null;
                }
            }
        }

        const float ZOOM_NEAR_PLANE_MULT = 1e-8f;

        const float MIN_ZOOM_DISTANCE = 1f;

        const float NEAR_CUTOFF_DISTANCE = 1e6f; // should be enough of a conservative value. Near cam is only 100 km, not 1000.

        const float NEAR_MIN = 0.1f;
        const float NEAR_MAX = 200.0f;

        // Two-camera setup because the shadow distance is permanently tied to the far plane distance.

        [field: SerializeField]
        public Transform CameraParent { get; private set; }

        [SerializeField]
        Camera _farCamera;

        [SerializeField]
        Camera _nearCamera;

        [SerializeField]
        Camera _effectCamera;

        [SerializeField]
        Camera _uiCamera;

        RenderTexture _colorRT;
        RenderTexture _farDepthRT;
        RenderTexture _nearDepthRT;

        BeforeRenderEventCaller _textureCreator;
        AfterRenderEventCaller _textureReleaser;

        public static RenderTexture ColorRenderTexture => instance._colorRT;
        public static RenderTexture FarDepthRenderTexture => instance._farDepthRT;
        public static RenderTexture NearDepthRenderTexture => instance._nearDepthRT;

        /// <summary>
        /// Used for rendering the planets mostly.
        /// </summary>
        public static Camera FarCamera => instance._farCamera;
        /// <summary>
        /// Used for rendering the vessels and other close / small objects, as well as shadows.
        /// </summary>
        public static Camera NearCamera => instance._nearCamera;
        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
        public static Camera EffectCamera => instance._effectCamera;
        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
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
            _farCamera.clearFlags = CameraClearFlags.Skybox;
            _farCamera.nearClipPlane = 90_000f;
            _farCamera.farClipPlane = 1e18f;
            _farCamera.fieldOfView = 60f;
            _farCamera.depth = -25f;

            _nearCamera.clearFlags = CameraClearFlags.Depth;
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

            _farCamera.cullingMask =
                  Layer.DEFAULT.ToMask()
                | Layer.Unity_TransparentFx.ToMask()
                | Layer.Unity_IgnoreRaycast.ToMask()
                | Layer.Unity_Water.ToMask()
                | Layer.CELESTIAL_BODY.ToMask()
                | Layer.CELESTIAL_BODY_LIGHT.ToMask();

            _nearCamera.cullingMask =
                  Layer.DEFAULT.ToMask()
                | Layer.Unity_TransparentFx.ToMask()
                | Layer.Unity_IgnoreRaycast.ToMask()
                | Layer.Unity_Water.ToMask()
                | Layer.CELESTIAL_BODY.ToMask()
                | Layer.CELESTIAL_BODY_LIGHT.ToMask()
                | Layer.PART_OBJECT.ToMask()
                | Layer.PART_OBJECT_LIGHT.ToMask()
                | Layer.VESSEL_DESIGN_HELD.ToMask();

            _effectCamera.cullingMask = 0;

            _uiCamera.cullingMask =
                  Layer.Unity_UI.ToMask()
                | Layer.SCENE_UI.ToMask();

            _effectCameraNearPlane = this._effectCamera.nearClipPlane;

            if( _textureCreator == null && this._farCamera != null )
                _textureCreator = this._farCamera.gameObject.AddComponent<BeforeRenderEventCaller>();
            if( _textureReleaser == null && this._uiCamera != null )
                _textureReleaser = this._uiCamera.gameObject.AddComponent<AfterRenderEventCaller>();
        }

        void OnEnable()
        {
            if( _textureCreator == null && this._farCamera != null )
                _textureCreator = this._farCamera.gameObject.AddComponent<BeforeRenderEventCaller>();
            if( _textureReleaser == null && this._uiCamera != null )
                _textureReleaser = this._uiCamera.gameObject.AddComponent<AfterRenderEventCaller>();
        }

        void OnDisable()
        {
            if( _textureCreator != null )
                Destroy( _textureCreator );
            if( _textureReleaser != null )
                Destroy( _textureReleaser );
        }

        void LateUpdate()
        {
            AdjustCameras();
        }

        public const string CREATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.camera.create";
        public const string ACTIVATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.camera.activate";
        public const string DEACTIVATE_CAMERA = HSPEvent.NAMESPACE_HSP + ".gameplay_scene.camera.deactivate";

        static GameObject _cameraPivot;

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, CREATE_CAMERA )]
        private static void OnGameplaySceneLoad()
        {
            GameObject cameraPivotGameObject = new GameObject( "Camera Pivot" );
            _cameraPivot = cameraPivotGameObject;

            SceneCamera sceneCamera = cameraPivotGameObject.AddComponent<SceneCamera>();

            GameplaySceneCameraManager cameraManager = cameraPivotGameObject.AddComponent<GameplaySceneCameraManager>();
            GameplaySceneOrbitingCameraController cameraController = cameraPivotGameObject.AddComponent<GameplaySceneOrbitingCameraController>();

            GameObject cameraParentGameObject = new GameObject( "Camera Parent" );
            cameraParentGameObject.transform.SetParent( cameraPivotGameObject.transform );

            AudioListener audioListener = cameraParentGameObject.AddComponent<AudioListener>();

            GameObject farCameraGameObject = new GameObject( "Far camera" );
            farCameraGameObject.transform.SetParent( cameraParentGameObject.transform );
            Camera farCamera = farCameraGameObject.AddComponent<Camera>();

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
            cameraManager._farCamera = farCamera;
            cameraManager._nearCamera = nearCamera;
            cameraManager._effectCamera = effectCamera;
            cameraManager._uiCamera = uiCamera;

            cameraController.CameraParent = cameraParentGameObject.transform;
            cameraController.ZoomDist = 5f;

            GameplaySceneDepthBufferCombiner bufferCombiner = nearCameraGameObject.AddComponent<GameplaySceneDepthBufferCombiner>();
            bufferCombiner.FarCamera = farCamera;
            bufferCombiner.NearCamera = nearCamera;
            bufferCombiner.EffectCamera = effectCamera;
            bufferCombiner.MergeDepthShader = AssetRegistry.Get<Shader>( "builtin::Resources/Shaders/merge_depth" );

            Skybox skybox = farCameraGameObject.AddComponent<Skybox>();
            skybox.material = AssetRegistry.Get<Material>( "builtin::HSP._DevUtils/skybox" );

            _cameraPivot.SetActive( false );
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, ACTIVATE_CAMERA )]
        private static void OnGameplaySceneActivate()
        {
            _cameraPivot.SetActive( true );
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, DEACTIVATE_CAMERA )]
        private static void OnGameplaySceneDeactivate()
        {
            _cameraPivot.SetActive( false );
        }
    }
}