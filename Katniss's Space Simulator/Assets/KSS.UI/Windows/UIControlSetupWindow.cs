using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    public class UIControlSetupWindow : UIWindow
    {
        private struct LastVisibleEntry
        {
            public Component component;
            public Vector2 lastAnchoredPosition;
        }

        /// <summary>
        /// Defines the possible components that can be displayed in the window.
        /// </summary>
        public Transform Target { get; private set; }

        private UIContextMenu _componentList;

        internal IUIElementContainer ComponentContainer { get; private set; }
        internal IUIElementContainer ConnectionContainer { get; private set; }

        internal UIScrollView scrollView;

        Dictionary<Component, ControlSetupWindowComponentUI> _visibleComponents = new();

        Dictionary<Control.Control, ControlSetupControlUI> _visibleInputs = new();
        Dictionary<Control.Control, ControlSetupControlUI> _visibleOutputs = new();

        List<ControlSetupControlConnectionUI> _visibleConnections = new();

        private static LastVisibleEntry[] _lastVisibleComponents = new LastVisibleEntry[] { };

        /// <summary>
        /// Creates a component UI for the specified component, if not already visible. <br/>
        /// Updates the connections to keep synced.
        /// </summary>
        public void ShowComponent( Component component )
        {
            if( _visibleComponents.ContainsKey( component ) )
                return;

            if( Target.IsAncestorOf( component.transform ) )
            {
                if( TryCreateNode( new LastVisibleEntry() { component = component }, out _ ) )
                {
                    RefreshConnections();
                }
            }
        }

        /// <summary>
        /// Destroys the component UI for the specified component. <br/>
        /// Updates the connections to keep synced.
        /// </summary>
        public void HideComponent( Component component )
        {
            if( _visibleComponents.TryGetValue( component, out var componentUI ) )
            {
                componentUI.Destroy();
                _visibleComponents.Remove( component );

                foreach( var input in componentUI.GetInputs() )
                {
                    _visibleInputs.Remove( input.Control );
                }
                foreach( var output in componentUI.GetOutputs() )
                {
                    _visibleOutputs.Remove( output.Control );
                }

                RefreshConnections();
            }
        }

        private bool TryCreateNode( LastVisibleEntry entryToShow, out ControlSetupWindowComponentUI componentUI )
        {
            if( ControlUtils.HasControlsOrGroups( entryToShow.component ) )
            {
                componentUI = ControlSetupWindowComponentUI.Create( this, entryToShow.component );
                _visibleComponents.Add( entryToShow.component, componentUI );
                ((RectTransform)componentUI.transform).anchoredPosition = entryToShow.lastAnchoredPosition;

                foreach( var input in componentUI.GetInputs() )
                {
                    _visibleInputs.Add( input.Control, input );
                }
                foreach( var output in componentUI.GetOutputs() )
                {
                    _visibleOutputs.Add( output.Control, output );
                }

                return true;
            }

            componentUI = null;
            return false;
        }

        private void CreateNodes( IEnumerable<LastVisibleEntry> entriesToShow )
        {
            foreach( var comp in entriesToShow )
            {
                TryCreateNode( comp, out _ );
            }
        }

        /*[Obsolete( "doesn't quite work yet" )]
        private void RefreshContainerSize()
        {
            // This is janky but I have no idea how to do it properly. Seems like it would need updating like 100 different things.
            RectTransform[] rects = _visibleComponents.Values.Select( ui => ui.panel.rectTransform ).ToArray();

            float xMin = 0;
            float xMax = 0;
            float yMin = 0;
            float yMax = 0;

            for( int i = 0; i < rects.Length; i++ )
            {
                Rect rect = rects[i].rect;
                Vector2 anchoredPosition = rects[i].anchoredPosition;

                xMin = Mathf.Min( xMin, rect.xMin + anchoredPosition.x );
                xMax = Mathf.Max( xMax, rect.xMax + anchoredPosition.x );
                yMin = Mathf.Min( yMin, rect.yMin + anchoredPosition.y );
                yMax = Mathf.Max( yMax, rect.yMax + anchoredPosition.y );
            }

            scrollView.SetContentsSize( xMin, xMax, yMax, yMin );
        }*/

        /// <summary>
        /// Updates the endpoint positions of all visible connection UIs without re-creating each connection.
        /// </summary>
        public void RefreshConnectionPositions()
        {
            RefreshConnectionPositions( _visibleConnections );
        }

        /// <summary>
        /// Updates the endpoint positions of the specified connection UIs without re-creating each connection.
        /// </summary>
        public void RefreshConnectionPositions( IEnumerable<ControlSetupControlConnectionUI> connectionsToRefresh )
        {
            foreach( var conn in connectionsToRefresh )
            {
                conn.RecalculateEndPositions();
            }
            //RefreshContainerSize();
        }

        /// <summary>
        /// Removes every existing connection UI, and re-creates the connections that should be visible.
        /// </summary>
        public void RefreshConnections()
        {
            ClearConnections();
            CreateConnections();
        }

        private void ClearConnections()
        {
            foreach( var conn in _visibleConnections )
            {
                conn.Destroy();
            }
            _visibleConnections.Clear();
        }

        private void CreateConnections()
        {
            // (achieved/2024-04-06): Intended to support many-to-many connections on both inputs and outputs.

            HashSet<(Control.Control input, Control.Control output)> connected = new HashSet<(Control.Control input, Control.Control output)>();

            foreach( var (input, inputUI) in _visibleInputs )
            {
                var connectedOutputs = input.GetConnectedControls();
                foreach( var output in connectedOutputs )
                {
                    if( connected.Contains( (input, output) ) )
                        continue;

                    if( _visibleOutputs.TryGetValue( output, out var outputUI ) )
                    {
                        ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.Create( this, inputUI, outputUI );
                        _visibleConnections.Add( connectionUI );
                    }
                    else
                    {
                        ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.CreateOpenEnded( this, inputUI, null, new Vector2( inputUI.Side > 0.5f ? ControlSetupControlConnectionUI.OPEN_ENDED_OFFSET : -ControlSetupControlConnectionUI.OPEN_ENDED_OFFSET, 0 ) );
                        _visibleConnections.Add( connectionUI );
                    }
                }
            }

            foreach( var (output, outputUI) in _visibleOutputs )
            {
                var connectedInputs = output.GetConnectedControls();
                foreach( var input in connectedInputs )
                {
                    if( connected.Contains( (input, output) ) )
                        continue;

                    if( _visibleInputs.TryGetValue( input, out var inputUI ) )
                    {
                        ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.Create( this, inputUI, outputUI );
                        _visibleConnections.Add( connectionUI );
                    }
                    else
                    {
                        ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.CreateOpenEnded( this, null, outputUI, new Vector2( outputUI.Side > 0.5f ? ControlSetupControlConnectionUI.OPEN_ENDED_OFFSET : -ControlSetupControlConnectionUI.OPEN_ENDED_OFFSET, 0 ) );
                        _visibleConnections.Add( connectionUI );
                    }
                }
            }
        }

        internal bool TryConnectWithMouse( ControlSetupControlUI endpoint, ControlSetupControlUI otherEndpoint )
        {
            if( endpoint == otherEndpoint )
                return false;

            if( endpoint.Control.TryConnect( otherEndpoint.Control ) )
            {
                RefreshConnections();

                return true;
            }

            return false;
        }

        internal bool TryDisconnectWithMouse( ControlSetupControlUI endpoint )
        {
            if( endpoint.Control.TryDisconnectAll() )
            {
                RefreshConnections();

                return true;
            }

            return false;
        }

        void OnDestroy()
        {
            _lastVisibleComponents = this._visibleComponents.Select( c => new LastVisibleEntry()
            {
                component = c.Key,
                lastAnchoredPosition = ((RectTransform)c.Value.transform).anchoredPosition
            } ).ToArray();
        }

        public static UIControlSetupWindow Create( Transform target )
        {
            _lastVisibleComponents = _lastVisibleComponents.Where( x => x.component != null ).ToArray(); // Removes components that were destroyed.

            if( !_lastVisibleComponents.Any() )
            {
                _lastVisibleComponents = target.GetComponentsInChildren()
                    .Where( c => ControlUtils.HasControlsOrGroups( c ) )
                    .Select( c => new LastVisibleEntry() { component = c } )
                    .ToArray();
            }

            UIControlSetupWindow controlWindow = (UIControlSetupWindow)UIWindow.Create<UIControlSetupWindow>( CanvasManager.Get( CanvasName.WINDOWS ), new UILayoutInfo( UIAnchor.Center, (0, 0), (750, 750) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/window" ) )
                .Draggable()
                .Focusable()
                .Resizeable()
                .WithCloseButton( new UILayoutInfo( UIAnchor.TopRight, (-7, -5), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            UIScrollView scrollView = controlWindow.AddScrollView( new UILayoutInfo( UIFill.Fill( 5, 5, 30, 5 ) ), (5000, 5000), true, true )
                .WithHorizontalScrollbar( UIAnchor.Bottom, 10, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_horizontal_background" ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_horizontal" ), out _ )
                .WithVerticalScrollbar( UIAnchor.Right, 10, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical_background" ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical" ), out _ );

            UIPanel topPanel = controlWindow.AddPanel( new UILayoutInfo( UIFill.Horizontal( 45, 45 ), UIAnchor.Top, 0, 30 ), null );
            UIButton componentListButton = topPanel.AddButton( new UILayoutInfo( UIAnchor.Left, (0, 0), (15, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_list_gold" ), null );

            UIPanel nodeLayerPanel = scrollView.AddPanel( new UILayoutInfo( UIAnchor.Center, 0, (0, 0) ), null );
            UIPanel connectionLayerPanel = scrollView.AddPanel( new UILayoutInfo( UIFill.Fill() ), null );

            controlWindow.Target = target;
            controlWindow.ComponentContainer = nodeLayerPanel;
            controlWindow.ConnectionContainer = connectionLayerPanel;
            controlWindow.scrollView = scrollView;

            componentListButton.onClick = () =>
            {
                if( !controlWindow._componentList.IsNullOrDestroyed() )
                    return;

                controlWindow._componentList = CreateAllComponentsContextMenu( controlWindow, componentListButton, target.GetComponentsInChildren()
                    .Where( c => ControlUtils.HasControlsOrGroups( c ) )
                    .ToArray() );
            };

            controlWindow.CreateNodes( _lastVisibleComponents );
            controlWindow.CreateConnections(); // All connections after the nodes, runs faster.

            return controlWindow;
        }

        private static UIContextMenu CreateAllComponentsContextMenu( UIControlSetupWindow window, UIButton targetButton, Component[] componentsWithControls )
        {
            UIContextMenu contextMenu = targetButton.rectTransform.CreateContextMenu( CanvasManager.Get( CanvasName.CONTEXT_MENUS ), new UILayoutInfo( UIAnchor.TopLeft, (0, 0), (200, 400) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/panel_light" ) );

            UIScrollView scrollView = contextMenu.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill() ), 1000 );

            float currentY = 0f;
            foreach( var component in componentsWithControls )
            {
                scrollView.AddButton( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, currentY, 15f ), null, () =>
                {
                    if( !window._visibleComponents.ContainsKey( component ) )
                    {
                        window.ShowComponent( component );
                    }
                } )
                    .AddStdText( new UILayoutInfo( UIFill.Fill() ), component.GetType().Name )
                    .WithFontColor( window._visibleComponents.ContainsKey( component ) ? Color.white : Color.green );

                currentY -= 15f;
            }

            return contextMenu;
        }
    }
}