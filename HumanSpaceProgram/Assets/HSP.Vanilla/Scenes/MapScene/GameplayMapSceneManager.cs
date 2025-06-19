using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    // Anything that wishes to display something in the map scene can hook into these...

    public static class HSPEvent_ON_MAP_OPENED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mapscene.open";
    }
    public static class HSPEvent_ON_MAP_CLOSED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mapscene.closed";
    }

    public class GameplayMapSceneManager : SingletonMonoBehaviour<GameplayMapSceneManager>
    {
        public static void OpenMap()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_MAP_OPENED.ID );
        }

        public static void CloseMap()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_MAP_CLOSED.ID );
        }
    }
}