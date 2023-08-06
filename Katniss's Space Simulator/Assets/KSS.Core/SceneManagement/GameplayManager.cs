using KSS.Core.TimeWarp;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A Manager that is active in the gameplay scene.
    /// </summary>
    public class GameplayManager : MonoBehaviour
    {
        public const string GAMEPLAY_SCENE_NAME = "Testing And Shit"; // TODO - swap out for "Gameplay" when the save system is working.

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_GAMEPLAY );
        }

        bool toggle = false;

        void Update()
        {
            if( Input.GetKeyDown( KeyCode.Escape ) )
            {
                if( !TimeWarpManager.LockTimescale )
                {
                    toggle = !toggle;
                    if( toggle )
                    {
                        TimeWarpManager.Pause();
                        HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                    }
                    else
                    {
                        TimeWarpManager.Unpause();
                        HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                    }
                }
            }
        }
    }
}