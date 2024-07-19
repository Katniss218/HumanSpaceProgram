
namespace HSP.Vanilla.Scenes.GameplayScene.Tools
{
    public static class VanillaToolRegisterer
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "gameplaytools.vanilla.register" )]
        private static void RegisterTool( object e )
        {
            GameplaySceneToolManager.RegisterTool<DefaultTool>();
            GameplaySceneToolManager.RegisterTool<ConstructTool>();
        }
    }
}