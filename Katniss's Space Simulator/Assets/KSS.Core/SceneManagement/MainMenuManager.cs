using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A manager that is active in the main menu.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        public const string MAIN_MENU_SCENE_NAME = "MainMenu";

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_MAINMENU );
        }
    }
}