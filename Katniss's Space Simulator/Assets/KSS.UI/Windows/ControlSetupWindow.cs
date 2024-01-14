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

        // node inputs/outputs might be connected to invisible (in the window) stuff, if it's outside of the hierarchy.

        List<ControlSetupWindowNode> _nodes = new List<ControlSetupWindowNode>();
        List<ControlSetupWindowNodeConnection> _connections = new List<ControlSetupWindowNodeConnection>();

        internal void AddNode( ControlSetupWindowNode node )
        {
            _nodes.Add( node );
        }

        internal void AddConnection( ControlSetupWindowNodeConnection conn )
        {
            _connections.Add( conn );
        }

        private void RefreshNodes()
        {
            // get all components that should be shown (have control in or out)
            // remove nodes that don't exist in the list.
            // add the new nodes.
        }

        public static ControlSetupWindow Create( Transform target )
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 350f, 400f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
                .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            ControlSetupWindow w = window.gameObject.AddComponent<ControlSetupWindow>();
            w._target = target;
            w.window = window;

            return w;
        }
    }
}