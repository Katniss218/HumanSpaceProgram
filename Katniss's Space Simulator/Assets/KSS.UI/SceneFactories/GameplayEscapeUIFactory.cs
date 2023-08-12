using KSS.Core;
using KSS.Core.Serialization;
using KSS.Core.TimeWarp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;
using Object = UnityEngine.Object;

namespace KSS.UI.SceneFactories
{
    public static class GameplayEscapeUIFactory
    {
        static GameObject escapeMenuWindow;

        [HSPEventListener( HSPEvent.ESCAPE_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".escape_menu_ui" )]
        public static void OnGameplayEscape( object obj )
        {
            if( escapeMenuWindow != null )
            {
                Object.Destroy( escapeMenuWindow );
                return;
            }

            Canvas canvas = CanvasManager.Get( CanvasName.WINDOWS );

            UIWindow window = canvas.AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 300, 300 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out UIButton closebutton );

            escapeMenuWindow = window.gameObject;

            closebutton.gameObject.GetComponent<RectTransformCloser>().CanClose = () => !TimeWarpManager.LockTimescale;
            closebutton.onClick.AddListener( () =>
            {
                if( !TimeWarpManager.LockTimescale ) 
                    TimeWarpManager.Unpause();
            } );

            window.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -50, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ) )
                .Disabled()
                .AddText( UILayoutInfo.Fill(), "SETTINGS" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -70, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () => SaveWindow.Create() )
                .AddText(UILayoutInfo.Fill(), "SAVE" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -90, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () => LoadWindow.Create() )
                .AddText( UILayoutInfo.Fill(), "LOAD" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -110, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ) )
                .Disabled()
                .AddText( UILayoutInfo.Fill(), "MAIN MENU" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            window.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -150, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ) )
                .Disabled()
                .AddText( UILayoutInfo.Fill(), "EXIT" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

        }
    }
}