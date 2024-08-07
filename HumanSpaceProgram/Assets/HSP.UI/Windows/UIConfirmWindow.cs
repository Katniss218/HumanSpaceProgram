﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.Windows
{
    public class UIConfirmWindow : UIWindow
    {
        protected internal static T Create<T>( UICanvas parent, UILayoutInfo layoutInfo, string title, string text, Action onSubmit ) where T : UIConfirmWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, layoutInfo, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
            .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), title )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Fill( 2, 2, 32, 19 ) ), text );

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

    public static class UIConfirmWindow_Ex
    {
        public static UIConfirmWindow AddConfirmWindow( this UICanvas parent, string title, string text, Action onSubmit )
        {
            return UIConfirmWindow.Create<UIConfirmWindow>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (250, 100) ), title, text, onSubmit );
        }
    }
}