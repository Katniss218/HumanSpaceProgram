using System;
using System.Collections;
using System.Collections.Generic;
using UILib;
using UnityEngine;

namespace KSS.UI.SceneFactories
{
    public class MainMenuManager : MonoBehaviour
    {
        // This class doesn't really belong here, it's more of a Core thing, but we can't move it yet.
        // it relies on the implementation of `MainMenuUIFactory.Create` method, and without a mod loader (reflection), we can't make it assign a different one.
        [SerializeField]
        Canvas _mainMenuCanvas;

        [SerializeField]
        UIStyle _style;

        void Awake()
        {
            MainMenuUIFactory.Create( _mainMenuCanvas, _style );
        }
    }
}