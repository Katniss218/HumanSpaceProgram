using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class LoadWindow : MonoBehaviour
    {
        public static LoadWindow Create()
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            LoadWindow loadWindow = window.gameObject.AddComponent<LoadWindow>();

            return loadWindow;
        }
    }
}