using HSP.Core;
using HSP.Core.SceneManagement;
using HSP.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.SceneFactories
{
    public static class GameplayEscapeUIFactory
    {
        static UIWindow escapeMenuWindow;

        [HSPEventListener( HSPEvent.ESCAPE_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".escape_menu_ui" )]
        private static void OnGameplayEscape()
        {
            if( TimeStepManager.IsPaused && TimeStepManager.LockTimescale )
            {
                return;
            }

            if( !escapeMenuWindow.IsNullOrDestroyed() )
            {
                escapeMenuWindow.Destroy();
            }

#warning TODO - Add conditional unpause to the 'ondestroy' of the escapeMenuWindow. (conditional = unless timescale has changed since displaying window).

            if( TimeStepManager.IsPaused )
            {
                UICanvas canvas = CanvasManager.Get( CanvasName.WINDOWS );

                escapeMenuWindow = canvas.AddWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (300, 300) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                    .Draggable()
                    .Focusable()
                    .WithButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), null, out UIButton closebutton );

                closebutton.onClick = () =>
                {
                    if( !TimeStepManager.LockTimescale )
                    {
                        if( !TimeStepManager.LockTimescale )
                            TimeStepManager.Unpause();
                        escapeMenuWindow.Destroy();
                    }
                };

                escapeMenuWindow.AddButton( new UILayoutInfo( UIFill.Horizontal( 50, 50 ), UIAnchor.Top, -50, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), null )
                    .Disabled()
                    .AddStdText( new UILayoutInfo( UIFill.Fill() ), "SETTINGS" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

                escapeMenuWindow.AddButton( new UILayoutInfo( UIFill.Horizontal( 50, 50 ), UIAnchor.Top, -70, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
                {
                    SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null ) );
                } )
                    .AddStdText( new UILayoutInfo( UIFill.Fill() ), "MAIN MENU" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

                escapeMenuWindow.AddButton( new UILayoutInfo( UIFill.Horizontal( 50, 50 ), UIAnchor.Top, -90, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), 
                    () => CanvasManager.Get( CanvasName.WINDOWS ).AddSaveWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (350f, 400f) ) ) )
                    .AddStdText( new UILayoutInfo( UIFill.Fill() ), "SAVE" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

                escapeMenuWindow.AddButton( new UILayoutInfo( UIFill.Horizontal( 50, 50 ), UIAnchor.Top, -110, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ),
                    () => CanvasManager.Get( CanvasName.WINDOWS ).AddLoadWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (350f, 400f) ) ) )
                    .AddStdText( new UILayoutInfo( UIFill.Fill() ), "LOAD" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

                escapeMenuWindow.AddButton( new UILayoutInfo( UIFill.Horizontal( 50, 50 ), UIAnchor.Top, -150, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                } )
                    .AddStdText( new UILayoutInfo( UIFill.Fill() ), "EXIT" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );
            }
        }
    }
}