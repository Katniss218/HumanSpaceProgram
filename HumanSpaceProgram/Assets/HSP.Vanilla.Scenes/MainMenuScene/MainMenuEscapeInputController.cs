using HSP.Input;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    /// <summary>
    /// Controls the invocation of the `escape` / pause event in the `main menu` scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuEscapeInputController : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.STARTUP_MAINMENU, HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller" )]
        private static void CreateInstanceInScene()
        {
            MainMenuSceneManager.Instance.gameObject.AddComponent<MainMenuEscapeInputController>();
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_ESCAPE, HierarchicalInputPriority.MEDIUM, Input_Escape );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.COMMON_ESCAPE, Input_Escape );
        }

        private bool Input_Escape( float value )
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_MAINMENU, null );
            return false;
        }
    }
}
