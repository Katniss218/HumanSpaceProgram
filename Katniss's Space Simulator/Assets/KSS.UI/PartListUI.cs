using System.Collections.Generic;
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
        private IUIElementContainer _categoryList;

        public void Refresh()
        {
            foreach( var entry in _entries )
            {
                Destroy( entry.gameObject );
            }
            _entries.Clear();

            // update part IDs from *somewhere*
            _partIds = new List<string>() { "aa", "bb", "cc", "dd", "ee", "ff", "gg",
                                            "aa", "bb", "cc", "dd", "ee", "ff", "gg",
                                            "aa", "bb", "cc", "dd", "ee", "ff", "gg",
                                            "aa", "bb", "cc", "dd", "ee", "ff", "gg",
                                            "aa", "bb", "cc", "dd", "ee", "ff", "gg",
                                            "aa", "bb", "cc", "dd", "ee", "ff", "gg", };

            foreach( var id in _partIds )
            {
                PartListEntryUI.Create( _list, new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 75, 75 ) ), id );
            }
            UILayout.BroadcastLayoutUpdate( _list );
            // create entries for every part id.
        }

        public static PartListUI Create( IUIElementContainer parent, UILayoutInfo layout )
        {
            UIPanel uiPanel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_background" ) );
            UIInputField filterIF = uiPanel.AddInputField( UILayoutInfo.FillHorizontal( 2, 2, 1f, -2, 20 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field_ha" ) );
            UIScrollView uiPartList = uiPanel.AddVerticalScrollView( UILayoutInfo.Fill( 42, 2, 24, 100 ), 200 )
                .WithVerticalScrollbar( UILayoutInfo.FillVertical( 0, 0, 1f, 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );
            UIScrollView categoryScroll = uiPanel.AddVerticalScrollView( UILayoutInfo.FillVertical( 24, 100, 0f, 0, 40 ), 200 );

            uiPartList.LayoutDriver = new BidirectionalLayoutDriver()
            {
                Spacing = new Vector2( 2, 2 ),
                FreeAxis = BidirectionalLayoutDriver.Axis2D.Y,
                FitToSize = true
            };

            PartListUI partListUI = uiPanel.gameObject.AddComponent<PartListUI>();
            partListUI._partIds = new List<string>();
            partListUI._list = uiPartList;
            partListUI._categoryList = categoryScroll;
            partListUI.Refresh();

            return partListUI;
        }
    }
}