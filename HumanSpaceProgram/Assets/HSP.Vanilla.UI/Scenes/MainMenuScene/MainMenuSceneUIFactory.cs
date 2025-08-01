using HSP.SceneManagement;
using HSP.UI;
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
        static UIPanel _mainPanel;

        static UIStartNewGameWindow _startNewGameWindow; // singleton window
        static UILoadWindow _loadWindow; // singleton window

        public const string CREATE_UI = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.ui.create";
        public const string DESTROY_UI = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.ui.destroy";

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_ACTIVATE.ID, CREATE_UI )]
        private static void Create()
        {
            UICanvas canvas = MainMenuSceneM.Instance.GetStaticCanvas();

            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }
            _mainPanel = canvas.AddPanel( new UILayoutInfo( UIFill.Fill() ), null );

            CreateStartNewGameButton( _mainPanel );
            CreateLoadButton( _mainPanel );
            CreateDesignARocketButton( _mainPanel );
            CreateDesignAPartButton( _mainPanel );
            CreateSettingsButton( _mainPanel );
            CreateExitButton( _mainPanel );

            UIValueBar bar = _mainPanel.AddHorizontalValueBar( new UILayoutInfo( UIAnchor.Center, (0, 35), (200, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            var seg = bar.AddSegment( 0.25f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.blue;

            seg = bar.InsertSegment( 0, 0.5f );
            seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
            seg.Color = Color.red;
        }

        [HSPEventListener( HSPEvent_MAIN_MENU_SCENE_DEACTIVATE.ID, DESTROY_UI )]
        private static void Destroy()
        {
            if( !_mainPanel.IsNullOrDestroyed() )
            {
                _mainPanel.Destroy();
            }
        }

        // #-#-#-#-#-#-#-#-#-#

        private static void CreateStartNewGameButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, 0), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _startNewGameWindow == null )
                    _startNewGameWindow = MainMenuSceneM.Instance.GetWindowCanvas().AddStartNewGameWindow();
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Start New Game" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateLoadButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -20), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                if( _loadWindow == null )
                    _loadWindow = MainMenuSceneM.Instance.GetWindowCanvas().AddLoadWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (350f, 400f) ) );
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Load Game" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateDesignARocketButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -100), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                HSPSceneManager.ReplaceForegroundScene<Vanilla.Scenes.DesignScene.DesignSceneM>();
            } )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Design a Rocket" )
                .WithAlignment( HorizontalAlignmentOptions.Center );
        }

        private static void CreateDesignAPartButton( IUIElementContainer parent )
        {
            parent.AddButton( new UILayoutInfo( UIAnchor.Center, (0, -120), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                HSPSceneManager.ReplaceForegroundScene<EditorSceneM>();
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