using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UILib;
using UILib.Factories;
using KSS.Core;
using UnityEngine.UI;

namespace KSS.UI
{
    public class SaveWindow : MonoBehaviour
    {
        //
#warning TODO - add buttons to gameplay scene (and a full factory for it like mainmenu as well)
        /// <summary>
        /// Creates a save window with the current context.
        /// </summary>
        public static SaveWindow Create()
        {
            KSSUIStyle style = (KSSUIStyle)UIStyleManager.Instance.Style;

            GameObject uiGO = UIHelper.UI( CanvasManager.GetCanvas( CanvasManager.WINDOWS ).transform, "part window", new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) );

            SaveWindow window = uiGO.AddComponent<SaveWindow>();

            GameObject goList = UIHelper.UIFill( uiGO.transform, "list", 2, 2, 30, 60 );

            GameObject content = UIHelper.AddScrollRect( goList, 300, false, true );

            (_, Button btn) = ButtonFactory.CreateTextXY( (RectTransform)uiGO.transform, "btn", "Button", new UILayoutInfo( Vector2.right, new Vector2( -2, 5 ), new Vector2( 95, 15 ) ), style );

            return window;
        }
    }
}