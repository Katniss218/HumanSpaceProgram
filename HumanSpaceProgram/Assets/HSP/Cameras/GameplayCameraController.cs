using HSP.Core;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HSP.Cameras
{
    public class GameplayCameraController : SingletonMonoBehaviour<GameplayCameraController>
    {
        /// <summary>
        /// The camera will focus on this object.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceObject { get; set; }

        [field: SerializeField]
        public Transform CameraParent { get; set; }

        [field: SerializeField]
        public float ZoomDist { get; private set; } = 5;

        // Two-camera setup because the shadow distance is permanently tied to the far plane distance.

        /// <summary>
        /// Used for rendering the vessels and other close / small objects, as well as shadows.
        /// </summary>
        [field: SerializeField]
        Camera _nearCamera;

        /// <summary>
        /// Used for rendering the planets mostly.
        /// </summary>
        [field: SerializeField]
        Camera _farCamera;

        /// <summary>
        /// Used for rendering screen space effects, like atmospheres.
        /// </summary>
        [field: SerializeField]
        Camera _effectCamera;

        [field: SerializeField]
        PostProcessLayer _closePPLayer;

        [field: SerializeField]
        PostProcessLayer _farPPLayer;

        float _effectCameraNearPlane;

        const float MOVE_MULTIPLIER = 3.0f;
        const float ZOOM_MULTIPLIER = 0.15f;

        const float ZOOM_NEAR_PLANE_MULT = 1e-8f;

        const float MIN_ZOOM_DISTANCE = 1f;
        const float MAX_ZOOM_DISTANCE = 1e10f;

        const float NEAR_CUTOFF_DISTANCE = 1e6f; // should be enough of a conservative value. Near cam is only 100 km, not 1000.

        const float NEAR_MIN = 0.1f;
        const float NEAR_MAX = 200.0f;

        /// <summary>
        /// Use this for raycasts and other methods that use a camera to calculate screen space positions, etc.
        /// </summary>
        /// <remarks>
        /// Do not manually modify the fields of this camera.
        /// </remarks>
        public static Camera MainCamera { get => instance._nearCamera; }

        bool _isTranslating;
        bool _isRotating;

        void UpdateZoomLevel()
        {
            if( UnityEngine.Input.mouseScrollDelta.y > 0 )
            {
                ZoomDist -= ZoomDist * ZOOM_MULTIPLIER;
            }
            else if( UnityEngine.Input.mouseScrollDelta.y < 0 )
            {
                ZoomDist += ZoomDist * ZOOM_MULTIPLIER;
            }

            ZoomDist = Mathf.Clamp( ZoomDist, MIN_ZOOM_DISTANCE, MAX_ZOOM_DISTANCE );

            // helps to make the shadow look nicer.
            QualitySettings.shadowDistance = 2550.0f + 1.3f * ZoomDist;

            // ---
            CameraParent.transform.localPosition = Vector3.back * ZoomDist;
        }

        void UpdateOrientation()
        {
            float mouseX = UnityEngine.Input.GetAxis( "Mouse X" );
            float mouseY = UnityEngine.Input.GetAxis( "Mouse Y" );

            this.transform.rotation *= Quaternion.AngleAxis( mouseX * MOVE_MULTIPLIER, Vector3.up );
            this.transform.rotation *= Quaternion.AngleAxis( -mouseY * MOVE_MULTIPLIER, Vector3.right );
        }

        void UpdatePosition()
        {
            if( ReferenceObject != null )
                return;

            float mouseX = UnityEngine.Input.GetAxis( "Mouse X" );
            float mouseY = UnityEngine.Input.GetAxis( "Mouse Y" );

            this.transform.position += mouseX * MOVE_MULTIPLIER * this.transform.right;
            this.transform.position += mouseY * MOVE_MULTIPLIER * this.transform.forward;
        }

        void TryToggleNearCamera()
        {
            // For some reason, at the distance of around Earth's radius, having the near camera enabled throws "position our of view frustum" exceptions.
            if( ZoomDist > NEAR_CUTOFF_DISTANCE )
            {
                this._nearCamera.cullingMask -= 1 << 31;
                this._farCamera.cullingMask -= 1 << 31;
                this._effectCamera.cullingMask = 1 << 31; // for some reason, this makes it draw properly, also has the effect of drawing PPP on top of everything.

                // instead of disabling, it's possible that we can increase the near clipping plane instead, the further the camera is zoomed out (up to ~30k at very far zooms).
                // Map view could work by constructing a virtual environment (planets at l0 subdivs) with the camera always in the center.
                // the camera would toggle to only render that view (like scaled space, but real size)
                // vessels and buildings would be invisible in map view.
                this._nearCamera.enabled = false;
                this._closePPLayer.enabled = false;
                this._farPPLayer.enabled = true;
            }
            else
            {
                this._nearCamera.cullingMask += 1 << 31;
                this._farCamera.cullingMask += 1 << 31;
                this._effectCamera.cullingMask = 0; // Prevents the atmosphere drawing over the geometry, somehow.

                this._nearCamera.enabled = true;
                this._closePPLayer.enabled = true;
                this._farPPLayer.enabled = false;
            }
        }

        void Awake()
        {
            _effectCameraNearPlane = this._effectCamera.nearClipPlane;
        }

        void Update()
        {
            if( !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
            {
                UpdateZoomLevel();

                if( UnityEngine.Input.GetKeyDown( KeyCode.Mouse1 ) ) // Mouse1 = Right Mouse Button
                {
                    _isRotating = true;
                }
                if( UnityEngine.Input.GetKeyDown( KeyCode.Mouse2 ) ) // Mouse2 = Middle Mouse Button
                {
                    _isTranslating = true;
                }
            }

            if( _isRotating && UnityEngine.Input.GetKeyUp( KeyCode.Mouse1 ) )
            {
                _isRotating = false;
            }
            if( _isTranslating && UnityEngine.Input.GetKeyUp( KeyCode.Mouse2 ) )
            {
                _isTranslating = false;
            }

            if( _isRotating )
            {
                UpdateOrientation();
            }
            if( _isTranslating )
            {
                UpdatePosition();
            }
        }

        void LateUpdate()
        {
            if( ReferenceObject != null ) // Raycasts using rays from the camera fail when the vessel is moving fast, but updating the camera earlier as well as later doesn't fix it.
            {
                this.transform.position = ReferenceObject.transform.position;
            }

            // After modifying position/rotation/zoom.
            TryToggleNearCamera();

            // Helps to prevent exceptions being thrown at medium zoom levels (due to something with precision of the view frustum).
            _effectCamera.nearClipPlane = _effectCameraNearPlane * (1 + (ZoomDist * ZOOM_NEAR_PLANE_MULT));
            _nearCamera.nearClipPlane = (float)MathD.Map( ZoomDist, MIN_ZOOM_DISTANCE, NEAR_CUTOFF_DISTANCE, NEAR_MIN, NEAR_MAX );
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_VANILLA + "camera.snap_to_vessel" )]
        static void SnapToActiveObject( object e )
        {
            if( ActiveObjectManager.ActiveObject == null )
                instance.ReferenceObject = null;
            else
                instance.ReferenceObject = ActiveObjectManager.ActiveObject.transform;
        }
    }
}