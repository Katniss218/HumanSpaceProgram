using HSP.Time;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string UNPAUSE = HSPEvent.NAMESPACE_HSP + ".unpause";

        [HSPEventListener( HSPEvent_SCENELOAD_MAIN_MENU.ID, UNPAUSE )]
        private static void Unpause()
        {
            TimeManager.Unpause();
        }

        public const string ADD_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller";

        [HSPEventListener( HSPEvent_SCENELOAD_MAIN_MENU.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void AddEscapeInputController()
        {
            MainMenuSceneManager.Instance.gameObject.AddComponent<MainMenuEscapeInputController>();
        }
    }
}