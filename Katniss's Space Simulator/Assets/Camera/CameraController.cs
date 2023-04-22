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

        // Start is called before the first frame update
        void Start()
        {

        }

        void Update()
        {
            Vector3 pos = this.Camera.transform.localPosition;
            if( Input.mouseScrollDelta.y > 0 )
            {
                pos -= pos * 15.0f * Time.deltaTime;
            }
            if( Input.mouseScrollDelta.y < 0 )
            {
                pos += pos * 15.0f * Time.deltaTime;
            }
            this.Camera.transform.localPosition = pos;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if( ReferenceObject != null )
            {
                this.transform.position = ReferenceObject.transform.position;
            }
        }
    }
}