using Assets.HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.DesignScene
{
    /// <summary>
    /// Invoked immediately after loading the design scene.
    /// </summary>
    public static class HSPEvent_STARTUP_DESIGN
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.design";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `design` scene.
    /// </summary>
    public class DesignSceneManager : SceneManager<DesignSceneManager>
    {
        public const string SCENE_NAME = "Design";

        public static DesignSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_DESIGN.ID );
        }
    }
}