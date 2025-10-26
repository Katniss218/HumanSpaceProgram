using HSP.Timelines.Serialization;
using HSP.SceneManagement;
using HSP.Timelines;
using HSP.UI;
using HSP.Vanilla.Scenes.GameplayScene;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Timelines
{
    public class UILoadWindow : UIWindow
    {
        UITimelineMetadata[] _timelines;
        UITimelineMetadata _selectedTimeline;
        UISaveMetadata[] _selectedTimelineSaves;
        UISaveMetadata _selectedSave;

        IUIElementContainer _saveListUI;
        IUIElementContainer _timelineListUI;

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
            _timelines = new UITimelineMetadata[timelines.Length];
            for( int i = 0; i < _timelines.Length; i++ )
            {
                _timelines[i] = _timelineListUI.AddTimelineMetadata( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 40 ), timelines[i], ( ui ) =>
                {
                    _selectedTimeline = ui;
                    RefreshSaveList();
                } );
            }
        }

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
            _selectedTimelineSaves = new UISaveMetadata[saves.Length];
            for( int i = 0; i < _selectedTimelineSaves.Length; i++ )
            {
                _selectedTimelineSaves[i] = _saveListUI.AddSaveMetadata( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 40 ), saves[i], ( ui ) =>
                {
                    _selectedSave = ui;
                } );
            }
        }

        public static void LoadAsync( SaveMetadata save )
        {
            HSPSceneManager.ReplaceForegroundScene<GameplaySceneM, GameplaySceneM.LoadData>( GameplaySceneM.LoadSaveLoadData( save.TimelineID, save.SaveID ), onAfterLoaded: () =>
            {
                //TimelineManager.BeginLoadAsync( save.TimelineID, save.SaveID );
            } );
        }

        void OnLoad()
        {
            LoadAsync( _selectedSave.Save );
        }

        public static T Create<T>( UICanvas parent, UILayoutInfo layout ) where T : UILoadWindow
        {
            T uiWindow = (T)UIWindow.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .Resizeable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            uiWindow.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 30 ), "Load..." )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            UIScrollView timelineList = uiWindow.AddVerticalScrollView( new UILayoutInfo( UIFill.HorizontalPercent( 0, 0.6667f ), UIFill.Vertical( 32, 19 ) ), 100 )
                .WithVerticalScrollbar( UIAnchor.Right, 10, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical_background" ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical" ), out _ );

            timelineList.LayoutDriver = new VerticalLayoutDriver() { Spacing = 2, FitToSize = true };

            UIScrollView saveList = uiWindow.AddVerticalScrollView( new UILayoutInfo( UIFill.HorizontalPercent( 0.3333f, 0 ), UIFill.Vertical( 32, 19 ) ), 100 )
                .WithVerticalScrollbar( UIAnchor.Right, 10, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical_background" ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical" ), out _ );

            saveList.LayoutDriver = new VerticalLayoutDriver() { Spacing = 2, FitToSize = true };

            UIButton loadButton = uiWindow.AddButton( new UILayoutInfo( UIAnchor.Bottom, (0, 2), (100, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), uiWindow.OnLoad );

            loadButton.AddStdText( new UILayoutInfo( UIFill.Fill() ), "Load" )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Center );

            uiWindow._timelineListUI = timelineList;
            uiWindow._saveListUI = saveList;

            uiWindow.RefreshTimelineList();

            return uiWindow;
        }
    }

    public static class UILoadWindow_Ex
    {
        public static UILoadWindow AddLoadWindow( this UICanvas parent, UILayoutInfo layout )
        {
            return UILoadWindow.Create<UILoadWindow>( parent, layout );
        }
    }
}