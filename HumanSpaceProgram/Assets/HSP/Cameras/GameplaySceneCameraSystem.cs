using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HSP.Cameras
{
    public class GameplaySceneCameraSystem : SingletonMonoBehaviour<GameplaySceneCameraSystem>
    {
        public class RenderTextureCreator : MonoBehaviour
        {
            /*void OnPreCull()
            {
                instance._farCamera.clearFlags = CameraClearFlags.Skybox;
                instance._nearCamera.clearFlags = CameraClearFlags.Depth;

                instance._colorRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32 );
                instance._farDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 24, RenderTextureFormat.Depth );
                instance._nearDepthRT = RenderTexture.GetTemporary( Screen.width, Screen.height, 24, RenderTextureFormat.Depth );

                instance._farCamera.SetTargetBuffers( instance._colorRT.colorBuffer, instance._farDepthRT.depthBuffer );
                instance._nearCamera.SetTargetBuffers( instance._colorRT.colorBuffer, instance._nearDepthRT.depthBuffer );
            }*/
        }

        public class RenderTextureReleaser : MonoBehaviour
        {
            /*void OnPostRender()
            {
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
            }*/
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
        [field: SerializeField]
        Camera _farCamera;

        /// <summary>
        /// Used for rendering the vessels and other close / small objects, as well as shadows.
        /// </summary>
        [field: SerializeField]
        Camera _nearCamera;

        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
        [field: SerializeField]
        Camera _effectCamera;

        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
        [field: SerializeField]
        Camera _uiCamera;

        [SerializeField]
        RenderTexture _colorRT;

        [SerializeField]
        RenderTexture _farDepthRT;

        [SerializeField]
        RenderTexture _nearDepthRT;

        RenderTextureCreator _textureCreator;
        RenderTextureReleaser _textureReleaser;

        public static RenderTexture ColorRenderTexture => instance._colorRT;
        public static RenderTexture FarDepthRenderTexture => instance._farDepthRT;
        public static RenderTexture NearDepthRenderTexture => instance._nearDepthRT;

        PostProcessLayer _farPPLayer;
        PostProcessLayer _nearPPLayer;

        float _effectCameraNearPlane;

        /// <summary>
        /// Use this for raycasts and other methods that use a camera to calculate screen space positions, etc.
        /// </summary>
        /// <remarks>
        /// Do not manually modify the fields of this camera.
        /// </remarks>
        public static Camera MainCamera { get => instance._nearCamera; }

        public static Camera NearCamera { get => instance._nearCamera; }
        public static Camera FarCamera { get => instance._farCamera; }

        void TryToggleNearCamera()
        {
            // For some reason, at the distance of around Earth's radius, having the near camera enabled throws "position our of view frustum" exceptions.
            if( this.transform.position.magnitude > NEAR_CUTOFF_DISTANCE )
            {
                this._nearCamera.cullingMask -= 1 << 31;
                this._farCamera.cullingMask -= 1 << 31;
                this._effectCamera.cullingMask = 1 << 31; // for some reason, this makes it draw properly, also has the effect of drawing PPP on top of everything.

                // instead of disabling, it's possible that we can increase the near clipping plane instead, the further the camera is zoomed out (up to ~30k at very far zooms).
                // Map view could work by constructing a virtual environment (planets at l0 subdivs) with the camera always in the center.
                // the camera would toggle to only render that view (like scaled space, but real size)
                // vessels and buildings would be invisible in map view.
                this._nearCamera.enabled = false;
                this._nearPPLayer.enabled = false;
                this._farPPLayer.enabled = true;
            }
            else
            {
                this._nearCamera.cullingMask += 1 << 31;
                this._farCamera.cullingMask += 1 << 31;
                this._effectCamera.cullingMask = 0; // Prevents the atmosphere drawing over the geometry, somehow.

                this._nearCamera.enabled = true;
                this._nearPPLayer.enabled = true;
                this._farPPLayer.enabled = false;
            }
        }

        void Awake()
        {
            _effectCameraNearPlane = this._effectCamera.nearClipPlane;
            _nearPPLayer = this._nearCamera.GetComponent<PostProcessLayer>();
            _farPPLayer = this._farCamera.GetComponent<PostProcessLayer>();

#warning TODO - for some reason, using temporaries fucks with something. maybe causes it to be desynchronized or something? idk...
            _farCamera.SetTargetBuffers( _colorRT.colorBuffer, _farDepthRT.depthBuffer );
            _nearCamera.SetTargetBuffers( _colorRT.colorBuffer, _nearDepthRT.depthBuffer );
        }

        private void OnEnable()
        {
            _textureCreator = this._farCamera.gameObject.AddComponent<RenderTextureCreator>();
            _textureReleaser = this._uiCamera.gameObject.AddComponent<RenderTextureReleaser>();
        }

        private void OnDisable()
        {
            Destroy( _textureCreator );
            Destroy( _textureReleaser );
        }

        void LateUpdate()
        {
            // After modifying position/rotation/zoom.
            // TryToggleNearCamera();

            float zoomDist = this.transform.position.magnitude;

            // helps to make the shadow look nicer.
            QualitySettings.shadowDistance = 2550.0f + 1.3f * zoomDist;

            // Helps to prevent exceptions being thrown at medium zoom levels (due to something with precision of the view frustum).
            _effectCamera.nearClipPlane = _effectCameraNearPlane * (1 + (zoomDist * ZOOM_NEAR_PLANE_MULT));
            _nearCamera.nearClipPlane = (float)MathD.Map( zoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, NEAR_MIN, NEAR_MAX );
        }
    }
}