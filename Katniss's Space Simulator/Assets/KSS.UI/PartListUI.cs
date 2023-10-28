using KSS.Core.Serialization;
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
        private List<PartMetadata> _parts;
        private List<string> _categories;

        private IUIElementContainer _list;
        private IUIElementContainer _categoryList;

        private string _category = null;
        private string _filter = null;

        public void Refresh()
        {
            foreach( var entry in _list.Children.ToArray() )
            {
                entry.Destroy();
            }
            foreach( var cat in _categoryList.Children.ToArray() )
            {
                cat.Destroy();
            }

            foreach( var id in _parts )
            {
                // TODO - filter.
                PartListEntryUI.Create( _list, new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 75, 75 ) ), id );
            }
            UILayout.BroadcastLayoutUpdate( _list );

            foreach( var cat in _categories )
            {
                _categoryList.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 40, 34 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/category_background" ), () =>
                 {
                     this._category = cat;
                     this.Refresh();
                 } )
                    .WithText( UILayoutInfo.Fill(), cat?[0].ToString() ?? "all", out var text );
                text.WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );
            }
            UILayout.BroadcastLayoutUpdate( _categoryList );
        }

        public static PartListUI Create( IUIElementContainer parent, UILayoutInfo layout )
        {
            UIPanel uiPanel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_background" ) );
            UIInputField filterIF = uiPanel.AddInputField( UILayoutInfo.FillHorizontal( 2, 2, 1f, -2, 20 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field_ha" ) )
                .WithMargins( 15, 15, 0, 0 )
                .WithPlaceholder( "search..." );
            UIScrollView uiPartList = uiPanel.AddVerticalScrollView( UILayoutInfo.Fill( 42, 2, 24, 100 ), 200 )
                .WithVerticalScrollbar( UILayoutInfo.FillVertical( 0, 0, 1f, 0, 10 ), null, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_handle" ), out _ );

            uiPartList.LayoutDriver = new BidirectionalLayoutDriver()
            {
                Spacing = new Vector2( 2, 2 ),
                FreeAxis = BidirectionalLayoutDriver.Axis2D.Y,
                FitToSize = true
            };

            UIScrollView categoryScroll = uiPanel.AddVerticalScrollView( UILayoutInfo.FillVertical( 24, 100, 0f, 0, 40 ), 200 );

            categoryScroll.LayoutDriver = new VerticalLayoutDriver()
            {
                Spacing = -2f,
                FitToSize = true
            };

            PartListUI partListUI = uiPanel.gameObject.AddComponent<PartListUI>();

            // update part IDs from *somewhere*
            partListUI._parts = new List<PartMetadata>()
            {
                new PartMetadata( "part.engine.f_1" ) { Author = "Katniss", Name = "F-1 Engine", Description = "Powerful", Categories = new [] { "engine" }, Filter = "nasa saturn apollo f1 f-1" },
                new PartMetadata( "part.engine.j_2" ) { Author = "Katniss", Name = "J-2 Engine", Description = "Powerful", Categories = new [] { "engine" }, Filter = "nasa saturn apollo f1 f-1" },
                new PartMetadata( "part.engine.ro_11" ) { Author = "Katniss", Name = "RO-11 Engine", Description = "Powerful", Categories = new [] { "engine" }, Filter = "nasa saturn apollo f1 f-1" },
                new PartMetadata( "part.apollo_cm" ) { Author = "Katniss", Name = "Apollo CM", Description = "Powerful", Categories = new [] { "command" }, Filter = "nasa saturn apollo f1 f-1" }
            };

            partListUI._categories = new List<string>()
            {
                null,
                "command",
                "fuel_tank",
                "engine",
                "aero",
                "structural",
            };

            partListUI._list = uiPartList;
            partListUI._categoryList = categoryScroll;
            partListUI.Refresh();

            return partListUI;
        }
    }
}