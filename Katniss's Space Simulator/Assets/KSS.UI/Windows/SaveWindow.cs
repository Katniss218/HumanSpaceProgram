using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;
using KSS.Core;
using UnityEngine.UI;
using UnityPlus.AssetManagement;

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
            (GameObject uiGO, RectTransform rootRT) = UIHelper.CreateUI( (UIElement)CanvasManager.Get( CanvasName.WINDOWS ).transform, "part window", new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) ) );

            SaveWindow window = uiGO.AddComponent<SaveWindow>();

            UIScrollView scrollView = ((UIElement)rootRT).AddScrollView( UILayoutInfo.Fill(), new Vector2(0, 300), false, true );

            ((UIElement)rootRT).AddButton( new UILayoutInfo( Vector2.right, new Vector2( -2, 5 ), new Vector2( 95, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/ui_button_biaxial" ) );

            return window;
        }
    }
}