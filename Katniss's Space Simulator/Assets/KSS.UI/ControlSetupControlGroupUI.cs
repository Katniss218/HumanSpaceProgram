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
		
        public const float ROW_HEIGHT = 15.0f;

		/// <summary>
		/// The group that is the parent of this group. May be null.
		/// </summary>
		public ControlSetupControlGroupUI ParentGroup { get; private set; }

		/// <summary>
		/// The component UI that this group belongs to. Every nested group belongs to the same node.
		/// </summary>
		public ControlSetupWindowComponentUI ComponentUI { get; private set; }

		/// <summary>
		/// The number of rows taken up by the inputs and outputs.
		/// </summary>
		public int TotalRows { get; private set; }

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

			int currentRow = 0;
			foreach( var (member, attr) in controls )
			{
				if( member is ControlGroup group )
				{
					var groupUI = ControlSetupControlGroupUI.Create( this, currentRow * ROW_HEIGHT, group, attr );
					childGroups.Add( groupUI );
					currentRow += groupUI.TotalRows;
				}
				if( member is ControlGroup[] groupArray )
				{
					for( int i = 0; i < groupArray.Length; i++ )
					{
#warning TODO - group arrays should be initialized
						var groupUI = ControlSetupControlGroupUI.Create( this, (currentRow + i) * ROW_HEIGHT, groupArray[i], attr );
						childGroups.Add( groupUI );
						currentRow += groupUI.TotalRows;
					}
				}

				//

				if( member is ControlleeInput input )
				{
					inputUIs.Add( ControlSetupControlUI.Create( this, 0f, currentRow * ROW_HEIGHT, input, attr ) );
					currentRow++;
				}
				if( member is ControlleeInput[] inputArray )
				{
					for( int i = 0; i < inputArray.Length; i++ )
					{
						inputUIs.Add( ControlSetupControlUI.Create( this, 0f, currentRow * ROW_HEIGHT, inputArray[i], attr ) );
						currentRow++;
					}

				}

				//

				if( member is ControllerOutput output )
				{
					outputUIs.Add( ControlSetupControlUI.Create( this, 1f, currentRow * ROW_HEIGHT, output, attr ) );
					currentRow++;
				}
				if( member is ControllerOutput[] outputArray )
				{
					for( int i = 0; i < outputArray.Length; i++ )
					{
						outputUIs.Add( ControlSetupControlUI.Create( this, 1f, currentRow * ROW_HEIGHT, outputArray[i], attr ) );
						currentRow++;
					}
				}

				//

				if( member is ControlParameterInput paramInput )
				{
					inputUIs.Add( ControlSetupControlUI.Create( this, 0f, currentRow * ROW_HEIGHT, paramInput, attr ) );
					currentRow++;
				}
				if( member is ControlParameterInput[] paramInputArray )
				{
					for( int i = 0; i < paramInputArray.Length; i++ )
					{
						inputUIs.Add( ControlSetupControlUI.Create( this, 0f, currentRow * ROW_HEIGHT, paramInputArray[i], attr ) );
						currentRow++;
					}
				}

				//

				if( member is ControlParameterOutput paramOutput )
				{
					outputUIs.Add( ControlSetupControlUI.Create( this, 1f, currentRow * ROW_HEIGHT, paramOutput, attr ) );
					currentRow++;
				}
				if( member is ControlParameterOutput[] paramOutputArray )
				{
					for( int i = 0; i < paramOutputArray.Length; i++ )
					{
						outputUIs.Add( ControlSetupControlUI.Create( this, 1f, currentRow * ROW_HEIGHT, paramOutputArray[i], attr ) );
						currentRow++;
					}
				}
			}

			TotalRows = currentRow;

			_inputUIs = inputUIs.ToArray();
			_outputUIs = outputUIs.ToArray();
			_childGroups = childGroups.ToArray();
		}

		internal static ControlSetupControlGroupUI Create( ControlSetupWindowComponentUI node, Component component )
		{
			UIPanel panel = node.panel.AddPanel( UILayoutInfo.Fill( 0, 0, 20, 5 ), null ); // AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" )

			ControlSetupControlGroupUI groupUI = panel.gameObject.AddComponent<ControlSetupControlGroupUI>();
			groupUI.panel = panel;
			groupUI._target = component;
			groupUI._attr = null;
			groupUI.ComponentUI = node;

			groupUI.RedrawControls();

			return groupUI;
		}

		internal static ControlSetupControlGroupUI Create( ControlSetupControlGroupUI group, float verticalOffset, object target, NamedControlAttribute attr )
		{
			UIPanel panel = group.panel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, UILayoutInfo.TopF, -verticalOffset, ROW_HEIGHT ), null );

			ControlSetupControlGroupUI groupUI = panel.gameObject.AddComponent<ControlSetupControlGroupUI>();
			groupUI.panel = panel;
			groupUI._target = target;
			groupUI._attr = attr;
			groupUI.ParentGroup = group;
			groupUI.ComponentUI = group.ComponentUI;

			groupUI.RedrawControls();

			panel.rectTransform.sizeDelta = new Vector2( panel.rectTransform.sizeDelta.y, groupUI.TotalRows * ROW_HEIGHT );

			return groupUI;
		}
	}
}