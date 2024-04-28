using KSS.Core;
using KSS.Core.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class UIPartList : UIPanel
    {
        private PartMetadata[] _parts;
        private string[] _categories;

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

            foreach( var id in PartMetadata.Filtered( _parts, _category, _filter ) )
            {
                _list.AddPartListEntry( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (75, 75) ), id );
            }
            UILayoutManager.ForceLayoutUpdate( _list );

            foreach( var cat in _categories )
            {
                string label = cat == null
                    ? "ALL"
                    : cat.Length < 4
                        ? cat
                        : cat[..4];

                _categoryList.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (40, 34) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/category_background" ), () =>
                 {
                     this._category = cat;
                     this.Refresh();
                 } )
                    .WithText( new UILayoutInfo( UIFill.Fill() ), label ?? "all", out var text );
                text.WithAlignment( TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle );
            }
            UILayoutManager.ForceLayoutUpdate( _categoryList );
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UIPartList
        {
            T partListUI = UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_list_background" ) );

            UIInputField<string> filterIF = partListUI.AddStringInputField( new UILayoutInfo( UIFill.Horizontal( 2, 2 ), UIAnchor.Top, -2, 20 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) )
                .WithMargins( 15, 15, 0, 0 )
                .WithPlaceholder( "search..." );
            UIScrollView uiPartList = partListUI.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill( 42, 2, 24, 100 ) ), 200 )
                .WithVerticalScrollbar( UIAnchor.Right, 10, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical_background" ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/scrollbar_vertical" ), out _ );

            uiPartList.LayoutDriver = new BidirectionalLayoutDriver()
            {
                Spacing = new Vector2( 2, 2 ),
                FreeAxis = BidirectionalLayoutDriver.Axis2D.Y,
                FitToSize = true
            };

            UIScrollView categoryScroll = partListUI.AddVerticalScrollView( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical( 24, 100 ), 0, 40 ), 200 );

            categoryScroll.LayoutDriver = new VerticalLayoutDriver()
            {
                Spacing = -2f,
                FitToSize = true
            };

            // update part IDs from *somewhere*
            partListUI._parts = PartRegistry.LoadAllMetadata();

            var categories = new List<string>();
            var categories2 = PartMetadata.GetUniqueCategories( partListUI._parts );
            categories.Add( null );
            categories.AddRange( categories2 );
            partListUI._categories = categories.ToArray();

            partListUI._list = uiPartList;
            partListUI._categoryList = categoryScroll;
            partListUI.Refresh();

            return partListUI;
        }
    }

    public static class UIPartList_Ex
    {
        public static UIPartList AddPartList( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UIPartList.Create<UIPartList>( parent, layout );
        }
    }
}