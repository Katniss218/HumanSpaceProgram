using System;
using System.Collections;
using System.Collections.Generic;
using UILib;
using UnityEngine;

namespace KSS.Core
{
    public class MainMenuManager : MonoBehaviour
    {
#warning TODO - turn MainMenuManager into a singleton with the editor-assigned fields.

        [SerializeField]
        UIStyle _style;

        void Awake()
        {
            HSPOverridableEvent.EventManager.TryInvoke( HSPOverridableEvent.STARTUP_MAINMENU, (_style, _style) );
        }
    }
}