using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.EditorScene
{
    /// <summary>
    /// Invoked immediately after loading the editor scene.
    /// </summary>
    public static class HSPEvent_SCENELOAD_EDITOR
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editorscene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the editor scene.
    /// </summary>
    public static class HSPEvent_SCENEUNLOAD_EDITOR
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editorscene.unload";
    }

    /// <summary>
    /// Invoked immediately after the editor scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEACTIVATE_EDITOR
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editorscene.activate";
    }

    /// <summary>
    /// Invoked immediately before the editor scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEDEACTIVATE_EDITOR
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".editorscene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `editor` scene.
    /// </summary>
    public sealed class EditorSceneManager : HSPSceneManager<EditorSceneManager>
    {
        public static new string UNITY_SCENE_NAME => "Editor";

        public static EditorSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        //void Awake()
        //{
        //    HSPEvent.EventManager.TryInvoke( HSPEvent_SCENELOAD_EDITOR.ID );
        //}

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENELOAD_EDITOR.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEUNLOAD_EDITOR.ID );
        }

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEACTIVATE_EDITOR.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEDEACTIVATE_EDITOR.ID );
        }
    }
}