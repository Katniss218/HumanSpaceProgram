using UnityEngine;

namespace HSP.Vanilla.Scenes.EditorScene
{
    public class OnStartup : MonoBehaviour
    {
        public const string ADD_ESCAPE_INPUT_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller";

        [HSPEventListener( HSPEvent_STARTUP_EDITOR.ID, ADD_ESCAPE_INPUT_CONTROLLER )]
        private static void AddEscapeInputController()
        {
            EditorSceneManager.Instance.gameObject.AddComponent<EditorSceneEscapeInputController>();
        }
    }
}