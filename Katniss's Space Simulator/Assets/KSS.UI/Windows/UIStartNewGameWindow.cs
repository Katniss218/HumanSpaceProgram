using KSS.Core;
using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class UIStartNewGameWindow : UIWindow
    {
        UIInputField<string> _nameInputField;
        UIInputField<string> _descriptionInputField;

        public void StartGame()
        {
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( GameplaySceneManager.SCENE_NAME, true, false, () =>
            {
                TimelineManager.CreateNew( IOHelper.SanitizeFileName( _nameInputField.GetOrDefault( "" ) ), SaveMetadata.PERSISTENT_SAVE_ID, _nameInputField.GetOrDefault( "" ), _descriptionInputField.GetOrDefault( "" ) );
            } ) );
        }

        public static T Create<T>( UICanvas parent ) where T : UIStartNewGameWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (300f, 200f) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), "Start New Game..." )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );


            uiWindow.AddStdText( new UILayoutInfo( UIAnchor.TopLeft, (2, -32), (150, 15) ), "Timeline Name" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Right );

            UIInputField<string> inputField = uiWindow.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 154, 2 ), UIAnchor.Top, -32, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            uiWindow.AddStdText( new UILayoutInfo( UIAnchor.TopLeft, (2, -32 - 17), (150, 15) ), "Timeline Description" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Right );

            UIInputField<string> inputField2 = uiWindow.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 154, 2 ), UIAnchor.Top, -32 - 17, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );


            uiWindow.AddButton( new UILayoutInfo( UIAnchor.Bottom, (0, 2), (100, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), uiWindow.StartGame )
                .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Start" )
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