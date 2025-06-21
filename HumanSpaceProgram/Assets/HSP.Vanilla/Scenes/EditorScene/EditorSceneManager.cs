using Assets.HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.EditorScene
{
    /// <summary>
    /// Invoked immediately after loading the design scene.
    /// </summary>
    public static class HSPEvent_STARTUP_EDITOR
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.editor";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `editor` scene.
    /// </summary>
    public class EditorSceneManager : SceneManager<EditorSceneManager>
    {
        public const string SCENE_NAME = "Editor";

        public static EditorSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_EDITOR.ID );
        }
    }
}