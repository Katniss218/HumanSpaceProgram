
namespace HSP.Vanilla.Scenes.GameplayScene.Tools
{
    public static class VanillaToolRegisterer
    {
        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, "gameplaytools.vanilla.register" )]
        private static void RegisterTool( object e )
        {
            GameplaySceneToolManager.RegisterTool<DefaultTool>();
            GameplaySceneToolManager.RegisterTool<ConstructTool>();
        }
    }
}