using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.EditorScene
{
    /// <summary>
    /// Invoked immediately after loading the editor scene.
    /// </summary>
    public static class HSPEvent_EDITOR_SCENE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editor_scene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the editor scene.
    /// </summary>
    public static class HSPEvent_EDITOR_SCENE_UNLOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editor_scene.unload";
    }

    /// <summary>
    /// Invoked immediately after the editor scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_EDITOR_SCENE_ACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editor_scene.activate";
    }

    /// <summary>
    /// Invoked immediately before the editor scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_EDITOR_SCENE_DEACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editor_scene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `editor` scene.
    /// </summary>
    public sealed class EditorSceneM : HSPScene<EditorSceneM>
    {
        public static new string UNITY_SCENE_NAME => "Editor";

        public static EditorSceneM Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_EDITOR_SCENE_LOAD.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_EDITOR_SCENE_UNLOAD.ID );
        }

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_EDITOR_SCENE_ACTIVATE.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_EDITOR_SCENE_DEACTIVATE.ID );
        }
    }
}