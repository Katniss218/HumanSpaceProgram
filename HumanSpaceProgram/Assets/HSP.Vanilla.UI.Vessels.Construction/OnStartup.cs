using HSP.Vanilla.Scenes.GameplayScene;

namespace HSP.Vanilla.UI.Vessels.Construction
{
    public static class OnStartup
    {
        public const string ADD_CONSTRUCTION_SITE_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_construction_site_hud_manager";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_CONSTRUCTION_SITE_HUD_MANAGER )]
        private static void ConstructionSiteHudManager()
        {
            GameplayScene.GameObject.AddComponent<ConstructionSiteHUDManager>();
        }
    }
}