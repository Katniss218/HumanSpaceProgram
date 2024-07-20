using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string ADD_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller";

        [HSPEventListener( HSPEvent_STARTUP_MAIN_MENU.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void AddEscapeInputController()
        {
            MainMenuSceneManager.Instance.gameObject.AddComponent<MainMenuEscapeInputController>();
        }
    }
}