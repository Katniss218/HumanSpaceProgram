using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A Manager that is active in the gameplay scene.
    /// </summary>
    public class GameplaySceneManager : MonoBehaviour
    {
        public const string SCENE_NAME = "Testing And Shit"; // TODO - swap out for "Gameplay" when the part with creating and loading rockets is done.

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_GAMEPLAY );
        }

        bool toggle = false;

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.Escape ) )
            {
                if( !TimeManager.LockTimescale )
                {
                    toggle = !toggle;
                    if( toggle )
                    {
                        TimeManager.Pause();
                        HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                    }
                    else
                    {
                        TimeManager.Unpause();
                        HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                    }
                }
            }
        }
    }
}