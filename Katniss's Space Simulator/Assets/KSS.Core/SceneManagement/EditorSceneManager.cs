using UnityEngine;
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

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.Escape ) )
            {
                HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_DESIGN, null );
            }
        }
    }
}