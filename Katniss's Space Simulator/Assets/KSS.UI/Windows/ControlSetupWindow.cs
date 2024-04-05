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
        /// <summary>
        /// Defines the possible components that can be displayed in the window.
        /// </summary>
        public Transform Target { get; private set; }

        internal UIWindow window;
        private UIScrollView _scrollView;
        internal IUIElementContainer Container => _scrollView;

        IEnumerable<Component> visibleComponents => _nodes.Keys;

        Dictionary<Component, ControlSetupWindowComponentUI> _nodes = new();

        Dictionary<Control.Control, ControlSetupControlUI> _inputs = new();
        Dictionary<Control.Control, ControlSetupControlUI> _outputs = new();

        List<ControlSetupControlConnectionUI> _connections = new();

        private static Component[] _lastVisibleComponents = new Component[] { };


        ControlSetupControlConnectionUI _mouseDraggedConnection = null;
        bool IsDragging => _mouseDraggedConnection != null;

        public void ShowComponent( Component component )
        {
            // When showing or hiding a component (has to be done when toggling AND WHEN COMP IS REMOVED FROM PHYSICAL VESSEL)
            // take both inputs and outputs of the newly created component UI
            // for each of them
            // - remove "going to nothing" connections of their corresponding visible counterpart endpoints
            // - if added: create new connections for both inputs and outputs

            if( Target.IsAncestorOf( component.transform ) )
            {
                if( TryCreateNode( component, out _ ) )
                {
                    RefreshConnections(); // much easier and actually works.
                }
            }
        }

        public void HideComponent( Component component )
        {
            if( _nodes.TryGetValue( component, out var componentUI ) )
            {
                componentUI.Destroy();
                _nodes.Remove( component );

                RefreshConnections();
            }
        }

        private bool TryCreateNode( Component componentToShow, out ControlSetupWindowComponentUI node )
        {
            if( ControlUtils.HasControlsOrGroups( componentToShow ) )
            {
                node = ControlSetupWindowComponentUI.Create( this, componentToShow );
                _nodes.Add( componentToShow, node );
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

        private void CreateNodes( IEnumerable<Component> componentsToShow )
        {
            foreach( var comp in componentsToShow )
            {
                if( ControlUtils.HasControlsOrGroups( comp ) )
                {
                    TryCreateNode( comp, out _ );
                }
            }
        }

        internal void RefreshConnections()
        {
            ClearConnections();
            CreateConnections();
        }

        private void ClearConnections()
        {
            foreach( var conn in _connections )
            {
                conn.Destroy();
            }
            _connections.Clear();
        }

        private void CreateConnections()
        {
            // for outputs (endpoints with multiple outgoing connections)
            // - spawn all connections (connected if both are visible, "going to nothing" otherwise)
            // for inputs
            // - only spawn "going to nothing" connections (since the rest will be covered by the visible outputs).

            foreach( var outputUI in _outputs.Values )
            {
                foreach( var other in outputUI.Control.GetConnectedControls() )
                {
                    if( _inputs.TryGetValue( other, out var inputUI ) )
                    {
                        ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.Create( this, inputUI, outputUI );
                        _connections.Add( connectionUI );
                    }
                    else
                    {
                        ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.CreateOpenEnded( this, null, outputUI, new Vector2( 20, 0 ) );
                        _connections.Add( connectionUI );
                    }
                }
            }
            foreach( var inputUI in _inputs.Values )
            {
                if( !inputUI.Control.GetConnectedControls().Any( c => _outputs.ContainsKey( c ) ) )
                {
                    ControlSetupControlConnectionUI connectionUI = ControlSetupControlConnectionUI.CreateOpenEnded( this, inputUI, null, new Vector2( -20, 0 ) );
                    _connections.Add( connectionUI );
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

        public void Destroy()
        {
            // not called
            _lastVisibleComponents = this.visibleComponents.ToArray();
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

            ControlSetupWindow w = window.gameObject.AddComponent<ControlSetupWindow>();
            w.Target = target;
            w.window = window;
            w._scrollView = scrollView;

            _lastVisibleComponents = _lastVisibleComponents.Where( x => x != null ).ToArray(); // Removes components that were destroyed.
            if( !_lastVisibleComponents.Any() )
            {
                _lastVisibleComponents = target.GetComponentsInChildren();
            }

            w.CreateNodes( _lastVisibleComponents );
            w.CreateConnections(); // Connections should be created after the component nodes are created. This ensures that all inputs/outputs are created.

            return w;
        }
    }
}