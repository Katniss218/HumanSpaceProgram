
namespace HSP.Vanilla.Scenes.DesignScene.Tools
{
    public static class VanillaToolRegisterer
    {
        public const string REGISTER_TOOLS = HSPEvent.NAMESPACE_HSP + "register_designtools";

        [HSPEventListener( HSPEvent_STARTUP_DESIGN.ID, REGISTER_TOOLS )]
        private static void RegisterTool()
        {
            DesignSceneToolManager.RegisterTool<PickTool>();
            DesignSceneToolManager.RegisterTool<TranslateTool>();
            DesignSceneToolManager.RegisterTool<RotateTool>();
            DesignSceneToolManager.RegisterTool<RerootTool>();
        }
    }
}