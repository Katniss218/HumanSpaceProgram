using KSS.Core;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace KSS.EditorScene
{
    /// <summary>
    /// Controls the invocation of the `escape` / pause event in the `editor` scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class EditorSceneEscapeInputController : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.STARTUP_EDITOR, HSPEvent.NAMESPACE_VANILLA + ".add_escape_icontroller" )]
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

        private bool Input_Escape()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_EDITOR, null );
            return false;
        }
    }
}
