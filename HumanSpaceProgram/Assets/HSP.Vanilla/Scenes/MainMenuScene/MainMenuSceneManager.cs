using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    /// <summary>
    /// Invoked immediately after loading the main menu scene.
    /// </summary>
    public static class HSPEvent_SCENELOAD_MAIN_MENU
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenuscene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the main menu scene.
    /// </summary>
    public static class HSPEvent_SCENEUNLOAD_MAIN_MENU
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenuscene.unload";
    }

    /// <summary>
    /// Invoked immediately after the main menu scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEACTIVATE_MAIN_MENU
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenuscene.activate";
    }

    /// <summary>
    /// Invoked immediately before the main menu scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEDEACTIVATE_MAIN_MENU
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenuscene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `main menu` scene.
    /// </summary>
    public class MainMenuSceneManager : HSPSceneManager<MainMenuSceneManager>
    {
        public static new string UNITY_SCENE_NAME => "MainMenu";

        public static MainMenuSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        //void Awake()
        //{
        //    HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_MAIN_MENU.ID );
        //}

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENELOAD_MAIN_MENU.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEUNLOAD_MAIN_MENU.ID );
        }

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEACTIVATE_MAIN_MENU.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEDEACTIVATE_MAIN_MENU.ID );
        }
    }
}