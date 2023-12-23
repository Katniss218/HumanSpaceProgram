using KSS.Core.Input;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;

namespace KSS.Core
{
    /// <summary>
    /// A Manager that is active in the gameplay scene.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
    public class EditorSceneManager : SingletonMonoBehaviour<EditorSceneManager>
    {
        public const string SCENE_NAME = "Editor";

        public static EditorSceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_DESIGN );
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
            HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_DESIGN, null );
            return false;
        }
    }
}