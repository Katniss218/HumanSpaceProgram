using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP._DevUtils
{
    public class AIRFRotationFixer : MonoBehaviour
    {
        private void Update()
        {
            if( SceneReferenceFrameManager.ReferenceFrame != null )
                this.transform.rotation = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( QuaternionDbl.identity );
        }
    }
}