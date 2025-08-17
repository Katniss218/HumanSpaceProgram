using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public static class HSPEvent_AFTER_MAP_VESSEL_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_vessel_created.after";
    }

    public static class HSPEvent_AFTER_MAP_VESSEL_DESTROYED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_vessel_destroyed.after";
    }

    public class MapVessel : MonoBehaviour, IMapFocusable
    {
        void Start()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_MAP_VESSEL_CREATED.ID, this );
        }

        void OnDestroy()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_MAP_VESSEL_DESTROYED.ID, this );
        }
    }
}