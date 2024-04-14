using KSS.Core;
using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using TMPro;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class UIStartNewGameWindow : UIWindow
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

        public static T Create<T>( UICanvas parent, UILayoutInfo layout ) where T : UIStartNewGameWindow
        {
            // CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (400f, 400f) )
            T window = (T)UIWindow.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            window.AddText( new UILayoutInfo( UIAnchor.Top, (-100, -32), (100, 15) ), "Timeline Name" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Right );

            UIInputField inputField = window.AddInputField( new UILayoutInfo( UIAnchor.Top, (100, -32), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            window.AddButton( new UILayoutInfo( UIAnchor.Bottom, (0, 2), (200, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), window.StartGame )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Start" )
                .WithFont( AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( HorizontalAlignmentOptions.Center );

            window._nameInputField = inputField;
#warning TODO - add a description box.
            window._descriptionInputField = inputField;

            return window;
        }
    }

    public static class UIStartNewGameWindow_Ex
    {
        public static UIStartNewGameWindow AddStartNewGameWindow( this UICanvas parent, UILayoutInfo layout )
        {
            return UIStartNewGameWindow.Create<UIStartNewGameWindow>( parent, layout );
        }
    }
}