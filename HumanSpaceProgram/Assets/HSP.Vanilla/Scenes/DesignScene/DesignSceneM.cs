using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.DesignScene
{
    /// <summary>
    /// Invoked immediately after loading the design scene.
    /// </summary>
    public static class HSPEvent_DESIGN_SCENE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".design_scene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the design scene.
    /// </summary>
    public static class HSPEvent_DESIGN_SCENE_UNLOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".design_scene.unload";
    }

    /// <summary>
    /// Invoked immediately after the design scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_DESIGN_SCENE_ACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".design_scene.activate";
    }

    /// <summary>
    /// Invoked immediately before the design scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_DESIGN_SCENE_DEACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".design_scene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `design` scene.
    /// </summary>
    public sealed class DesignSceneM : HSPScene<DesignSceneM>
    {
        public static new string UNITY_SCENE_NAME => "Design";

        public static DesignSceneM Instance => instance;
        public static GameObject GameObject => instance.gameObject;


        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_DESIGN_SCENE_LOAD.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_DESIGN_SCENE_UNLOAD.ID );
        }

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_DESIGN_SCENE_ACTIVATE.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_DESIGN_SCENE_DEACTIVATE.ID );
        }
    }
}