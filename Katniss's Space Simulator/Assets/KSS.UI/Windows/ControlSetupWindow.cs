using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Extensions;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    public class ControlSetupWindow : MonoBehaviour
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

        internal UIWindow window;
        private UIScrollView _scrollView;
        internal IUIElementContainer ComponentContainer { get; private set; }
        internal IUIElementContainer ConnectionContainer { get; private set; }

        Dictionary<Component, ControlSetupWindowComponentUI> _visibleComponents = new();

        Dictionary<Control.Control, ControlSetupControlUI> _visibleInputs = new();
        Dictionary<Control.Control, ControlSetupControlUI> _visibleOutputs = new();

        List<ControlSetupControlConnectionUI> _visibleConnections = new();

        private static LastVisibleEntry[] _lastVisibleComponents = new LastVisibleEntry[] { };

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

        internal bool TryConnectWithMouse( ControlSetupControlUI firstEndpoint, ControlSetupControlUI otherEndpoint )
        {
            if( firstEndpoint == otherEndpoint )
                return false;

            if( firstEndpoint.Control.TryConnect( otherEndpoint.Control ) )
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
            window.Destroy();
        }

        public static ControlSetupWindow Create( Transform target )
        {
            _lastVisibleComponents = _lastVisibleComponents.Where( x => x.component != null ).ToArray(); // Removes components that were destroyed.

            if( !_lastVisibleComponents.Any() )
            {
                _lastVisibleComponents = target.GetComponentsInChildren()
                    .Where( c => ControlUtils.HasControlsOrGroups( c ) )
                    .Select( c => new LastVisibleEntry() { component = c } )
                    .ToArray();
            }

            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 750, 750 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            // TODO - resizable windows.

            UIScrollView scrollView = window.AddScrollView( UILayoutInfo.Fill( 5, 5, 30, 5 ), new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 750, 750 ) ), true, true );

            UIPanel topPanel = window.AddPanel( UILayoutInfo.FillHorizontal( 45, 45, UILayoutInfo.TopF, 0, 30 ), null );
            UIButton btn = topPanel.AddButton( new UILayoutInfo( UILayoutInfo.Left, Vector2.zero, new Vector2( 15, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_list_gold" ), null );

            UIPanel nodeLayerPanel = scrollView.AddPanel( UILayoutInfo.Fill(), null );
            UIPanel connectionLayerPanel = scrollView.AddPanel( UILayoutInfo.Fill(), null );

            ControlSetupWindow w = window.gameObject.AddComponent<ControlSetupWindow>();
            w.Target = target;
            w.window = window;
            w._scrollView = scrollView;
            w.ComponentContainer = nodeLayerPanel;
            w.ConnectionContainer = connectionLayerPanel;

            btn.onClick = () =>
            {
                CreateAllComponentsContextMenu( w, btn, target.GetComponentsInChildren()
                    .Where( c => ControlUtils.HasControlsOrGroups( c ) )
                    .ToArray() );
            };

            w.CreateNodes( _lastVisibleComponents );
            w.CreateConnections(); // Connections should be created after the component nodes are created. This ensures that all inputs/outputs are created.

            return w;
        }

        private static UIContextMenu CreateAllComponentsContextMenu( ControlSetupWindow window, UIButton targetButton, Component[] componentsWithControls )
        {
            UIContextMenu cm = targetButton.rectTransform.CreateContextMenu( CanvasManager.Get( CanvasName.CONTEXT_MENUS ), new UILayoutInfo( UILayoutInfo.TopLeft, Vector2.zero, new Vector2( 200, 400 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_entry_background" ) );
            // create context menu with the elements.
            // - each component: name and whether already shown. if not shown, click to show.

            UIScrollView sv = cm.AddVerticalScrollView( UILayoutInfo.Fill(), 1000 );

            float currentY = 0f;
            foreach( var comp in componentsWithControls )
            {
                sv.AddButton( UILayoutInfo.FillHorizontal( 0, 0, UILayoutInfo.TopF, currentY, 15f ), null, () =>
                {
                    if( !window._visibleComponents.ContainsKey( comp ) )
                    {
                        window.ShowComponent( comp );
                    }
                } )
                    .AddText( UILayoutInfo.Fill(), comp.GetType().Name )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, window._visibleComponents.ContainsKey(comp) ? Color.white : Color.green );

                currentY -= 15f;
            }

            return cm;
        }
    }
}