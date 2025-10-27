using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.Windows
{
    public class UIPromptWindow : UIWindow
    {
        protected internal static T Create<T>( UICanvas parent, UILayoutInfo layoutInfo, string title, string placeholder, Action<string> onSubmit ) where T : UIPromptWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, layoutInfo, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
            .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), title )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            UIInputField<string> input = uiWindow.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 2, 2 ), UIAnchor.Top, -32, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
                .WithPlaceholder( placeholder )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            uiWindow.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (-2, 2), (60, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
            {
                onSubmit?.Invoke( input.GetOrDefault( "" ) );
                uiWindow.Destroy();
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Confirm" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );

            return uiWindow;
        }
    }

    public static class UIPromptWindow_Ex
    {
        public static UIPromptWindow AddPromptWindow( this UICanvas parent, string title, string placeholder, Action<string> onSubmit )
        {
            return UIPromptWindow.Create<UIPromptWindow>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (300, 120) ), title, placeholder, onSubmit );
        }
    }
}