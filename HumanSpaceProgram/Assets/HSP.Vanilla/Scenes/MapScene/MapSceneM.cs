using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    /// <summary>
    /// Invoked immediately after loading the map scene.
    /// </summary>
    public static class HSPEvent_MAP_SCENE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_scene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the map scene.
    /// </summary>
    public static class HSPEvent_MAP_SCENE_UNLOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_scene.unload";
    }

    /// <summary>
    /// Invoked immediately after the map scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_MAP_SCENE_ACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_scene.activate";
    }

    /// <summary>
    /// Invoked immediately before the map scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_MAP_SCENE_DEACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_scene.deactivate";
    }

    public class MapSceneM : HSPScene<MapSceneM>
    {
        public static new string UNITY_SCENE_NAME => null; 

        public static MapSceneM Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAP_SCENE_LOAD.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAP_SCENE_UNLOAD.ID );
        }

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAP_SCENE_ACTIVATE.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAP_SCENE_DEACTIVATE.ID );
        }
    }
}