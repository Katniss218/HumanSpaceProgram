using KSS.Core;
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
        UIInputField _descriptionInputField;

        public void StartGame()
        {
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( GameplaySceneManager.SCENE_NAME, true, false, () =>
            {
                TimelineManager.CreateNew( IOHelper.SanitizeFileName( _nameInputField.Text ), SaveMetadata.PERSISTENT_SAVE_ID, _nameInputField.Text, _descriptionInputField.Text );
            } ) );
        }

        public static StartNewGameWindow Create()
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (400f, 400f) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            window.AddText( new UILayoutInfo( UIAnchor.Top, (-100, -32), (100, 15) ), "Timeline Name" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Right );

            UIInputField inputField = window.AddInputField( new UILayoutInfo( UIAnchor.Top, (100, -32), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            StartNewGameWindow windowComponent = window.gameObject.AddComponent<StartNewGameWindow>();

            window.AddButton( new UILayoutInfo( UIAnchor.Bottom, (0, 2), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                windowComponent.StartGame();
            } )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Start" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );

            windowComponent._nameInputField = inputField;
#warning TODO - add a description box.
            windowComponent._descriptionInputField = inputField;

            return windowComponent;
        }
    }
}