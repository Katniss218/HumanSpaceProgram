using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class LoadWindow : MonoBehaviour
    {
        TimelineMetadataUI[] _timelines;
        TimelineMetadataUI _selectedTimeline;
        SaveMetadataUI[] _selectedTimelineSaves;
        SaveMetadataUI _selectedSave;

        IUIElementContainer _saveList;
        IUIElementContainer _timelineList;

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

        public void SelectTimeline( TimelineMetadataUI timeline )
        {
            _selectedTimeline = timeline;
        }

        void RefreshSaveList()
        {
            foreach( UIElement saveUI in _saveList.Children )
            {
#warning TODO - Doesn't remove from the list.
                saveUI.Destroy();
            }

            var saves = SaveMetadata.ReadAllSaves( _selectedTimeline.Timeline.TimelineID ).ToArray();
            _selectedTimelineSaves = new SaveMetadataUI[saves.Length];
            for( int i = 0; i < _timelines.Length; i++ )
            {
                _selectedTimelineSaves[i] = SaveMetadataUI.Create( _saveList, UILayoutInfo.FillHorizontal( 0, 0, 0, 0, 40 ), saves[i] );
            }
        }

        void RefreshTimelineList()
        {
            if( _timelineList == null )
            {
                return;
            }

            foreach( UIElement timelineUI in _timelineList.Children )
            {
#warning TODO - Doesn't remove from the list.
                timelineUI.Destroy();
            }

            var timelines = TimelineMetadata.ReadAllTimelines().ToArray();
            _timelines = new TimelineMetadataUI[timelines.Length];
            for( int i = 0; i < _timelines.Length; i++ )
            {
                _timelines[i] = TimelineMetadataUI.Create( _timelineList, UILayoutInfo.FillHorizontal( 0, 0, 0, 0, 40 ), timelines[i] );
            }
        }

        public void OnPressLoad()
        {
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( "Testing And Shit", true, false, () =>
            {
                TimelineManager.BeginLoadAsync( _selectedSave.Save.TimelineID, _selectedSave.Save.SaveID );
            } ) );
        }

        void Awake()
        {
            RefreshTimelineList();
        }

        public static LoadWindow Create()
        {
            UIWindow window = ((UICanvas)CanvasManager.Get( CanvasName.WINDOWS )).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 350f, 400f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            UIScrollView timelineList = window.AddVerticalScrollView( UILayoutInfo.FillVertical( 30, 0, 0f, 0, 100 ), new Vector2( 300, 100 ) )
                .WithVerticalScrollbar( UILayoutInfo.FillVertical( 0, 0, 1f, 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

            UIScrollView saveList = window.AddVerticalScrollView( UILayoutInfo.FillVertical( 30, 0, 1f, 0, 250 ), new Vector2( 300, 100 ) )
                .WithVerticalScrollbar( UILayoutInfo.FillVertical( 0, 0, 1f, 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

#warning TODO - needs proper layout support in the UILib

            LoadWindow loadWindow = window.gameObject.AddComponent<LoadWindow>();
            loadWindow._timelineList = timelineList;
            loadWindow._saveList = saveList;
            return loadWindow;
        }
    }
}