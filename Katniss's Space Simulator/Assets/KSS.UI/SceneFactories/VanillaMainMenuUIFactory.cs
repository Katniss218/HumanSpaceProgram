using KSS.Core;
using KSS.Core.Mods;
using KSS.Core.Scenes;
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
        [OverridableEventListener( HSPOverridableEvent.STARTUP_MAINMENU, HSPOverridableEvent.NAMESPACE_VANILLA + ".mainmenu_ui" )]
        public static void Create( object obj )
        {
            RectTransform canvasTransform = (RectTransform)CanvasManager.Get( CanvasName.STATIC ).transform;

            CreatePlayButton( canvasTransform );
            CreateSettingsButton( canvasTransform );
            CreateExitButton( canvasTransform );

            UIValueBar bar = UIValueBarEx.AddHorizontalValueBar( (UIElement)canvasTransform, new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, 35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            var seg = bar.valueBarComponent.AddSegment( 0.25f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.blue;

            seg = bar.valueBarComponent.InsertSegment( 0, 0.5f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.red;
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreatePlayButton( RectTransform parent )
        {
            UIButton button = ((UIElement)parent).AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ) );

            button.AddText( UILayoutInfo.Fill(), "PLAY" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );

            PlayButtonSwitcher pbs = button.gameObject.AddComponent<PlayButtonSwitcher>();
            button.buttonComponent.onClick.AddListener( pbs.StartGame );
        }

        private static void CreateSettingsButton( RectTransform parent )
        {
            ((UIElement)parent).AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ) )
                .Disabled()
                .AddText( UILayoutInfo.Fill(), "SETTINGS" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateExitButton( RectTransform parent )
        {
            ((UIElement)parent).AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ) )
                .AddText( UILayoutInfo.Fill(), "EXIT" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }
    }
}