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
    public class PartListEntryUI : MonoBehaviour
    {
        private string _partId;

        void OnClick()
        {

        }

        public static PartListEntryUI Create( IUIElementContainer parent, UILayoutInfo layout, string partId )
        {
            UIPanel uiPanel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_entry_background" ) );

            PartListEntryUI partListEntryUI = uiPanel.gameObject.AddComponent<PartListEntryUI>();
            partListEntryUI._partId = partId;

            return partListEntryUI;
        }
    }
}