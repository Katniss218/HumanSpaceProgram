using HSP.Input;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    /// <summary>
    /// Invoked when the player toggles the escape (pause) menu in the main menu scene.
    /// </summary>
    public static class HSPEvent_ON_ESCAPE_MAIN_MENU
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".escape.mainmenu";
    }

    /// <summary>
    /// Controls the invocation of the `escape` / pause event in the `main menu` scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuEscapeInputController : MonoBehaviour
    {
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
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_ESCAPE_MAIN_MENU.ID, null );
            return false;
        }
    }
}
