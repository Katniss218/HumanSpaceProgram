using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using KSS.UI.Windows;
using System;
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

        void OnLoad()
        {
            _selectedSave.Save.LoadAsync();
        }

        public static T Create<T>( UICanvas parent, UILayoutInfo layout ) where T : UILoadWindow
        {
            //UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (350f, 400f) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
            T uiWindow = (T)UIWindow.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            UIScrollView timelineList = uiWindow.AddVerticalScrollView( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical( 30, 30 ), 0, 100 ), 100 )
                .WithVerticalScrollbar( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical(), 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

            timelineList.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            UIScrollView saveList = uiWindow.AddVerticalScrollView( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical( 30, 30 ), 0, 250 ), 100 )
                .WithVerticalScrollbar( new UILayoutInfo( UIAnchor.Right, UIFill.Vertical(), 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

            saveList.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            UIButton loadButton = uiWindow.AddButton( new UILayoutInfo( UIFill.Horizontal( 5, 5 ), UIAnchor.Bottom, 0, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), uiWindow.OnLoad );

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