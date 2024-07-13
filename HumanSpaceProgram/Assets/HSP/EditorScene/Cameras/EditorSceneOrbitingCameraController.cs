using HSP.Input;
using System;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.DesignScene.Cameras
{
    public class EditorSceneOrbitingCameraController : SingletonMonoBehaviour<EditorSceneOrbitingCameraController>
    {
        [field: SerializeField]
        public Transform CameraParent { get; set; }

        [field: SerializeField]
        public float ZoomDist { get; private set; } = 5;

        const float MOVE_MULTIPLIER = 3.0f;
        const float ZOOM_MULTIPLIER = 0.15f;

        const float MIN_ZOOM_DISTANCE = 1f;
        const float MAX_ZOOM_DISTANCE = 1e10f;

        bool _isRotating;

        private void UpdateZoomLevel()
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

            // ---
            this.CameraParent.localPosition = Vector3.back * ZoomDist;
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
    }
}