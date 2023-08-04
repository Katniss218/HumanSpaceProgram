using System;
using System.Collections;
using System.Collections.Generic;
using UILib;
using UnityEngine;

namespace KSS.Core
{
    public class MainMenuManager : MonoBehaviour
    {
        void Awake()
        {
            HSPOverridableEvent.EventManager.TryInvoke( HSPOverridableEvent.STARTUP_MAINMENU );
        }
    }
}