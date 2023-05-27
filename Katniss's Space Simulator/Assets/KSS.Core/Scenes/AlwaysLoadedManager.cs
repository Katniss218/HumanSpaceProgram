using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// This manager is loaded first and always remains loaded until the game is exited.
    /// </summary>
    public class AlwaysLoadedManager : MonoBehaviour
    {
        void Awake()
        {
            // first scene should be the always loaded scene, so load the main menu.

            Scenes.SceneManager.Instance.LoadScene( "MainMenu", true, false, null );
        }
    }
}