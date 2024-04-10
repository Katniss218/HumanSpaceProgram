using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    /// <summary>
    /// Specifies how to lay out the child elements of a container.
    /// </summary>
    public abstract class LayoutDriver
    {
        /// <summary>
        /// Runs the driver on the specified container element.
        /// </summary>
        public abstract void DoLayout( IUIElementContainer c );
    }
}