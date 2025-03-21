using HSP.Content;
using HSP.SceneManagement;
using HSP.Timelines;
using HSP.Timelines.Serialization;
using HSP.UI;
using HSP.Vanilla.Scenes.GameplayScene;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Timelines
{
    public class UIStartNewGameWindow : UIWindow
    {
        public enum Step
        {
            SelectScenario = 0,
            SetParameters = 1
        }

        IUIElementContainer _contentsPanel;
        IUIElementContainer _scenarioListUI;

        UIInputField<string> _nameInputField;
        UIInputField<string> _descriptionInputField;

        public NamespacedID ScenarioID { get; private set; }

        public Step CurrentStep { get; private set; }

        public void StartGame()
        {
            TimelineMetadata meta = new TimelineMetadata( IOHelper.SanitizeFileName( _nameInputField.GetOrDefault( "" ) ) )
            {
                Name = _nameInputField.GetOrDefault( "" ),
                Description = _descriptionInputField.GetOrDefault( "" )
            };

            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( GameplaySceneManager.SCENE_NAME, true, false, () =>
            {
                TimelineManager.BeginScenarioAsync( ScenarioID, meta );
            } ) );
        }

        private void ReloadScenarios()
        {

        }

        private void Create_Step1()
        {
            foreach( var child in _contentsPanel.Children )
            {
                child.Destroy();
            }
            this.CurrentStep = Step.SelectScenario;

            _scenarioListUI = _contentsPanel.AddPanel( new UILayoutInfo( UIFill.Fill( 0, 0, 0, 30 ) ), null );

            var btn = _contentsPanel.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (0, 0), (100, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), () =>
            {
                Create_Step2();
            } );

            ReloadScenarios();
        }

        private void Create_Step2()
        {
            foreach( var child in _contentsPanel.Children )
            {
                child.Destroy();
            }
            this.CurrentStep = Step.SetParameters;

            _contentsPanel.AddStdText( new UILayoutInfo( UIAnchor.TopLeft, (2, -32), (150, 15) ), "Timeline Name" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Right );

            UIInputField<string> inputField = _contentsPanel.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 154, 2 ), UIAnchor.Top, -32, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            _contentsPanel.AddStdText( new UILayoutInfo( UIAnchor.TopLeft, (2, -32 - 17), (150, 15) ), "Timeline Description" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Right );

            UIInputField<string> inputField2 = _contentsPanel.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 154, 2 ), UIAnchor.Top, -32 - 17, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );
            _contentsPanel.AddButton( new UILayoutInfo( UIAnchor.Bottom, (0, 2), (100, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), this.StartGame )
            .AddStdText( new UILayoutInfo( UIFill.Fill() ), "Start" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            this._nameInputField = inputField;
            this._descriptionInputField = inputField2;
        }

        public static T Create<T>( UICanvas parent, Step step = Step.SelectScenario ) where T : UIStartNewGameWindow
        {
            // 2 steps

            // first player picks the scenario

            // click next

            // replace the uis with a new ui where they can edit the values
            // including name/desc of the timeline.



            T uiWindow = (T)UIWindow.Create<T>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (500f, 700f) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .Resizeable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), "Start New Game..." )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            uiWindow._contentsPanel = uiWindow.AddPanel( new UILayoutInfo( UIFill.Fill( 0, 0, 30, 0 ) ), null );

            if( step == Step.SelectScenario )
                uiWindow.Create_Step1();
            else if( step == Step.SetParameters )
                uiWindow.Create_Step2();

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