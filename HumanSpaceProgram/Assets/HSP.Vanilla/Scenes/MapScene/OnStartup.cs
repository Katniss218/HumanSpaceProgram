using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string ADD_MAP_FOCUSED_OBJECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_map_focused_object_manager";
        public const string ADD_SCENE_REFERENCE_FRAME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_scene_reference_frame_manager";
        public const string SET_FOCUS_TO_HOME_PLANET = HSPEvent.NAMESPACE_HSP + ".set_focus_to_home_planet";

        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_SCENE_REFERENCE_FRAME_MANAGER )]
        private static void AddSceneReferenceFrameManager()
        {
            MapSceneReferenceFrameManager.Instance = MapSceneM.Instance.gameObject.AddComponent<MapSceneReferenceFrameManager>();
            MapSceneReferenceFrameManager.Instance.MaxRelativePosition = 1e8f;
            MapSceneReferenceFrameManager.Instance.MaxRelativeVelocity = float.MaxValue;
        }
        [HSPEventListener( HSPEvent_MAP_SCENE_LOAD.ID, ADD_MAP_FOCUSED_OBJECT_MANAGER )]
        private static void AddMapFocusedObjectManager()
        {
            MapSceneM.Instance.gameObject.AddComponent<MapFocusedObjectManager>();
        }

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, SET_FOCUS_TO_HOME_PLANET, After = new[] { MapSceneCelestialBodyManager.CREATE_MAP_CELESTIAL_BODIES } )]
        private static void SetFocus()
        {
            if( MapSceneCelestialBodyManager.TryGet( "main", out var mapCelestialBody ) )
                MapFocusedObjectManager.FocusedObject = mapCelestialBody;
        }
    }
}