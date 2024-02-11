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
    /// UI for components that have inputs/outputs or groups with inputs/outputs.
    /// </summary>
    public class ControlSetupWindowComponentUI : MonoBehaviour
    {
        // Component UIs, a.k.a. 'nodes' are the building blocks of the control setup UI.
        // Every one represents a single component present in the hierarchy of the target object.

        /// <summary>
        /// The window that this node belongs to.
        /// </summary>
        public ControlSetupWindow Window { get; private set; }

        /// <summary>
        /// The component that this node represents.
        /// </summary>
        public Component Component { get; private set; }
        
        /// <summary>
        /// The 'root' group of this component UI. Every node has one.
        /// </summary>
        public ControlSetupControlGroupUI Group { get; private set; }
        
        internal UIPanel panel; // this is ugly. UI should provide a way of creating new UI elements easily, without boilerplate, and with restrictions what can be a child, etc.

		public IEnumerable<ControlSetupControlUI> GetInputs()
        {
            return Group.GetInputsRecursive();
        }
        
		public IEnumerable<ControlSetupControlUI> GetOutputs()
        {
            return Group.GetOutputsRecursive();
        }

        public void Destroy()
        {
            this.panel.Destroy();
        }

        /// <summary>
        /// Creates a control setup node for a given component.
        /// </summary>
        internal static ControlSetupWindowComponentUI Create( ControlSetupWindow window, Component component )
        {
            // it is possible to force-show nodes for components outside of the target hierarchy of the window.

            UIPanel panel = window.Container.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, Vector2.zero, new Vector2( 100, 150 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_node" ) );

            panel.Raycastable( true );

            UIText text = panel.AddText( UILayoutInfo.FillHorizontal( 5, 5, UILayoutInfo.TopF, 0, 20 ), component.GetType().Name );

            RectTransformDragger dragger = panel.gameObject.AddComponent<RectTransformDragger>();
            dragger.UITransform = panel.rectTransform;
            
            ControlSetupWindowComponentUI node = panel.gameObject.AddComponent<ControlSetupWindowComponentUI>();
            node.panel = panel;
            node.Window = window;

            ControlSetupControlGroupUI groupUI = ControlSetupControlGroupUI.Create( node, component );
            node.Group = groupUI;

            return node;
        }
    }
}