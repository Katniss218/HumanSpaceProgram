using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class PartListUI : MonoBehaviour
    {
        private List<string> _partIds;
        private List<PartListEntryUI> _entries = new List<PartListEntryUI>();
        private IUIElementContainer _list;

        public void Refresh()
        {
            foreach( var entry in _entries )
            {
                Destroy( entry.gameObject );
            }
            _entries.Clear();

            // update part IDs from *somewhere*

            foreach( var id in _partIds )
            {
                PartListEntryUI.Create( _list, UILayoutInfo.Fill(), id );
            }
            UILayout.BroadcastLayoutUpdate( _list );
            // create entries for every part id.
        }

        public static PartListUI Create( IUIElementContainer parent, UILayoutInfo layout )
        {
            UIPanel uiPanel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/sequencer_element" ) );
            UIScrollView uiScroll = uiPanel.AddVerticalScrollView( layout, 200 );

            PartListUI partListUI = uiPanel.gameObject.AddComponent<PartListUI>();
            partListUI._partIds = new List<string>();
            partListUI._list = uiScroll;
            partListUI.Refresh();

            return partListUI;
        }
    }
}