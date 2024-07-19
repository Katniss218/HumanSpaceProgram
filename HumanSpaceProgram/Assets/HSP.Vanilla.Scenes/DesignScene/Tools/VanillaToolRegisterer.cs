
using HSP.Vanilla.Scenes.EditorScene;

namespace HSP.Vanilla.Scenes.DesignScene.Tools
{
    public static class VanillaToolRegisterer
    {
        [HSPEventListener( HSPEvent_STARTUP_DESIGN.ID, "designtools.vanilla.register" )]
        private static void RegisterTool()
        {
            DesignSceneToolManager.RegisterTool<PickTool>();
            DesignSceneToolManager.RegisterTool<TranslateTool>();
            DesignSceneToolManager.RegisterTool<RotateTool>();
            DesignSceneToolManager.RegisterTool<RerootTool>();
        }
    }
}