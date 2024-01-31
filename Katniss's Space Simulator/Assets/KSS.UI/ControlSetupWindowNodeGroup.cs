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

namespace KSS.UI
{
    public class ControlSetupWindowNodeGroup : MonoBehaviour
    {
        ControlSetupWindowNode _node;

        ControlGroup _controlGroup;
        NamedControlAttribute _attr;

        public int Height => throw new NotImplementedException( "height in 'units' of this group." );

        internal static ControlSetupWindowNodeGroup Create( ControlSetupWindowNode node, float verticalOffset, ControlGroup controlGroup, NamedControlAttribute attr )
        {
            UIIcon icon = node.panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopLeft, UILayoutInfo.TopLeft, new Vector2( 0, verticalOffset ), new Vector2( 10, 10 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_input" ) );

            ControlSetupWindowNodeGroup input = icon.gameObject.AddComponent<ControlSetupWindowNodeGroup>();
            input._node = node;
            input._controlGroup = controlGroup;
            input._attr = attr;

            return input;
        }
    }
}
