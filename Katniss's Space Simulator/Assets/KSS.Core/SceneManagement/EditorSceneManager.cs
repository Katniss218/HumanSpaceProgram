using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace KSS.Core
{
    /// <summary>
    /// A Manager that is active in the gameplay scene.
    /// </summary>
    public class EditorSceneManager : MonoBehaviour
    {
        public const string SCENE_NAME = "Editor";

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