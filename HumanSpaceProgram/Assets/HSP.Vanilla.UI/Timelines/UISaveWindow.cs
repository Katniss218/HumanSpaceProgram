using HSP.Timelines;
using HSP.Timelines.Serialization;
using HSP.UI;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Timelines
{
    public class UISaveWindow : UIWindow
    {
        UISaveMetadata[] _selectedTimelineSaves;
        UISaveMetadata _selectedSave;

        IUIElementContainer _saveListUI;

        UIInputField<string> _nameInputField;
        UIInputField<string> _descriptionInputField;

        public SaveMetadata Save { get; private set; } = TimelineManager.CurrentSave ?? new SaveMetadata( TimelineManager.CurrentTimeline.TimelineID );

        private void RefreshSaveList()
        {
            if( _saveListUI.IsNullOrDestroyed() )
            {
                return;
            }

            foreach( UIElement saveUI in _saveListUI.Children.ToArray() )
            {
                saveUI.Destroy();
            }

            if( TimelineManager.CurrentTimeline == null )
            {
                return;
            }

            SaveMetadata[] saves = SaveMetadata.ReadAllSaves( TimelineManager.CurrentTimeline.TimelineID ).ToArray();
            _selectedTimelineSaves = new UISaveMetadata[saves.Length];
            for( int i = 0; i < _selectedTimelineSaves.Length; i++ )
            {
                _selectedTimelineSaves[i] = _saveListUI.AddSaveMetadata( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 40 ), saves[i], OnExistingSaveClick );
            }
        }

        private void OnExistingSaveClick( UISaveMetadata ui )
        {
            _selectedSave = ui;
            Save = ui.Save;
            _nameInputField.SetValue( ui.Save.Name );
            _descriptionInputField.SetValue( ui.Save.Description );
        }

        private void OnNameChanged( IUIInputElement<string>.ValueChangedEventData e )
        {
            _selectedSave = null;
            Save = new SaveMetadata( Save.TimelineID, IOHelper.SanitizeFileName( e.NewValue ) )
            {
                Name = e.NewValue,
                Description = Save.Description,
            };
        }

        private void OnSave()
        {
            TimelineManager.BeginSaveAsync( Save );
            /* if( _nameInputField.TryGetValue( out string Text ) )
             {
                 _descriptionInputField.TryGetValue( out string Description );
                 TimelineManager.BeginSaveAsync( TimelineManager.CurrentTimeline.TimelineID, IOHelper.SanitizeFileName( Text ), Text, Description );
             }
             else
             {
                 TimelineManager.BeginSaveAsync( TimelineManager.CurrentTimeline.TimelineID, _selectedSave.Save.SaveID, _selectedSave.Save.Name, _selectedSave.Save.Description );
             }
            */
#warning TODO -
            // after changing the name - change the name of the selected save.
            // add a button that deselects the current save.
            // currently selected timeline/save should be outlined in the UI.
        }

        /// <summary>
        /// Creates a save window with the current context.
        /// </summary>
        public static T Create<T>( UICanvas parent, UILayoutInfo layout ) where T : UISaveWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .Resizeable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), "Save..." )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            UIScrollView saveScrollView = uiWindow.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill( 2, 2, 30, 22 ) ), 75 )
                .WithVerticalScrollbar( UIAnchor.Right, 10, null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out UIScrollBar scrollbar );

            UIButton saveBtn = uiWindow.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (-2, 5), (95, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), uiWindow.OnSave );

            saveBtn.AddStdText( new UILayoutInfo( UIFill.Fill() ), "Save" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            UIInputField<string> inputField = uiWindow.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 2, 99 ), UIAnchor.Bottom, 5, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );
            inputField.OnValueChanged += uiWindow.OnNameChanged;

            uiWindow._nameInputField = inputField;
            uiWindow._descriptionInputField = inputField;
            uiWindow._saveListUI = saveScrollView;

            uiWindow.RefreshSaveList();

            return uiWindow;
        }
    }

    public static class UISaveWindow_Ex
    {
        public static UISaveWindow AddSaveWindow( this UICanvas parent, UILayoutInfo layout )
        {
            return UISaveWindow.Create<UISaveWindow>( parent, layout );
        }
    }
}