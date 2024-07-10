using HSP.Core;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Cameras
{
    public class OrbitingCameraController : SingletonMonoBehaviour<OrbitingCameraController>
    {
        /// <summary>
        /// The camera will focus on this object.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceObject { get; set; }

        [field: SerializeField]
        public float ZoomDist { get; private set; } = 5;

        [field: SerializeField]
        GameplaySceneCamera _camera;

        const float MOVE_MULTIPLIER = 3.0f;
        const float ZOOM_MULTIPLIER = 0.15f;

        const float MIN_ZOOM_DISTANCE = 1f;
        const float MAX_ZOOM_DISTANCE = 1e10f;

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

            // ---
            _camera.transform.localPosition = Vector3.back * ZoomDist;
        }

        private Vector3 GetUpDir()
        {
            Vector3Dbl airfGravVec = GravityUtils.GetNBodyGravityAcceleration( SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( ReferenceObject.position ) );

            Vector3 upDir = -SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformDirection( airfGravVec.NormalizeToVector3() );

            return upDir;
        }

        void UpdateOrientation()
        {
            Vector3 upDir = GetUpDir();
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

                if( UnityEngine.Input.GetKeyDown( KeyCode.Mouse1 ) ) // Mouse1 = Right Mouse Button
                {
                    _isRotating = true;
                }
            }

            if( _isRotating && UnityEngine.Input.GetKeyUp( KeyCode.Mouse1 ) )
            {
                _isRotating = false;
            }

            if( _isRotating )
            {
                UpdateOrientation();
            }
        }

        void LateUpdate()
        {
            if( ReferenceObject != null ) // Raycasts using rays from the camera fail when the vessel is moving fast, but updating the camera earlier as well as later doesn't fix it.
            {
                this.transform.position = ReferenceObject.transform.position;
            }
        }

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_VANILLA + "camera.snap_to_vessel" )]
        private static void SnapToActiveObject( object e )
        {
            if( ActiveObjectManager.ActiveObject == null )
                instance.ReferenceObject = null;
            else
                instance.ReferenceObject = ActiveObjectManager.ActiveObject.transform;

            instance.transform.rotation = Quaternion.LookRotation( instance.transform.forward, instance.GetUpDir() );
        }
    }
}