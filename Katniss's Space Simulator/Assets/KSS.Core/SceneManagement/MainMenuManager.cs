using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A manager that is active in the main menu.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_MAINMENU );
        }
    }
}