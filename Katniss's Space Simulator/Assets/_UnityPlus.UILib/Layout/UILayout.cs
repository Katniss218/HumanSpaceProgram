using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    public static class UILayout
    {
        // when modifying the element in a way that can change the properties, we should update the layouts.

        // the parent element needs to remember how its children are supposed to be laid out.
        // potentially can use dependency injection with a subclass/func.

        // sequential layout:
        // - horizontal
        // - vertical
        // - grid

        // "fit" layout (fit to size of contents)

        // fit layout only makes sense for container-style elements and text.

        // when property of UI element is modified, it needs to notify that UI element (sprite, text, etc). Mostly text though.

        // when the size of the UI element is modified, it needs to notify the children, then the parent (recursively).
        // - if child is text and new size has less lines, it can shrink (assuming fit to size vert or hor is present)
        // - - then the parent can use the new shrunken child and move the vertical layout elements up to fill the now empty space.
        // when the position is changed, it only needs to notify the parent.
        // - if parent is sequential layout group, it will just overwrite the position change.

        static HashSet<object> _set = new HashSet<object>();

        private static void BroadcastLayoutUpdateRecursive( object elem )
        {
            // passed in element is the element that is updating.
            // we need to update its children.
            // then update its parent.

            // ----

            // we have a list of texts.
            // the list has changed its width
            // we update its children (texts) to fit the new width (more or less lines = different height)
            // then update the list itself (re-vertical layout it).
            // then update its parent, if applicable.

            // ----

            // TODO - this can be optimized in many ways - marking as stale with something changes and redrawing only once in lateupdate, not updating if nothing has changed, etc.

            // Update children.
            if( elem is IUIElementContainer co )
            {
                foreach( var e in co.Children )
                {
                    if( !_set.Contains( e ) )
                    {
                        _set.Add( e );
                        BroadcastLayoutUpdateRecursive( e );
                    }
                }
            }

            // Update self.
            if( elem is IUILayoutDriven ld )
            {
                ld.LayoutDriver?.DoLayout( ld );
            }
            else if( elem is IUILayoutSelf ls )
            {
                ls.DoLayout();
            }

            // Update parent.
            if( elem is IUIElementChild ci )
            {
                if( !_set.Contains( ci.Parent ) )
                {
                    _set.Add( ci.Parent );
                    BroadcastLayoutUpdateRecursive( ci.Parent );
                }
            }
        }

        public static void BroadcastLayoutUpdate( object elem ) // `object` is kinda ugly here.
        {
#warning TODO - call this when element is added or rearranged.

            _set.Clear();
            BroadcastLayoutUpdateRecursive( elem );
        }
    }
}