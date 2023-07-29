using KSS.Core.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// The <see cref="AlwaysLoadedManager"/> is loaded immediately and remains loaded until the game is exited.
    /// </summary>
    public class AlwaysLoadedManager : MonoBehaviour
    {
        void Awake()
        {
            // Load mods first, then cache, because mods might (will) define autorunning methods.
            ModLoader.LoadModAssemblies();

            OverridableEventListenerAttribute.CreateEventsForAutorunningMethods( AppDomain.CurrentDomain.GetAssemblies() );

            OverridableEvent.Instance.TryInvoke( OverridableEvent.STARTUP_IMMEDIATELY );
        }

        void Start()
        {
            // Load the main menu after every Awake has finished processing.
            // - The initial scene should be the "always loaded scene".
            Scenes.SceneManager.Instance.LoadScene( "MainMenu", true, false, null );
        }
    }
}