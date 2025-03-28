using HSP.Vanilla.Scenes.GameplayScene;

namespace HSP.Vanilla.UI.Vessels.Construction
{
    public static class OnStartup
    {
        public const string ADD_CONSTRUCTION_SITE_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_construction_site_hud_manager";

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_CONSTRUCTION_SITE_HUD_MANAGER )]
        private static void ConstructionSiteHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }
    }
}