using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.DesignScene
{
    /// <summary>
    /// Invoked immediately after loading the design scene.
    /// </summary>
    public static class HSPEvent_SCENELOAD_DESIGN
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the design scene.
    /// </summary>
    public static class HSPEvent_SCENEUNLOAD_DESIGN
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.unload";
    }

    /// <summary>
    /// Invoked immediately after the design scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEACTIVATE_DESIGN
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.activate";
    }

    /// <summary>
    /// Invoked immediately before the design scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEDEACTIVATE_DESIGN
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `design` scene.
    /// </summary>
    public sealed class DesignSceneManager : HSPSceneManager<DesignSceneManager>
    {
        public static new string UNITY_SCENE_NAME => "Design";

        public static DesignSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        //void Awake()
        //{
        //    HSPEvent.EventManager.TryInvoke( HSPEvent_SCENELOAD_DESIGN.ID );
        //}

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENELOAD_DESIGN.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEUNLOAD_DESIGN.ID );
        }

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEACTIVATE_DESIGN.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEDEACTIVATE_DESIGN.ID );
        }
    }
}