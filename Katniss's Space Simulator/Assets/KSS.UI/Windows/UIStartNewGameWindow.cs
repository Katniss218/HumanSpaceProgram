using KSS.Core;
using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using UnityEditor.PackageManager.UI;
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

        public static T Create<T>( UICanvas parent ) where T : UIStartNewGameWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (300f, 200f) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), "Start New Game..." )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );


            uiWindow.AddText( new UILayoutInfo( UIAnchor.TopLeft, (2, -32), (150, 15) ), "Timeline Name" )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Right );

            UIInputField inputField = uiWindow.AddInputField( new UILayoutInfo( UIFill.Horizontal( 154, 2 ), UIAnchor.Top, -32, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            uiWindow.AddText( new UILayoutInfo( UIAnchor.TopLeft, (2, -32 - 17), (150, 15) ), "Timeline Description" )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Right );

            UIInputField inputField2 = uiWindow.AddInputField( new UILayoutInfo( UIFill.Horizontal( 154, 2 ), UIAnchor.Top, -32 - 17, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );


            uiWindow.AddButton( new UILayoutInfo( UIAnchor.Bottom, (0, 2), (100, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), uiWindow.StartGame )
                .AddText( new UILayoutInfo( UIFill.Fill() ), "Start" )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            uiWindow._nameInputField = inputField;
            uiWindow._descriptionInputField = inputField2;

            return uiWindow;
        }
    }

    public static class UIStartNewGameWindow_Ex
    {
        public static UIStartNewGameWindow AddStartNewGameWindow( this UICanvas parent )
        {
            return UIStartNewGameWindow.Create<UIStartNewGameWindow>( parent );
        }
    }
}