using HSP.Input;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.EditorScene
{
    /// <summary>
    /// Invoked when the player toggles the escape (pause) menu in the editor scene.
    /// </summary>
    public static class HSPEvent_ON_ESCAPE_EDITOR
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".escape.editor";
    }

    /// <summary>
    /// Controls the invocation of the `escape` / pause event in the `editor` scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class EditorSceneEscapeInputController : MonoBehaviour
    {
        [HSPEventListener( HSPEvent_STARTUP_EDITOR.ID, HSPEvent.NAMESPACE_HSP + ".add_escape_icontroller" )]
        private static void CreateInstanceInScene()
        {
            EditorSceneManager.Instance.gameObject.AddComponent<EditorSceneEscapeInputController>();
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
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_ESCAPE_EDITOR.ID, null );
            return false;
        }
    }
}