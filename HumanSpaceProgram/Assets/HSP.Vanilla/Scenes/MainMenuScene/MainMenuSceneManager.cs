using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    /// <summary>
    /// Invoked immediately after loading the main menu scene.
    /// </summary>
    public static class HSPEvent_STARTUP_MAIN_MENU
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.mainmenu";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `main menu` scene.
    /// </summary>
    public class MainMenuSceneManager : HSPSceneManager<MainMenuSceneManager>
    {
        public static string UNITY_SCENE_NAME = "MainMenu";

        public static MainMenuSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_MAIN_MENU.ID );
        }

        protected override void OnActivate()
        {
        }

        protected override void OnDeactivate()
        {
        }

        protected override void OnLoad()
        {
        }

        protected override void OnUnload()
        {
        }
    }
}