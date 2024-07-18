using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;
using HSP.Vanilla.UI.Construction;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    public static class OnStartup
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.csite_huds" )]
        private static void ConstructionSiteHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.spawn_navball" )]
        private static void OnGameplayEnter()
        {
            var manager = GameplaySceneManager.GameObject.AddComponent<NavballRenderTextureManager>();

            NavballRenderTextureManager.ResetAttitudeIndicatorRT();
            NavballRenderTextureManager.CreateNavball();
            NavballRenderTextureManager.CreateNavballCamera();
        }
    }
}