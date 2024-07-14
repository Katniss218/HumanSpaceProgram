using HSP.CelestialBodies;
using HSP.Core;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityPlus.AssetManagement;

namespace HSP.GameplayScene.Cameras
{
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

                instance._colorRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );
                instance._farDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 24, RenderTextureFormat.Depth );
                instance._nearDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 24, RenderTextureFormat.Depth );

                instance._farCamera.SetTargetBuffers( instance._colorRT.colorBuffer, instance._farDepthRT.depthBuffer );
                instance._nearCamera.SetTargetBuffers( instance._colorRT.colorBuffer, instance._nearDepthRT.depthBuffer );

                HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_BEFORE_RENDERING );
            }
        }

        public class AfterRenderEventCaller : MonoBehaviour
        {
            void OnPostRender()
            {
                HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_RENDERING );

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

        /// <summary>
        /// Used for rendering the planets mostly.
        /// </summary>
        [SerializeField]
        Camera _farCamera;

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

        RenderTexture _colorRT;
        RenderTexture _farDepthRT;
        RenderTexture _nearDepthRT;

        BeforeRenderEventCaller _textureCreator;
        AfterRenderEventCaller _textureReleaser;

        public static RenderTexture ColorRenderTexture => instance._colorRT;
        public static RenderTexture FarDepthRenderTexture => instance._farDepthRT;
        public static RenderTexture NearDepthRenderTexture => instance._nearDepthRT;

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

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.gameplayscene_camera" )]
        private static void OnGameplaySceneLoad()
        {
            GameObject cameraPivotGameObject = new GameObject( "Camera Pivot" );

            SceneCamera sceneCamera = cameraPivotGameObject.AddComponent<SceneCamera>();
            GameplaySceneCameraManager cameraManager = cameraPivotGameObject.AddComponent<GameplaySceneCameraManager>();
            GameplaySceneOrbitingCameraController cameraController = cameraPivotGameObject.AddComponent<GameplaySceneOrbitingCameraController>();

            GameObject cameraParentGameObject = new GameObject( "Camera Parent" );
            cameraParentGameObject.transform.SetParent( cameraPivotGameObject.transform );

            AudioListener audioListener = cameraParentGameObject.AddComponent<AudioListener>();

#warning TODO - it'd be very good to add events for all these tbh. make them overridable, etc.
            // event for post processing.
            // event for controller.

            // events calling events shouldn't be some sort of taboo, it makes a lot of sense imo. Makes it a bit harder to trace/debug, but it should be well worth it.
            // could also be done with a topologically sortable event listeners.

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

            AtmosphereRenderer atmosphereRenderer = effectCameraGameObject.AddComponent<AtmosphereRenderer>();
            atmosphereRenderer.light = GameObject.Find( "CBLight" ).GetComponent<Light>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.gameplayscene_postprocessing", After = new[] { "vanilla.gameplayscene_camera" } )]
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
                // I had to copy the PostProcessResources.asset file from the `\HumanSpaceProgram\Library\PackageCache\com.unity.postprocessing@3.2.2\PostProcessing` directory.
                //   It could maybe be addressed from there directly though (tho it needs a modification of the AssetRegistererAssetSource class to be able to handle it).
                //   It can be enumerated with AssetDatabase.
                //var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::Resources/com.unity.postprocessing/PostProcessResources" );
                var postProcessResources = AssetRegistry.Get<PostProcessResources>( "builtin::com.unity.postprocessing/PostProcessing/PostProcessResources" );
                layer.Init( postProcessResources );
                layer.InitBundles();
            }

#warning TODO - PPP as a separate subsystem, maybe a bundled mod.

            PostProcessLayer farPPL = GameplaySceneCameraManager.instance._farCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( farPPL );

            PostProcessLayer nearPPL = GameplaySceneCameraManager.instance._nearCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( nearPPL );

            PostProcessLayer uiPPL = GameplaySceneCameraManager.instance._uiCamera.gameObject.AddComponent<PostProcessLayer>();
            SetupPPL( uiPPL );
        }
    }
}