using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;
using HSP.Vanilla.UI.Vessels;
using HSP.Vanilla.UI.Vessels.Construction;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    public static class OnStartup
    {
        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, HSPEvent.NAMESPACE_HSP + ".add_vessel_hud_manager" )]
        private static void AddVesselHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<VesselHUDManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, HSPEvent.NAMESPACE_HSP + ".add_construction_site_hud_manager" )]
        private static void ConstructionSiteHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, HSPEvent.NAMESPACE_HSP + ".add_click_controller" )]
        private static void AddViewportClickController()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplayViewportClickController>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, HSPEvent.NAMESPACE_HSP + ".add_navball_render_texture_manager" )]
        private static void AddNavballRenderTextureManager()
        {
            var manager = GameplaySceneManager.GameObject.AddComponent<NavballRenderTextureManager>();

            NavballRenderTextureManager.ResetAttitudeIndicatorRT();
            NavballRenderTextureManager.CreateNavball();
            NavballRenderTextureManager.CreateNavballCamera();
        }
    }
}