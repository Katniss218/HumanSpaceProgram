using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using KSS.Core.Serialization;
using System.Linq;

namespace KSS.UI
{
    public class SaveWindow : MonoBehaviour
    {
        SaveMetadataUI[] _selectedTimelineSaves;
        SaveMetadataUI _selectedSave;

        IUIElementContainer _saveListUI;

        UIInputField _nameInputField;
        UIInputField _descriptionInputField;

        void RefreshSaveList()
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
            _selectedTimelineSaves = new SaveMetadataUI[saves.Length];
            for( int i = 0; i < _selectedTimelineSaves.Length; i++ )
            {
                _selectedTimelineSaves[i] = SaveMetadataUI.Create( _saveListUI, new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 40 ), saves[i], ( ui ) =>
                {
                    _selectedSave = ui;
                } );
            }
        }

        void OnSave()
        {
            if( _nameInputField.Text != null )
            {
                TimelineManager.BeginSaveAsync( TimelineManager.CurrentTimeline.TimelineID, IOHelper.SanitizeFileName( _nameInputField.Text ), _nameInputField.Text, _descriptionInputField.Text );
            }
            else
            {
                TimelineManager.BeginSaveAsync( TimelineManager.CurrentTimeline.TimelineID, _selectedSave.Save.SaveID, _selectedSave.Save.Name, _selectedSave.Save.Description );
            }
        }

        /// <summary>
        /// Creates a save window with the current context.
        /// </summary>
        public static SaveWindow Create()
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (250f, 100f) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            SaveWindow saveWindow = window.gameObject.AddComponent<SaveWindow>();

            UIScrollView saveScrollView = window.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill( 2, 2, 30, 22 ) ), 75 )
                .WithVerticalScrollbar( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical( 2, 2 ), 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out UIScrollBar scrollbar );


            UIButton saveBtn = window.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (-2, 5), (95, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), saveWindow.OnSave );

            saveBtn.AddText( new UILayoutInfo( UIFill.Fill() ), "Save" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UIInputField inputField = window.AddInputField( new UILayoutInfo( UIFill.Horizontal( 2, 99 ), UIAnchor.Bottom, 5, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

            saveWindow._nameInputField = inputField;
            saveWindow._descriptionInputField = inputField;
            saveWindow._saveListUI = saveScrollView;

            saveWindow.RefreshSaveList();

            return saveWindow;
        }
    }
}