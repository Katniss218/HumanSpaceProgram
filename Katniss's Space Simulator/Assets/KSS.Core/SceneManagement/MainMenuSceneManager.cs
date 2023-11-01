using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    /// <summary>
    /// A manager that is active in the main menu.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
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