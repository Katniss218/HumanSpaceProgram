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
using KSS.Core.Serialization;
using UnityPlus.UILib.Layout;

namespace KSS.UI.SceneFactories
{
    /// <summary>
    /// Creates the Main Menu UI elements.
    /// </summary>
    public static class VanillaMainMenuUIFactory
    {
        static GameObject _startNewGameWindow; // singleton window
        static GameObject _loadWindow; // singleton window

        [HSPEventListener( HSPEvent.STARTUP_MAINMENU, HSPEvent.NAMESPACE_VANILLA + ".mainmenu_ui" )]
        public static void Create( object e )
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            CreateStartNewGameButton( canvas );
            CreateLoadButton( canvas );
            CreateSettingsButton( canvas );
            CreateExitButton( canvas );

            UIValueBar bar = UIValueBarEx.AddHorizontalValueBar( canvas, new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, 35 ), new Vector2( 200, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            var seg = bar.AddSegment( 0.25f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.blue;

            seg = bar.InsertSegment( 0, 0.5f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.red;
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreateStartNewGameButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, 0 ), new Vector2( 200, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _startNewGameWindow == null )
                    _startNewGameWindow = StartNewGameWindow.Create().gameObject;
            } )
                .AddText( UILayoutInfo.Fill(), "Start New Game" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateLoadButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -20 ), new Vector2( 200, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _loadWindow == null )
                    _loadWindow = LoadWindow.Create().gameObject;
            } )
                .AddText( UILayoutInfo.Fill(), "Load Game" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateSettingsButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -60 ), new Vector2( 200, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), null )
                .Disabled()
                .AddText( UILayoutInfo.Fill(), "Settings" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateExitButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), new Vector2( 0, -80 ), new Vector2( 200, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            } )
                .AddText( UILayoutInfo.Fill(), "Exit" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }
    }
}