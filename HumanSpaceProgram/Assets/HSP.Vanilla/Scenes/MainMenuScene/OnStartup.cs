using HSP.Time;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string UNPAUSE = HSPEvent.NAMESPACE_HSP + ".unpause";

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_LOAD.ID, UNPAUSE )]
        private static void Unpause()
        {
            TimeManager.Unpause();
        }

        public const string ADD_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller";

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_LOAD.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void AddEscapeInputController()
        {
            MainMenuSceneM.Instance.gameObject.AddComponent<MainMenuEscapeInputController>();
        }
    }
}