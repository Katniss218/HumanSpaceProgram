using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;
using HSP.Vanilla.UI.Vessels;
using HSP.Vanilla.UI.Vessels.Construction;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    public static class OnStartup
    {
        public const string ADD_VESSEL_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_vessel_hud_manager";
        public const string ADD_CONSTRUCTION_SITE_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_construction_site_hud_manager";
        public const string ADD_VIEWPORT_CLICK_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_click_controller";
        public const string ADD_NAVBALL_RENDER_TEXTURE_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_navball_render_texture_manager";

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_VESSEL_HUD_MANAGER )]
        private static void AddVesselHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<VesselHUDManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_CONSTRUCTION_SITE_HUD_MANAGER )]
        private static void ConstructionSiteHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_VIEWPORT_CLICK_CONTROLLER )]
        private static void AddViewportClickController()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplayViewportClickController>();
        }

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_NAVBALL_RENDER_TEXTURE_MANAGER )]
        private static void AddNavballRenderTextureManager()
        {
            var manager = GameplaySceneManager.GameObject.AddComponent<NavballRenderTextureManager>();

            NavballRenderTextureManager.ResetAttitudeIndicatorRT();
            NavballRenderTextureManager.CreateNavball();
            NavballRenderTextureManager.CreateNavballCamera();
        }
    }
}