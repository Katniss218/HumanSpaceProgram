using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Camera
{
    public class CameraController : MonoBehaviour
    {
        [field: SerializeField]
        public UnityEngine.Camera Camera { get; set; }

        [field: SerializeField]
        public Transform ReferenceObject { get; set; }

        [field: SerializeField]
        float zoomDist = 5;

        void Start()
        {

        }

        void Update()
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

            this.Camera.transform.localPosition = Vector3.back * zoomDist;

            if( Input.GetKey( KeyCode.Mouse1 ) ) // RMB
            {
                float mouseX = Input.GetAxis( "Mouse X" );
                float mouseY = Input.GetAxis( "Mouse Y" );

                this.transform.rotation *= Quaternion.AngleAxis( mouseX * 200 * Time.unscaledDeltaTime, Vector3.up );
                this.transform.rotation *= Quaternion.AngleAxis( -mouseY * 200 * Time.unscaledDeltaTime, Vector3.right );
            }
        }

        void LateUpdate()
        {
            if( ReferenceObject != null )
            {
                this.transform.position = ReferenceObject.transform.position;
            }
        }
    }
}