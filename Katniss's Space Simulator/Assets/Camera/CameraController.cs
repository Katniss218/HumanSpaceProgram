using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace KatnisssSpaceSimulator.Camera
{
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// The camera will focus on this object.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceObject { get; set; }

        [field: SerializeField]
        public Transform CameraParent { get; set; }

        [field: SerializeField]
        UnityEngine.Camera _closeCamera;

        [field: SerializeField]
        UnityEngine.Camera _farCamera;

        [field: SerializeField]
        float zoomDist = 5;

        float? mapViewPreviousZoomDist = null;

        [field: SerializeField]
        PostProcessLayer _closePPLayer;

        [field: SerializeField]
        PostProcessLayer _farPPLayer; 

        void UpdateZoomLevel()
        {
            if( Input.mouseScrollDelta.y > 0 )
            {
                zoomDist -= zoomDist * 15.0f * Time.unscaledDeltaTime;
            }
            else if( Input.mouseScrollDelta.y < 0 )
            {
                zoomDist += zoomDist * 15.0f * Time.unscaledDeltaTime;
            }

            if( zoomDist < 2 )
            {
                zoomDist = 2;
            }

            // Toggle "map" - just zoom out for now, it's handy.
            if( Input.GetKeyDown( KeyCode.M ) ) // map view, kind of
            {
                if( mapViewPreviousZoomDist == null )
                {
                    mapViewPreviousZoomDist = zoomDist;
                    zoomDist = 25_000_000.0f;
                }
                else
                {
                    zoomDist = mapViewPreviousZoomDist.Value;
                    mapViewPreviousZoomDist = null;
                }
            }

            // helps to make the shadow look nicer.
            QualitySettings.shadowDistance = 2550.0f + 1.3f * zoomDist;

            // ---
            CameraParent.transform.localPosition = Vector3.back * zoomDist;
        }

        void UpdateOrientation()
        {
            if( Input.GetKey( KeyCode.Mouse1 ) ) // RMB
            {
                float mouseX = Input.GetAxis( "Mouse X" );
                float mouseY = Input.GetAxis( "Mouse Y" );

                this.transform.rotation *= Quaternion.AngleAxis( mouseX * 500 * Time.unscaledDeltaTime, Vector3.up );
                this.transform.rotation *= Quaternion.AngleAxis( -mouseY * 500 * Time.unscaledDeltaTime, Vector3.right );
            }
        }

        void PreventViewFrustumException()
        {
            // For some reason, at the distance of around Earth's radius, having the near camera enabled throws "position our of view frustum" exceptions.
            if( zoomDist > 1_000_000 ) // should be enough of a conservative value. Near cam is only 100km, not 1000.
            {
                this._closeCamera.enabled = false;
                this._closePPLayer.enabled = false;
                this._farPPLayer.enabled = true;
            }
            else
            {
                this._closeCamera.enabled = true;
                this._closePPLayer.enabled = true;
                this._farPPLayer.enabled = false;
            }
        }

        void Update()
        {
            UpdateZoomLevel();

            UpdateOrientation();
        }

        void LateUpdate()
        {
            // after modifying position/rotation/zoom.
            PreventViewFrustumException();

            if( ReferenceObject != null )
            {
                this.transform.position = ReferenceObject.transform.position;
            }
        }
    }
}