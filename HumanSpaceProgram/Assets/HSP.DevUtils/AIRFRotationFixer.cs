using HSP.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.DevUtils
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