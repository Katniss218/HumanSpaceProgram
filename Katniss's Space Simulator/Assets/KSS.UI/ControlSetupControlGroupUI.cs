using KSS.Control;
using KSS.Control.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class ControlSetupControlGroupUI : MonoBehaviour
    {
        ControlSetupControlGroupUI _group;
        ControlSetupWindowNode _node;

        object _target;
        NamedControlAttribute _attr;

        ControlGroup _controlGroup => (ControlGroup)_target;

        ControlSetupControlUI[] _controlUIs;

        ControlSetupControlGroupUI[] _groupUIs;

        public int Height { get; private set; }

        internal UIPanel panel; // this is ugly. UI should provide a way of creating new UI elements easily, without boilerplate, and with restrictions what can be a child, etc.


        private void RedrawControls()
        {
            IEnumerable<(object member, NamedControlAttribute attr)> controls = ControlUtils.GetControls( _target );

            List<ControlSetupControlUI> controlUIs = new();
            List<ControlSetupControlGroupUI> groupUIs = new();

            int inputIndex = 0;
            int outputIndex = 0;
            foreach( var (member, attr) in controls )
            {
                if( member is ControlGroup group )
                {
                    inputIndex = Mathf.Max( inputIndex, outputIndex );
                    outputIndex = inputIndex;

                    var groupUI = ControlSetupControlGroupUI.Create( this, inputIndex * 10f, group, attr );
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
                        var groupUI = ControlSetupControlGroupUI.Create( this, (inputIndex + i) * 10f, groupArray[i], attr );
                        groupUIs.Add( groupUI );

                        inputIndex += groupUI.Height;
                        outputIndex += groupUI.Height;
                    }
                }

                //

                if( member is ControlleeInput input )
                {
                    controlUIs.Add( ControlSetupControlUI.Create( this, 0f, inputIndex * 10f, input, attr ) );

                    inputIndex += 1;
                }
                if( member is ControlleeInput[] inputArray )
                {
                    for( int i = 0; i < inputArray.Length; i++ )
                    {
                        controlUIs.Add( ControlSetupControlUI.Create( this, 0f, (inputIndex + i) * 10f, inputArray[i], attr ) );
                    }

                    inputIndex += inputArray.Length;
                }

                //

                if( member is ControllerOutput output )
                {
                    controlUIs.Add( ControlSetupControlUI.Create( this, 1f, outputIndex * 10f, output, attr ) );

                    outputIndex += 1;
                }
                if( member is ControllerOutput[] outputArray )
                {
                    for( int i = 0; i < outputArray.Length; i++ )
                    {
                        controlUIs.Add( ControlSetupControlUI.Create( this, 1f, (outputIndex + i) * 10f, outputArray[i], attr ) );
                    }

                    outputIndex += outputArray.Length;
                }

                //

                if( member is ControlParameterInput paramInput )
                {
                    controlUIs.Add( ControlSetupControlUI.Create( this, 0f, inputIndex * 10f, paramInput, attr ) );

                    inputIndex += 1;
                }
                if( member is ControlParameterInput[] paramInputArray )
                {
                    for( int i = 0; i < paramInputArray.Length; i++ )
                    {
                        controlUIs.Add( ControlSetupControlUI.Create( this, 0f, (inputIndex + i) * 10f, paramInputArray[i], attr ) );
                    }

                    inputIndex += paramInputArray.Length;
                }

                //

                if( member is ControlParameterOutput paramOutput )
                {
                    controlUIs.Add( ControlSetupControlUI.Create( this, 1f, outputIndex * 10f, paramOutput, attr ) );

                    outputIndex += 1;
                }
                if( member is ControlParameterOutput[] paramOutputArray )
                {
                    for( int i = 0; i < paramOutputArray.Length; i++ )
                    {
                        controlUIs.Add( ControlSetupControlUI.Create( this, 1f, (outputIndex + i) * 10f, paramOutputArray[i], attr ) );
                    }

                    outputIndex += paramOutputArray.Length;
                }
            }

            Height = Mathf.Max( inputIndex, outputIndex );

            _controlUIs = controlUIs.ToArray();
            _groupUIs = groupUIs.ToArray();
        }


        internal static ControlSetupControlGroupUI Create( ControlSetupWindowNode node, float verticalOffset, object target, NamedControlAttribute attr )
        {
            UIPanel panel = node.panel.AddPanel( new UILayoutInfo( UILayoutInfo.TopLeft, UILayoutInfo.TopLeft, new Vector2( 0, verticalOffset ), new Vector2( 10, 10 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" ) );

            ControlSetupControlGroupUI groupUI = panel.gameObject.AddComponent<ControlSetupControlGroupUI>();
            groupUI.panel = panel;
            groupUI._target = target;
            groupUI._attr = attr;
            groupUI._node = node;

            groupUI.RedrawControls();

            return groupUI;
        }

        internal static ControlSetupControlGroupUI Create( ControlSetupControlGroupUI group, float verticalOffset, object target, NamedControlAttribute attr )
        {
            UIPanel panel = group.panel.AddPanel( new UILayoutInfo( UILayoutInfo.TopLeft, UILayoutInfo.TopLeft, new Vector2( 0, verticalOffset ), new Vector2( 10, 10 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" ) );

            ControlSetupControlGroupUI groupUI = panel.gameObject.AddComponent<ControlSetupControlGroupUI>();
            groupUI.panel = panel;
            groupUI._target = target;
            groupUI._attr = attr;
            groupUI._group = group;

            groupUI.RedrawControls();

            group.Height

            return groupUI;
        }
    }
}