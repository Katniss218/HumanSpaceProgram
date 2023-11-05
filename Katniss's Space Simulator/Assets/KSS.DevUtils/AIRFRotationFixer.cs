using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.DevUtils
{
    public class AIRFRotationFixer : MonoBehaviour
    {
        private void Update()
        {
            this.transform.rotation = (Quaternion)SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformRotation( QuaternionDbl.identity );
        }
    }
}