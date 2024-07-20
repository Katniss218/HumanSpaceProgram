using HSP.SceneManagement;
using HSP.UI;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.EditorScene;
using HSP.Vanilla.Scenes.MainMenuScene;
using HSP.Vanilla.UI.Timelines;
using TMPro;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Scenes.MainMenuScene
{
    /// <summary>
    /// Creates the Main Menu UI elements.
    /// </summary>
    public static class MainMenuSceneUIFactory
    {
        static UIStartNewGameWindow _startNewGameWindow; // singleton window
        static UILoadWindow _loadWindow; // singleton window

        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".mainmenu_ui";

        [HSPEventListener( HSPEvent_STARTUP_MAIN_MENU.ID, CREATE_UI )]
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
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreateStartNewGameButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, 0), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _startNewGameWindow == null )
                    _startNewGameWindow = CanvasManager.Get( CanvasName.WINDOWS ).AddStartNewGameWindow();
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Start New Game" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateLoadButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -20), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _loadWindow == null )
                    _loadWindow = CanvasManager.Get( CanvasName.WINDOWS ).AddLoadWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (350f, 400f) ) );
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Load Game" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateDesignARocketButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -100), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( DesignSceneManager.SCENE_NAME, true, false, null ) );
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Design a Rocket" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateDesignAPartButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -120), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( EditorSceneManager.SCENE_NAME, true, false, null ) );
            } )
                .Disabled()
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Part Editor" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateSettingsButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -200), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), null )
                .Disabled()
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Settings" )
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
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Exit" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }
    }
}