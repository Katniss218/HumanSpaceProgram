using UnityEngine;

namespace HSP.Vanilla.Scenes.EditorScene
{
    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `editor` scene.
    /// </summary>
    public class EditorSceneManager : SingletonMonoBehaviour<EditorSceneManager>
    {
        public const string SCENE_NAME = "Editor";

        public static EditorSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_DESIGN );
        }
    }
}