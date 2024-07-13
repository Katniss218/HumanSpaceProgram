using HSP.Core;
using UnityEngine;

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
            float zoomDist = this.transform.position.magnitude;

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

        void Awake()
        {
            _effectCameraNearPlane = this._effectCamera.nearClipPlane;
        }

        private void OnEnable()
        {
            _textureCreator = this._farCamera.gameObject.AddComponent<BeforeRenderEventCaller>();
            _textureReleaser = this._uiCamera.gameObject.AddComponent<AfterRenderEventCaller>();
        }

        private void OnDisable()
        {
            Destroy( _textureCreator );
            Destroy( _textureReleaser );
        }

        void LateUpdate()
        {
            AdjustCameras();
        }
    }
}