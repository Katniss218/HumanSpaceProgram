using HSP.Vanilla.Scenes.GameplayScene;
using UnityEngine;

namespace HSP._DevUtils
{
    public class AIRFRotationFixer : MonoBehaviour
    {
        private void Update()
        {
            if( GameplaySceneReferenceFrameManager.ReferenceFrame != null )
                this.transform.rotation = (Quaternion)GameplaySceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( QuaternionDbl.identity );
        }
    }
}