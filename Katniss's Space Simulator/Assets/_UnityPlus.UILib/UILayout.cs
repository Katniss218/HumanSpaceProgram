using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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



        // when property of UI element is modified, it needs to notify that UI element (sprite, text, etc). Mostly text though.

        // when the size of the UI element is modified, it needs to notify the children, then the parent (recursively).
        // - if child is text and new size has less lines, it can shrink (assuming fit to size vert or hor is present)
        // - - then the parent can use the new chrunken child and move the vertical layout elements up to fill the now empty space.
        // when the position is changed, it only needs to notify the parent.
        // - if parent is sequential layout group, it will just overwrite the position change.
    }
}