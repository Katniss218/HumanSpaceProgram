using HSP.Vanilla.Scenes.GameplayScene;

namespace HSP.Vanilla.UI.Vessels
{
    public static class OnStartup
    {
        public const string ADD_VESSEL_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_vessel_hud_manager";

        [HSPEventListener( HSPEvent_STARTUP_GAMEPLAY.ID, ADD_VESSEL_HUD_MANAGER )]
        private static void AddVesselHudManager()
        {
            GameplaySceneManager.GameObject.AddComponent<VesselHUDManager>();
        }
    }
}