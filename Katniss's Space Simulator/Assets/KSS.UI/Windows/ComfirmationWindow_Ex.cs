using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    public static class ComfirmationWindow_Ex
    {
        // a simple generic confirmation window that lets you pass an action to call when OK is clicked.

        public static UIWindow AddComfirmationWindow( this UICanvas canvas, string title, string text, Action onClick )
        {
            UIWindow window = canvas.AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
           .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            window.AddText( UILayoutInfo.FillHorizontal( 0, 0, 1f, 0, 30 ), title )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddText( UILayoutInfo.Fill( 2, 2, 32, 19 ), text )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddButton( new UILayoutInfo( Vector2.right, new Vector2( -2, 2 ), new Vector2( 60, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
            {
                onClick?.Invoke();
                window.Destroy();
            } )
                .AddText( UILayoutInfo.Fill(), "Confirm" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            return window;
        }
    }

    public static class TextSelectionWindow_Ex
    {
        // a simple generic selection window that lets you pass the contents of an input field to an action to call when Confirm is clicked.
        
        public static UIWindow AddTextSelectionWindow( this UICanvas canvas, string title, string placeholder, Action<string> onClick )
        {
            UIWindow window = canvas.AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
           .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            window.AddText( UILayoutInfo.FillHorizontal( 0, 0, 1f, 0, 30 ), title )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UIInputField input = window.AddInputField( UILayoutInfo.FillHorizontal( 2, 2, 1f, -32, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
                .WithPlaceholder( placeholder )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddButton( new UILayoutInfo( Vector2.right, new Vector2( -2, 2 ), new Vector2( 60, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
            {
                onClick?.Invoke( input.Text );
                window.Destroy();
            } )
                .AddText( UILayoutInfo.Fill(), "Confirm" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            return window;
        }
    }
}