using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.DesignScene.Tools
{
    public static class VanillaToolRegisterer
    {
        [HSPEventListener( HSPEvent.STARTUP_DESIGN, "designtools.vanilla.register" )]
        private static void RegisterTool()
        {
            DesignSceneToolManager.RegisterTool<PickTool>();
            DesignSceneToolManager.RegisterTool<TranslateTool>();
            DesignSceneToolManager.RegisterTool<RotateTool>();
            DesignSceneToolManager.RegisterTool<RerootTool>();
        }
    }
}