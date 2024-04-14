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
        public UIControlSetupWindow Window { get; private set; }

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

        private void OnDragged()
        {
            Window.RefreshConnectionPositions();
        }

        /// <summary>
        /// Creates a control setup node for a given component.
        /// </summary>
        internal static ControlSetupWindowComponentUI Create( UIControlSetupWindow window, Component component )
        {
            // it is possible to force-show nodes for components outside of the target hierarchy of the window.

            UIPanel panel = window.ComponentContainer.AddPanel( new UILayoutInfo( UIAnchor.Center, (0, 0), (150, 150) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_node" ) );

            panel.Raycastable( true );

            ControlSetupWindowComponentUI node = panel.gameObject.AddComponent<ControlSetupWindowComponentUI>();
            node.panel = panel;
            node.Window = window;

            ControlSetupControlGroupUI groupUI = ControlSetupControlGroupUI.Create( node, component );
            node.Group = groupUI;
            node.Component = component;

            panel.rectTransform.sizeDelta = new Vector2( panel.rectTransform.sizeDelta.x, ((RectTransform)groupUI.transform).sizeDelta.y + 20f );

            RectTransformDragMove dragger = panel.gameObject.AddComponent<RectTransformDragMove>();
            dragger.UITransform = panel.rectTransform;
            dragger.OnDragging = node.OnDragged;

            UIText text = panel.AddText( new UILayoutInfo( UIFill.Horizontal( 5, 5 ), UIAnchor.Top, 0, 20 ), component.GetType().Name );

            panel.AddButton( new UILayoutInfo( UIAnchor.TopRight, (0, 0), (20, 20) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_node_close" ), () =>
            {
                window.HideComponent( component );
            } );

            return node;
        }
    }
}