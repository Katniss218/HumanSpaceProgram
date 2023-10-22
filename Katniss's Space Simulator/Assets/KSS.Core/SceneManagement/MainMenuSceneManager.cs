using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A manager that is active in the main menu.
    /// </summary>
    public class MainMenuSceneManager : MonoBehaviour
    {
        public const string SCENE_NAME = "MainMenu";

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_MAINMENU );
        }
    }
}