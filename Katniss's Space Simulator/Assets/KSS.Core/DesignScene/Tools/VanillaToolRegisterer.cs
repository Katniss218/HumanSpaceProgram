using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.DesignScene.Tools
{
    public static class VanillaToolRegisterer
    {
        [HSPEventListener( HSPEvent.STARTUP_DESIGN, "designtools.vanilla.register" )]
        private static void RegisterTool( object e )
        {
            DesignSceneToolManager.RegisterTool<PickTool>();
            DesignSceneToolManager.RegisterTool<TranslateTool>();
            DesignSceneToolManager.RegisterTool<RotateTool>();
            DesignSceneToolManager.RegisterTool<RerootTool>();
        }
    }
}