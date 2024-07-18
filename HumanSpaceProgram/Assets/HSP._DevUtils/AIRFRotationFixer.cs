using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP._DevUtils
{
    public class AIRFRotationFixer : MonoBehaviour
    {
        private void Update()
        {
            if( SceneReferenceFrameManager.SceneReferenceFrame != null )
                this.transform.rotation = (Quaternion)SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformRotation( QuaternionDbl.identity );
        }
    }
}