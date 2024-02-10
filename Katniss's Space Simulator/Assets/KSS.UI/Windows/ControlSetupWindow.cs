using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    public class ControlSetupWindow : MonoBehaviour
    {
        // window where the player can set up the control systems of the part/vessel.

        // control setup window consists of "nodes", representing parts that can be controlled, or can control other parts
        // as well as editable connections between them.

        // each node represents a Unity component.

        Transform _target; // defines what will be displayed in the window.
        internal UIWindow window;
        private UIScrollView _scrollView;
        public IUIElementContainer Container => _scrollView;

        // node inputs/outputs might be connected to invisible (in the window) stuff, if it's outside of the hierarchy.
        // - that means we should show the connection at the visible input, and "cut it off" shortly after.

        Dictionary<Component, ControlSetupWindowNodeUI> _nodes = new();

        Dictionary<Control.Control, ControlSetupControlUI> _endpoints = new();

        List<ControlSetupControlConnectionUI> _connections = new();

        // nodes may be connected while the window is open, we need to account for that.

        private void RefreshNodesAndConnections()
        {
            foreach( var node in _nodes )
            {
                node.Destroy();
            }
            foreach( var connection in _connections )
            {
                connection.Destroy();
            }

            Component[] components = _target.GetComponentsInChildren();

            foreach( var comp in components )
            {
                if( Control.ControlUtils.HasControls( comp ) )
                {
                    ControlSetupWindowNodeUI node = ControlSetupWindowNodeUI.Create( this, comp );
                    _nodes.Add( node );
                }
            }
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
            w._target = target;
            w.window = window;
            w._scrollView = scrollView;

            w.RefreshNodesAndConnections();

            // window is created
            // look up which components were visible before
            // spawn uis for those components

            // for each input/output 'circle', add it to the dict to lookup endpoint visibility later.

            // after all visible component UIs are spawned
            // spawn connections.

            // for outputs (endpoints with multiple outgoing connections)
            // - spawn all connections (connected if both are visible, "going to nothing" otherwise)

            // for inputs
            // - only spawn "going to nothing" connections (since the rest will be covered by the visible outputs).


            // When adding or removing visibility of a component (has to be done when toggling AND WHEN COMP IS REMOVED FROM PHYSICAL VESSEL)
            // take both inputs and outputs of the newly created component UI
            // for each of them
            // - remove "going to nothing" connections of their corresponding visible counterpart endpoints
            // - create new connections for both inputs and outputs
            // - - so we do have to have a capability to create connections "when going backwards" too.


            return w;
        }
    }
}