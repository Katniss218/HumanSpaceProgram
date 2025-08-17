using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    /// <summary>
    /// Invoned after the focused object in the map view has changed.
    /// </summary>
    public static class HSPEvent_AFTER_MAP_FOCUS_CHANGED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_scene.focus_changed";
    }

    public class MapFocusedObjectManager : SingletonMonoBehaviour<MapFocusedObjectManager>
    {
        IMapFocusable _focus;

        public static IMapFocusable FocusedObject
        {
            get => instance._focus;
            set
            {
                instance._focus = value;
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_MAP_FOCUS_CHANGED.ID, value );
            }
        }
    }
}