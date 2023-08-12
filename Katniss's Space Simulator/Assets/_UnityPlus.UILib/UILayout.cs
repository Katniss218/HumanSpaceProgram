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
        // - "circular"

        // "fit" layout (fit to size of contents)

        // when an object is modified in a way that changes its layout, it needs to communicate that change to its parent. The parent can then adjust its layout.
    }
}