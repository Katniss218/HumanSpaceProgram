using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
	public class ControlSetupControlUI : MonoBehaviour
	{
		public ControlSetupControlGroupUI Group { get; private set; }
		public Control.Control Control { get; private set; }

		NamedControlAttribute _attr;

		static ControlSetupControlUI _startedConnection;

		void OnClick()
		{

			// on release on this, when dragging out a connection - connect.

			// show the name/description of channel on mouseover.
			// also when pressed if connected to something - disconnect and hook the end to the mouse until released. if released over nothing - delete connection
			if( _startedConnection == null )
				_startedConnection = this;
			else
			{
				if( _startedConnection != this )
				{
					Group.ComponentUI.Window.TryConnectWithMouse( _startedConnection, this );
				}
				_startedConnection = null;
			}
		}

		internal static ControlSetupControlUI Create( ControlSetupControlGroupUI group, float side, float verticalOffset, Control.Control control, NamedControlAttribute attr )
		{
			UIPanel panel = group.panel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, UILayoutInfo.TopF, -verticalOffset, ControlSetupControlGroupUI.ROW_HEIGHT ), null );

			ControlSetupControlUI controlUI = panel.gameObject.AddComponent<ControlSetupControlUI>();
			controlUI.Group = group;
			controlUI.Control = control;
			controlUI._attr = attr;

			UIButton button = panel.AddButton( new UILayoutInfo( new Vector2( side, 1.0f ), Vector2.zero, new Vector2( ControlSetupControlGroupUI.ROW_HEIGHT, ControlSetupControlGroupUI.ROW_HEIGHT ) ),
				AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" ), controlUI.OnClick );

			UIText name = panel.AddText( UILayoutInfo.FillHorizontal( (1 - side) * ControlSetupControlGroupUI.ROW_HEIGHT, (side) * ControlSetupControlGroupUI.ROW_HEIGHT, UILayoutInfo.TopF, 0, ControlSetupControlGroupUI.ROW_HEIGHT ), attr.Name )
				.WithAlignment( side == 0 ? TMPro.HorizontalAlignmentOptions.Left : TMPro.HorizontalAlignmentOptions.Right );

			return controlUI;
		}
	}
}