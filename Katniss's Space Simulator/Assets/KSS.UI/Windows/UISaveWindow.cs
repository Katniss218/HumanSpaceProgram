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
    public class UISaveWindow : UIWindow
    {
        UISaveMetadata[] _selectedTimelineSaves;
        UISaveMetadata _selectedSave;

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
            _selectedTimelineSaves = new UISaveMetadata[saves.Length];
            for( int i = 0; i < _selectedTimelineSaves.Length; i++ )
            {
                _selectedTimelineSaves[i] = _saveListUI.AddSaveMetadata( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 40 ), saves[i], ( ui ) =>
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
        public static T Create<T>( UICanvas parent, UILayoutInfo layout ) where T : UISaveWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .Resizeable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), "Save..." )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UIScrollView saveScrollView = uiWindow.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill( 2, 2, 30, 22 ) ), 75 )
                .WithVerticalScrollbar( UIAnchor.Right, 10, null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out UIScrollBar scrollbar );

            UIButton saveBtn = uiWindow.AddButton( new UILayoutInfo( UIAnchor.BottomRight, (-2, 5), (95, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_biaxial" ), uiWindow.OnSave );

            saveBtn.AddText( new UILayoutInfo( UIFill.Fill() ), "Save" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UIInputField inputField = uiWindow.AddInputField( new UILayoutInfo( UIFill.Horizontal( 2, 99 ), UIAnchor.Bottom, 5, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );

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