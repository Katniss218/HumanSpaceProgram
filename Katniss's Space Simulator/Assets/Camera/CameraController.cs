using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Camera
{
    public class CameraController : MonoBehaviour
    {
        [field: SerializeField]
        public Transform CameraParent { get; set; }

        [field: SerializeField]
        UnityEngine.Camera _closeCamera { get; set; }

        [field: SerializeField]
        UnityEngine.Camera _farCamera { get; set; }

        [field: SerializeField]
        public Transform ReferenceObject { get; set; }

        [field: SerializeField]
        float zoomDist = 5;

        void PreventViewFrustumException()
        {
            // For some reason, at the distance of around Earth's radius, having the near camera enabled throws "position our of view frustum" exceptions.
            if( zoomDist > 1_000_000 ) // should be enough of a conservative value. Near cam is only 100km, not 1000.
            {
                this._closeCamera.enabled = false;
            }
            else
            {
                this._closeCamera.enabled = true;
            }
        }

        void LateUpdate()
        {
            if( Input.mouseScrollDelta.y > 0 )
            {
                zoomDist -= zoomDist * 15.0f * Time.unscaledDeltaTime;
            }
            if( Input.mouseScrollDelta.y < 0 )
            {
                zoomDist += zoomDist * 15.0f * Time.unscaledDeltaTime;
            }
            if( zoomDist < 2 )
            {
                zoomDist = 2;
            }

            CameraParent.transform.localPosition = Vector3.back * zoomDist;

            if( Input.GetKey( KeyCode.Mouse1 ) ) // RMB
            {
                float mouseX = Input.GetAxis( "Mouse X" );
                float mouseY = Input.GetAxis( "Mouse Y" );

                this.transform.rotation *= Quaternion.AngleAxis( mouseX * 500 * Time.unscaledDeltaTime, Vector3.up );
                this.transform.rotation *= Quaternion.AngleAxis( -mouseY * 500 * Time.unscaledDeltaTime, Vector3.right );
            }

            PreventViewFrustumException();

            if( ReferenceObject != null )
            {
                this.transform.position = ReferenceObject.transform.position;
            }
        }
    }
}