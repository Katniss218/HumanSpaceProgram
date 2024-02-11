using KSS.Control;
using KSS.Control.Controls;
using KSS.UI.Windows;
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
	/// <summary>
	/// UI for <see cref="ControlGroup"/>-derived classes. <br/>
	/// As well as that, every component UI has its implicit group to reduce code duplication.
	/// </summary>
	public class ControlSetupControlGroupUI : MonoBehaviour
	{
		// Groups are what actually directly contain the inputs/outputs.

		public ControlSetupControlGroupUI ParentGroup { get; private set; }

		public ControlSetupWindowComponentUI Node { get; private set; }

		public int Height { get; private set; }

		object _target; // control group instance or component instance.
		NamedControlAttribute _attr;

		ControlSetupControlUI[] _inputUIs;
		ControlSetupControlUI[] _outputUIs;

		ControlSetupControlGroupUI[] _childGroups;

		internal UIPanel panel; // this is ugly. UI should provide a way of creating new UI elements easily, without boilerplate, and with restrictions what can be a child, etc.

		internal IEnumerable<ControlSetupControlUI> GetInputsRecursive()
		{
			List<ControlSetupControlUI> inputs = new List<ControlSetupControlUI>();

			inputs.AddRange( _inputUIs );

			foreach( var group in _childGroups )
			{
				inputs.AddRange( group.GetInputsRecursive() );
			}
			return inputs;
		}

		internal IEnumerable<ControlSetupControlUI> GetOutputsRecursive()
		{
			List<ControlSetupControlUI> outputs = new List<ControlSetupControlUI>();

			outputs.AddRange( _outputUIs );

			foreach( var group in _childGroups )
			{
				outputs.AddRange( group.GetInputsRecursive() );
			}
			return outputs;
		}

		private void RedrawControls()
		{
			IEnumerable<(object member, NamedControlAttribute attr)> controls = ControlUtils.GetControlsAndGroups( _target );

			List<ControlSetupControlUI> inputUIs = new();
			List<ControlSetupControlUI> outputUIs = new();
			List<ControlSetupControlGroupUI> childGroups = new();

			int inputIndex = 0;
			int outputIndex = 0;
			foreach( var (member, attr) in controls )
			{
				if( member is ControlGroup group )
				{
					inputIndex = Mathf.Max( inputIndex, outputIndex );
					outputIndex = inputIndex;

					var groupUI = ControlSetupControlGroupUI.Create( this, inputIndex * ControlSetupControlUI.HEIGHT, group, attr );
					childGroups.Add( groupUI );

					inputIndex += groupUI.Height;
					outputIndex += groupUI.Height;
				}
				if( member is ControlGroup[] groupArray )
				{
					inputIndex = Mathf.Max( inputIndex, outputIndex );
					outputIndex = inputIndex;

					for( int i = 0; i < groupArray.Length; i++ )
					{
#warning TODO - group arrays should be initialized
						var groupUI = ControlSetupControlGroupUI.Create( this, (inputIndex + i) * ControlSetupControlUI.HEIGHT, groupArray[i], attr );
						childGroups.Add( groupUI );

						inputIndex += groupUI.Height;
						outputIndex += groupUI.Height;
					}
				}

				//

				if( member is ControlleeInput input )
				{
					inputUIs.Add( ControlSetupControlUI.Create( this, 0f, inputIndex * ControlSetupControlUI.HEIGHT, input, attr ) );

					inputIndex += 1;
					outputIndex += 1;
				}
				if( member is ControlleeInput[] inputArray )
				{
					for( int i = 0; i < inputArray.Length; i++ )
					{
						inputUIs.Add( ControlSetupControlUI.Create( this, 0f, (inputIndex + i) * ControlSetupControlUI.HEIGHT, inputArray[i], attr ) );
					}

					inputIndex += inputArray.Length;
					outputIndex += inputArray.Length;
				}

				//

				if( member is ControllerOutput output )
				{
					outputUIs.Add( ControlSetupControlUI.Create( this, 1f, outputIndex * ControlSetupControlUI.HEIGHT, output, attr ) );

					inputIndex += 1;
					outputIndex += 1;
				}
				if( member is ControllerOutput[] outputArray )
				{
					for( int i = 0; i < outputArray.Length; i++ )
					{
						outputUIs.Add( ControlSetupControlUI.Create( this, 1f, (outputIndex + i) * ControlSetupControlUI.HEIGHT, outputArray[i], attr ) );
					}

					inputIndex += outputArray.Length;
					outputIndex += outputArray.Length;
				}

				//

				if( member is ControlParameterInput paramInput )
				{
					inputUIs.Add( ControlSetupControlUI.Create( this, 0f, inputIndex * ControlSetupControlUI.HEIGHT, paramInput, attr ) );

					inputIndex += 1;
					outputIndex += 1;
				}
				if( member is ControlParameterInput[] paramInputArray )
				{
					for( int i = 0; i < paramInputArray.Length; i++ )
					{
						inputUIs.Add( ControlSetupControlUI.Create( this, 0f, (inputIndex + i) * ControlSetupControlUI.HEIGHT, paramInputArray[i], attr ) );
					}

					inputIndex += paramInputArray.Length;
					outputIndex += paramInputArray.Length;
				}

				//

				if( member is ControlParameterOutput paramOutput )
				{
					outputUIs.Add( ControlSetupControlUI.Create( this, 1f, outputIndex * ControlSetupControlUI.HEIGHT, paramOutput, attr ) );

					inputIndex += 1;
					outputIndex += 1;
				}
				if( member is ControlParameterOutput[] paramOutputArray )
				{
					for( int i = 0; i < paramOutputArray.Length; i++ )
					{
						outputUIs.Add( ControlSetupControlUI.Create( this, 1f, (outputIndex + i) * ControlSetupControlUI.HEIGHT, paramOutputArray[i], attr ) );
					}

					inputIndex += paramOutputArray.Length;
					outputIndex += paramOutputArray.Length;
				}
			}

			Height = Mathf.Max( inputIndex, outputIndex );

			_inputUIs = inputUIs.ToArray();
			_outputUIs = outputUIs.ToArray();
			_childGroups = childGroups.ToArray();

			foreach( var cui in this._inputUIs )
			{
				this.Node.Window.RegisterInput( cui.Control, cui );
			}
			foreach( var cui in this._outputUIs )
			{
				this.Node.Window.RegisterOutput( cui.Control, cui );
			}
		}


		internal static ControlSetupControlGroupUI Create( ControlSetupWindowComponentUI node, Component component )
		{
			UIPanel panel = node.panel.AddPanel( UILayoutInfo.Fill( 0, 0, 20, 5 ), null ); // AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" )

			ControlSetupControlGroupUI groupUI = panel.gameObject.AddComponent<ControlSetupControlGroupUI>();
			groupUI.panel = panel;
			groupUI._target = component;
			groupUI._attr = null;
			groupUI.Node = node;

			groupUI.RedrawControls();

			return groupUI;
		}

		internal static ControlSetupControlGroupUI Create( ControlSetupControlGroupUI group, float verticalOffset, object target, NamedControlAttribute attr )
		{
			UIPanel panel = group.panel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, UILayoutInfo.TopF, -verticalOffset, ControlSetupControlUI.HEIGHT ), null );

			ControlSetupControlGroupUI groupUI = panel.gameObject.AddComponent<ControlSetupControlGroupUI>();
			groupUI.panel = panel;
			groupUI._target = target;
			groupUI._attr = attr;
			groupUI.ParentGroup = group;
			groupUI.Node = group.Node;

			groupUI.RedrawControls();

			panel.rectTransform.sizeDelta = new Vector2( panel.rectTransform.sizeDelta.y, groupUI.Height * ControlSetupControlUI.HEIGHT );

			return groupUI;
		}
	}
}