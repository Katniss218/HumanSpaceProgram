using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager.UI;
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

        Dictionary<Control.Control, ControlSetupControlUI> _inputs = new();
        Dictionary<Control.Control, ControlSetupControlUI> _outputs = new();

        List<ControlSetupControlConnectionUI> _visibleConnections = new();

        private static LastVisibleEntry[] _lastVisibleComponents = new LastVisibleEntry[] { };

        public void ShowComponent( Component component )
        {
            // When showing or hiding a component (has to be done when toggling AND WHEN COMP IS REMOVED FROM PHYSICAL VESSEL)
            // take both inputs and outputs of the newly created component UI
            // for each of them
            // - remove "going to nothing" connections of their corresponding visible counterpart endpoints
            // - if added: create new connections for both inputs and outputs

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

                RefreshConnections();
            }
        }

        private bool TryCreateNode( LastVisibleEntry entryToShow, out ControlSetupWindowComponentUI node )
        {
            if( ControlUtils.HasControlsOrGroups( entryToShow.component ) )
            {
                node = ControlSetupWindowComponentUI.Create( this, entryToShow.component );
                _visibleComponents.Add( entryToShow.component, node );
                ((RectTransform)node.transform).anchoredPosition = entryToShow.lastAnchoredPosition;

                foreach( var input in node.GetInputs() )
                {
                    _inputs.Add( input.Control, input );
                }
                foreach( var output in node.GetOutputs() )
                {
                    _outputs.Add( output.Control, output );
                }
                return true;
            }
            node = null;
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
            foreach( var outputUI in _outputs.Values )
            {
#warning TODO - if nothing is connected - don't draw connection. If something is, but is
                if( !outputUI.Control.GetConnectedControls().Any( c => _inputs.ContainsKey( c ) ) )
                {
                    ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.CreateOpenEnded( this, null, outputUI, new Vector2( outputUI.Side > 0.5f ? 20f : -20f, 0 ) );
                    _visibleConnections.Add( connectionUI );
                }
                else
                {
                    foreach( var other in outputUI.Control.GetConnectedControls() )
                    {
                        if( _inputs.TryGetValue( other, out var inputUI ) )
                        {
                            ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.Create( this, inputUI, outputUI );
                            _visibleConnections.Add( connectionUI );
                        }
                    }
                }
            }
            foreach( var inputUI in _inputs.Values )
            {
                if( !inputUI.Control.GetConnectedControls().Any( c => _outputs.ContainsKey( c ) ) )
                {
                    ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.CreateOpenEnded( this, inputUI, null, new Vector2( inputUI.Side > 0.5f ? 20f : -20f, 0 ) );
                    _visibleConnections.Add( connectionUI );
                }
            }
        }

        internal bool TryConnectWithMouse( ControlSetupControlConnectionUI mouseConnection, ControlSetupControlUI otherEndpoint )
        {
            if( !mouseConnection.IsOpenEnded )
            {
                return false;
            }

            if( mouseConnection.GetClosedEnd().Control.TryConnect( otherEndpoint.Control ) )
            {
                RefreshConnections();

                return true;
            }

            return false;
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
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 750, 750 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            // TODO - resizable windows.

            UIScrollView scrollView = window.AddScrollView( UILayoutInfo.Fill( 5, 5, 30, 5 ), new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 750, 750 ) ), true, true );

            UIPanel nodeLayerPanel = scrollView.AddPanel( UILayoutInfo.Fill(), null );
            UIPanel connectionLayerPanel = scrollView.AddPanel( UILayoutInfo.Fill(), null );

            ControlSetupWindow w = window.gameObject.AddComponent<ControlSetupWindow>();
            w.Target = target;
            w.window = window;
            w._scrollView = scrollView;
            w.ComponentContainer = nodeLayerPanel;
            w.ConnectionContainer = connectionLayerPanel;

            _lastVisibleComponents = _lastVisibleComponents.Where( x => x.component != null ).ToArray(); // Removes components that were destroyed.
            if( !_lastVisibleComponents.Any() )
            {
                _lastVisibleComponents = target.GetComponentsInChildren()
                    .Where( c => ControlUtils.HasControlsOrGroups( c ) )
                    .Select( c => new LastVisibleEntry() { component = c } ).ToArray();
            }

            w.CreateNodes( _lastVisibleComponents );
            w.CreateConnections(); // Connections should be created after the component nodes are created. This ensures that all inputs/outputs are created.

            return w;
        }
    }
}