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

        ControlSetupControleeInput[] _inputUIs;
        ControlSetupParameterInput[] _inputParamUIs;

        ControlSetupControlerOutput[] _outputUIs;
        ControlSetupParameterOutput[] _outputParamUIs;

#warning TODO - programatically, a node would be the same as a group. Groups can be nested.
        ControlSetupWindowNodeGroup[] _groupUIs;

        public int Height { get; private set; }

        // rendering takes into account the field order (I think the return order does).

        // inputs are rendered one under the other, same as outputs.
        // when a group is rendered, it resets the height of both to its bottom position.

        internal UIPanel panel; // this is ugly. UI should provide a way of creating new UI elements easily, without boilerplate, and with restrictions what can be a child, etc.

        private void RedrawControls()
        {
            IEnumerable<(object member, NamedControlAttribute attr)> controls = ControlUtils.GetControls( Component );

            List<ControlSetupControleeInput> inputUIs = new();
            List<ControlSetupParameterInput> inputParamUIs = new();
            List<ControlSetupControlerOutput> outputUIs = new();
            List<ControlSetupParameterOutput> outputParamUIs = new();
            List<ControlSetupWindowNodeGroup> groupUIs = new();

            int inputIndex = 0;
            int outputIndex = 0;
            foreach( var (member, attr) in controls )
            {
                if( member is ControlGroup group )
                {
                    inputIndex = Mathf.Max( inputIndex, outputIndex );
                    outputIndex = inputIndex;

                    var groupUI = ControlSetupWindowNodeGroup.Create( this, inputIndex * 10f, group, attr );
                    groupUIs.Add( groupUI );

                    inputIndex += groupUI.Height;
                    outputIndex += groupUI.Height;
                }
                if( member is ControlGroup[] groupArray )
                {
                    inputIndex = Mathf.Max( inputIndex, outputIndex );
                    outputIndex = inputIndex;

                    for( int i = 0; i < groupArray.Length; i++ )
                    {
                        var groupUI = ControlSetupWindowNodeGroup.Create( this, (inputIndex + i) * 10f, groupArray[i], attr );
                        groupUIs.Add( groupUI );

                        inputIndex += groupUI.Height;
                        outputIndex += groupUI.Height;
                    }
                }

                //

                if( member is ControlleeInput input )
                {
                    inputUIs.Add( ControlSetupControleeInput.Create( this, inputIndex * 10f, input, attr ) );

                    inputIndex += 1;
                }
                if( member is ControlleeInput[] inputArray )
                {
                    for( int i = 0; i < inputArray.Length; i++ )
                    {
                        inputUIs.Add( ControlSetupControleeInput.Create( this, (inputIndex + i) * 10f, inputArray[i], attr ) );
                    }

                    inputIndex += inputArray.Length;
                }

                //

                if( member is ControllerOutput output )
                {
                    outputUIs.Add( ControlSetupControlerOutput.Create( this, outputIndex * 10f, output, attr ) );

                    outputIndex += 1;
                }
                if( member is ControllerOutput[] outputArray )
                {
                    for( int i = 0; i < outputArray.Length; i++ )
                    {
                        outputUIs.Add( ControlSetupControlerOutput.Create( this, (outputIndex + i) * 10f, outputArray[i], attr ) );
                    }

                    outputIndex += outputArray.Length;
                }

                //

                if( member is ControlParameterInput paramInput )
                {
                    inputParamUIs.Add( ControlSetupParameterInput.Create( this, inputIndex * 10f, paramInput, attr ) );

                    inputIndex += 1;
                }
                if( member is ControlParameterInput[] paramInputArray )
                {
                    for( int i = 0; i < paramInputArray.Length; i++ )
                    {
                        inputParamUIs.Add( ControlSetupParameterInput.Create( this, (inputIndex + i) * 10f, paramInputArray[i], attr ) );
                    }

                    inputIndex += paramInputArray.Length;
                }

                //

                if( member is ControlParameterOutput paramOutput )
                {
                    outputParamUIs.Add( ControlSetupParameterOutput.Create( this, outputIndex * 10f, paramOutput, attr ) );

                    outputIndex += 1;
                }
                if( member is ControlParameterOutput[] paramOutputArray )
                {
                    for( int i = 0; i < paramOutputArray.Length; i++ )
                    {
                        outputParamUIs.Add( ControlSetupParameterOutput.Create( this, (outputIndex + i) * 10f, paramOutputArray[i], attr ) );
                    }

                    outputIndex += paramOutputArray.Length;
                }
            }

            Height = Mathf.Max( inputIndex, outputIndex );

            _inputUIs = inputUIs.ToArray();
            _outputUIs = outputUIs.ToArray();
            _inputParamUIs = inputParamUIs.ToArray();
            _outputParamUIs = outputParamUIs.ToArray();
            _groupUIs = groupUIs.ToArray();
        }

        /// <summary>
        /// Creates a control setup node for a given component.
        /// </summary>
        internal static ControlSetupWindowNode Create( ControlSetupWindow window, Component component )
        {
            // it is possible to force-show nodes for components outside of the target hierarchy of the window.

            UIPanel panel = window.window.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, Vector2.zero, new Vector2( 50, 50 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_node" ) );

            ControlSetupWindowNode node = panel.gameObject.AddComponent<ControlSetupWindowNode>();
            node.panel = panel;
            node.RedrawControls();
            node.Window = window;

            return node;
        }
    }
}