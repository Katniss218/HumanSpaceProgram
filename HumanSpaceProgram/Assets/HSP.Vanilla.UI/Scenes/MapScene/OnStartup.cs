using HSP.Vanilla.Scenes.MapScene;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public static class OnStartup
    {
        public const string ADD_MAP_VESSEL_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_map_vessel_hud_manager";
        public const string ADD_MAP_CELESTIAL_BODY_HUD_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_map_celestial_body_hud_manager";

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_MAP_VESSEL_HUD_MANAGER )]
        private static void AddMapVesselHudManager()
        {
            MapSceneM.GameObject.AddComponent<MapVesselHUDManager>();
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_MAP_CELESTIAL_BODY_HUD_MANAGER )]
        private static void AddMapCelestialBodyHudManager()
        {
            MapSceneM.GameObject.AddComponent<MapCelestialBodyHUDManager>();
        }
    }
}
