﻿using HSP.Core;
using HSP.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.GameplayScene
{
    /// <summary>
    /// Controls the invocation of the `escape` / pause event in the `gameplay` scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplaySceneEscapeInputController : MonoBehaviour
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_escape_icontroller" )]
        private static void CreateInstanceInScene()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplaySceneEscapeInputController>();
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
            if( !TimeStepManager.LockTimescale )
            {
                if( TimeStepManager.IsPaused )
                {
                    TimeStepManager.Unpause();
                    HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                }
                else
                {
                    TimeStepManager.Pause();
                    HSPEvent.EventManager.TryInvoke( HSPEvent.ESCAPE_GAMEPLAY, null );
                }
            }
            return false;
        }
    }
}