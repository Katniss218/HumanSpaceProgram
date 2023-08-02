using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UILib;

namespace KSS.UI
{
    public class SaveWindow : MonoBehaviour
    {
        //

        /// <summary>
        /// Creates a save window with the current context.
        /// </summary>
        public static SaveWindow Create()
        {
            GameObject uiGO = UIHelper.UI( CanvasManager.GetCanvas( CanvasManager.WINDOWS ).transform, "part window", new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) );

            SaveWindow window = uiGO.AddComponent<SaveWindow>();

            return window;
        }
    }
}