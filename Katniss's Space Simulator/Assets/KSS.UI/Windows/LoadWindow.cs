using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class LoadWindow : MonoBehaviour
    {
        TimelineMetadataUI[] _timelines;
        TimelineMetadataUI _selectedTimeline;
        SaveMetadataUI[] _selectedTimelineSaves;
        SaveMetadataUI _selectedSave;

        IUIElementContainer _saveListUI;
        IUIElementContainer _timelineListUI;

        [SerializeField]
        Button _loadButton;

        // load window will contain a scrollable list of timelines, and then for each timeline, you can load a specific save, or a default (persistent) save.

        // after clicking on a timeline
        // - it is selected,
        // - and its persistent save is selected.
        // after double-clicking
        // - it is selected,
        // - its persistent save is selected,
        // - and loaded.

        // after clicking on a save
        // - it is selected.
        // after double-clicking
        // - it is selected,
        // - and loaded.

        // clicking on the load button becomes possible after a save has been selected.

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

            if( _selectedTimeline == null )
            {
                return;
            }

            SaveMetadata[] saves = SaveMetadata.ReadAllSaves( _selectedTimeline.Timeline.TimelineID ).ToArray();
            _selectedTimelineSaves = new SaveMetadataUI[saves.Length];
            for( int i = 0; i < _selectedTimelineSaves.Length; i++ )
            {
                _selectedTimelineSaves[i] = SaveMetadataUI.Create( _saveListUI, UILayoutInfo.FillHorizontal( 0, 0, 0, 0, 40 ), saves[i], ( ui ) =>
                {
                    _selectedSave = ui;
                } );
            }
        }

        void RefreshTimelineList()
        {
            if( _timelineListUI.IsNullOrDestroyed() )
            {
                return;
            }

            foreach( UIElement timelineUI in _timelineListUI.Children.ToArray() )
            {
                timelineUI.Destroy();
            }

            var timelines = TimelineMetadata.ReadAllTimelines().ToArray();
            _timelines = new TimelineMetadataUI[timelines.Length];
            for( int i = 0; i < _timelines.Length; i++ )
            {
                _timelines[i] = TimelineMetadataUI.Create( _timelineListUI, UILayoutInfo.FillHorizontal( 0, 0, 0, 0, 40 ), timelines[i], ( ui ) =>
                {
                    _selectedTimeline = ui;
                    RefreshSaveList();
                } );
            }
        }

        void OnLoad()
        {
            _selectedSave.Save.LoadAsync();
        }

        public static LoadWindow Create()
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 350f, 400f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            LoadWindow loadWindow = window.gameObject.AddComponent<LoadWindow>();

            UIScrollView timelineList = window.AddVerticalScrollView( UILayoutInfo.FillVertical( 30, 30, 0f, 0, 100 ), 100 )
                .WithVerticalScrollbar( UILayoutInfo.FillVertical( 0, 0, 1f, 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

            timelineList.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            UIScrollView saveList = window.AddVerticalScrollView( UILayoutInfo.FillVertical( 30, 30, 1f, 0, 250 ), 100 )
                .WithVerticalScrollbar( UILayoutInfo.FillVertical( 0, 0, 1f, 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

            saveList.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            UIButton loadButton = window.AddButton( UILayoutInfo.FillHorizontal( 5, 5, 0, 0, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), loadWindow.OnLoad );

            loadWindow._timelineListUI = timelineList;
            loadWindow._saveListUI = saveList;

            loadWindow.RefreshTimelineList();

            return loadWindow;
        }
    }
}