using HSP.Input;
using System;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.DesignScene.Cameras
{
    public class DesignSceneOrbitingCameraController : SingletonMonoBehaviour<DesignSceneOrbitingCameraController>
    {
        [field: SerializeField]
        public Transform CameraParent { get; set; }

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
        const float MAX_ZOOM_DISTANCE = 1e10f;

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

            // ---
            this.CameraParent.localPosition = Vector3.back * _zoomDist;
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

        void Update()
        {
            if( !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
            {
                UpdateZoomLevel();
            }

            Vector3 upDir = Vector3.up;

            if( _isRotating )
            {
                UpdateOrientation( upDir );
            }
            else
            {
                this.transform.rotation = Quaternion.LookRotation( this.transform.forward, upDir );
            }
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_SECONDARY_DOWN, HierarchicalInputPriority.HIGH, Input_MouseDown );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_SECONDARY_UP, HierarchicalInputPriority.HIGH, Input_MouseUp );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_SECONDARY_DOWN, Input_MouseDown );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_SECONDARY_UP, Input_MouseUp );
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

        // TODO - add an ortho camera, blender-style.
    }
}