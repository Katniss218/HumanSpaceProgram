using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.SceneManagement;
using System;
using System.Collections.Generic;
using UnityPlus.UILib;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;
using UnityPlus.AssetManagement;
using TMPro;

namespace KSS.UI.SceneFactories
{
    /// <summary>
    /// Creates the Main Menu UI elements.
    /// </summary>
    public static class VanillaMainMenuUIFactory
    {
        [OverridableEventListener( HSPEvent.STARTUP_MAINMENU, HSPEvent.NAMESPACE_VANILLA + ".mainmenu_ui" )]
        public static void Create( object obj )
        {
            UIElement canvas = (UIElement)CanvasManager.Get( CanvasName.STATIC );

            CreatePlayButton( canvas );
            CreateSettingsButton( canvas );
            CreateExitButton( canvas );

            UIValueBar bar = UIValueBarEx.AddHorizontalValueBar( canvas, new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, 35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            var seg = bar.valueBarComponent.AddSegment( 0.25f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.blue;

            seg = bar.valueBarComponent.InsertSegment( 0, 0.5f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.red;
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreatePlayButton( UIElement parent )
        {
            UIButton button = parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                SceneLoader.UnloadSceneAsync( "MainMenu", () => SceneLoader.LoadSceneAsync( "Testing And Shit", true, false, null ) );
            } );

            button.AddText( UILayoutInfo.Fill(), "PLAY" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateSettingsButton( UIElement parent )
        {
            parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ) )
                .Disabled()
                .AddText( UILayoutInfo.Fill(), "SETTINGS" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateExitButton( UIElement parent )
        {
            parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ) )
                .AddText( UILayoutInfo.Fill(), "EXIT" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }
    }
}