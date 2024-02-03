using KSS.Control;
using KSS.Control.Controls;
using KSS.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    /// <summary>
    /// Represents a component that has controls or control groups.
    /// </summary>
    public class ControlSetupWindowNode : MonoBehaviour
    {
        /// <summary>
        /// The window that this node belongs to.
        /// </summary>
        public ControlSetupWindow Window { get; private set; }

        /// <summary>
        /// The component that this node represents.
        /// </summary>
        public Component Component { get; private set; }

        // rendering takes into account the field order (I think the return order does).

        // inputs are rendered one under the other, same as outputs.
        // when a group is rendered, it resets the height of both to its bottom position.

        internal UIPanel panel; // this is ugly. UI should provide a way of creating new UI elements easily, without boilerplate, and with restrictions what can be a child, etc.

        public void Destroy()
        {
            this.panel.Destroy();
        }

        /// <summary>
        /// Creates a control setup node for a given component.
        /// </summary>
        internal static ControlSetupWindowNode Create( ControlSetupWindow window, Component component )
        {
            // it is possible to force-show nodes for components outside of the target hierarchy of the window.

            UIPanel panel = window.Container.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, Vector2.zero, new Vector2( 50, 50 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_node" ) );

            RectTransformDragger dragger = panel.gameObject.AddComponent<RectTransformDragger>();
            dragger.UITransform = panel.rectTransform;

            ControlSetupWindowNode node = panel.gameObject.AddComponent<ControlSetupWindowNode>();
            node.panel = panel;
            node.Window = window;

            ControlSetupControlGroupUI groupUI = ControlSetupControlGroupUI.Create( node, 0, component, null );

            panel.AddText( UILayoutInfo.Fill(), component.GetType().Name );

            return node;
        }
    }
}