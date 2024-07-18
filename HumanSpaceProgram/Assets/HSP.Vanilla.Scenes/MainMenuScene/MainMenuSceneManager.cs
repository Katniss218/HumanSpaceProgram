using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `main menu` scene.
    /// </summary>
    public class MainMenuSceneManager : SingletonMonoBehaviour<MainMenuSceneManager>
    {
        public const string SCENE_NAME = "MainMenu";

        public static MainMenuSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_MAINMENU );
        }
    }
}