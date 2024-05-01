using KSS.Core;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace KSS.DesignScene
{
    /// <summary>
    /// Controls the invocation of the `escape` / pause event in the `design` scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class DesignSceneEscapeInputController : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.STARTUP_DESIGN, HSPEvent.NAMESPACE_VANILLA + ".add_escape_icontroller" )]
        private static void CreateInstanceInScene()
        {
            DesignSceneManager.Instance.gameObject.AddComponent<DesignSceneEscapeInputController>();
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
            HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_DESIGN, null );
            return false;
        }
    }
}