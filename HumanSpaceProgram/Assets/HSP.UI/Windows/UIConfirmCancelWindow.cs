using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.Windows
{
    /// <summary>
    /// 'confirm' or 'cancel' window.
    /// </summary>
    public class UIConfirmCancelWindow : UIWindow
    {
        protected internal static T Create<T>( UICanvas parent, UILayoutInfo layoutInfo, string title, string text, Action onSubmit ) where T : UIConfirmCancelWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, layoutInfo, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
            .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), title )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Fill( 8, 8, 32, 19 ) ), text );

            uiWindow.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (-2 - 60 - 2, 2), (60, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
            {
                uiWindow.Destroy();
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Cancel" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );

            uiWindow.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (-2, 2), (60, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
            {
                onSubmit?.Invoke();
                uiWindow.Destroy();
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Confirm" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );

            return uiWindow;
        }
    }

    public static class UIConfirmCancelWindow_Ex
    {
        public static UIConfirmCancelWindow AddConfirmCancelWindow( this UICanvas parent, string title, string text, Action onSubmit )
        {
            return UIConfirmCancelWindow.Create<UIConfirmCancelWindow>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (300, 120) ), title, text, onSubmit );
        }
    }
}