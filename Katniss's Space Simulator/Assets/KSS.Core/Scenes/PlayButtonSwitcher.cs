using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.Core.Scenes
{
    public class PlayButtonSwitcher : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.Instance.UnloadScene( "MainMenu", () => SceneManager.Instance.LoadScene( "Testing And Shit", true, false, null ) );
        }
    }
}