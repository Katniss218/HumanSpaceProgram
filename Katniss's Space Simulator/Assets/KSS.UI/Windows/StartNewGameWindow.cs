using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class StartNewGameWindow : MonoBehaviour
    {
        UIInputField _nameInputField;

        public void StartGame()
        {
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( "Testing And Shit", true, false, () =>
            {
                TimelineManager.CreateNew( IOHelper.SanitizeFileName( _nameInputField.Text ), SaveMetadata.PERSISTENT_SAVE_ID );
            } ) );
        }

        public static StartNewGameWindow Create()
        {
            UIWindow window = ((UICanvas)CanvasManager.Get( CanvasName.WINDOWS )).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 400f, 400f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            window.AddText( new UILayoutInfo( new Vector2( 0.5f, 1 ), new Vector2( -100, -32 ), new Vector2( 100, 15 ) ), "Timeline Name" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Right );

            UIInputField inputField = window.AddInputField( new UILayoutInfo( new Vector2( 0.5f, 1 ), new Vector2( 100, -32 ), new Vector2( 200, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            StartNewGameWindow windowComponent = window.gameObject.AddComponent<StartNewGameWindow>();

            window.AddButton( new UILayoutInfo( new Vector2( 0.5f, 0 ), new Vector2( 0, 2 ), new Vector2( 200, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                windowComponent.StartGame();
            } )
                .AddText( UILayoutInfo.Fill(), "Start" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );

            windowComponent._nameInputField = inputField;

            return windowComponent;
        }
    }
}