using KSS.Core;
using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
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

            if( TimeStepManager.IsPaused )
            {
                UICanvas canvas = CanvasManager.Get( CanvasName.WINDOWS );

                escapeMenuWindow = canvas.AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 300, 300 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                    .Draggable()
                    .Focusable()
                    .AddButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), null, out UIButton closebutton );

                closebutton.onClick = () =>
                {
                    if( !TimeStepManager.LockTimescale )
                    {
                        if( !TimeStepManager.LockTimescale )
                            TimeStepManager.Unpause();
                        escapeMenuWindow.Destroy();
                    }
                };

                escapeMenuWindow.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -50, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), null )
                    .Disabled()
                    .AddText( UILayoutInfo.Fill(), "SETTINGS" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                escapeMenuWindow.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -70, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
                {
                    SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null ) );
                } )
                    .AddText( UILayoutInfo.Fill(), "MAIN MENU" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                escapeMenuWindow.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -90, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () => SaveWindow.Create() )
                    .AddText( UILayoutInfo.Fill(), "SAVE" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                escapeMenuWindow.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -110, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () => LoadWindow.Create() )
                    .AddText( UILayoutInfo.Fill(), "LOAD" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

                escapeMenuWindow.AddButton( UILayoutInfo.FillHorizontal( 50, 50, 1, -150, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), () =>
                {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            } )
                    .AddText( UILayoutInfo.Fill(), "EXIT" )
                    .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );
            }
        }
    }
}