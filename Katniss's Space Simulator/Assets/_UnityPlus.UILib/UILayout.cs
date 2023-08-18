using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
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

        public static void BroadcastLayoutUpdate( object elem ) // `object` here is kinda ugly.
        {
            if( elem is IUIElementContainer co )
            {
                foreach( var e in co.Children )
                {
                    if( !(e is IUILayoutDriven d) ) // this is probably wrong, but I'm too tired to work on this today.
                    {
                        continue;
                    }

                    BroadcastLayoutUpdate( e );
                }
            }
            if( elem is IUIElementChild ci )
            {
                if( !(elem is IUILayoutDriven d) )
                {
                    return;
                }

                BroadcastLayoutUpdate( ci.Parent );
            }
        }
    }
}