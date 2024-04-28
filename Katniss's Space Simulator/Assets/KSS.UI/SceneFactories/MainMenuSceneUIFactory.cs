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
    public static class MainMenuSceneUIFactory
    {
        static UIStartNewGameWindow _startNewGameWindow; // singleton window
        static UILoadWindow _loadWindow; // singleton window

        [HSPEventListener( HSPEvent.STARTUP_MAINMENU, HSPEvent.NAMESPACE_VANILLA + ".mainmenu_ui" )]
        public static void Create()
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            CreateStartNewGameButton( canvas );
            CreateLoadButton( canvas );
            CreateDesignARocketButton( canvas );
            CreateDesignAPartButton( canvas );
            CreateSettingsButton( canvas );
            CreateExitButton( canvas );

            UIValueBar bar = canvas.AddHorizontalValueBar( new UILayoutInfo( UIAnchor.Center, (0, 35), (200, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            var seg = bar.AddSegment( 0.25f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.blue;

            seg = bar.InsertSegment( 0, 0.5f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.red;


            canvas.AddStringInputDropdown( CanvasManager.Get( CanvasName.CONTEXT_MENUS ), new UILayoutInfo( UIAnchor.Center, (0, -300), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), null )
                .WithOptions( new[] { "hello", "hi", "foo", "bar", "baz" } )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreateStartNewGameButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, 0), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _startNewGameWindow == null )
                    _startNewGameWindow = CanvasManager.Get( CanvasName.WINDOWS ).AddStartNewGameWindow();
            } )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Start New Game" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateLoadButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -20), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _loadWindow == null )
                    _loadWindow = CanvasManager.Get( CanvasName.WINDOWS ).AddLoadWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (350f, 400f) ) );
            } )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Load Game" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateDesignARocketButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -100), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( DesignSceneManager.SCENE_NAME, true, false, null ) );
            } )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Design a Rocket" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateDesignAPartButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -120), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( EditorSceneManager.SCENE_NAME, true, false, null ) );
            } )
                .Disabled()
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Part Editor" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateSettingsButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -200), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), null )
                .Disabled()
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Settings" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateExitButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -300), (150, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            } )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Exit" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }
    }
}