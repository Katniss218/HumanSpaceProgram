using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatnisssSpaceSimulator.Core.ReferenceFrames
{
    public interface IReferenceFrameSwitchResponder
    {
        void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data );
    }
}
