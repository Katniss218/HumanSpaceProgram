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
    public class GameplaySceneManager : SingletonMonoBehaviour<GameplaySceneManager>
    {
        public const string SCENE_NAME = "Testing And Shit"; // TODO - swap out for "Gameplay" when the part with creating and loading rockets is done.

        public static GameplaySceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_GAMEPLAY );
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
            if( !TimeManager.LockTimescale )
            {
                if( TimeManager.IsPaused )
                {
                    TimeManager.Unpause();
                    HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                }
                else
                {
                    TimeManager.Pause();
                    HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                }
            }
            return false;
        }
    }
}