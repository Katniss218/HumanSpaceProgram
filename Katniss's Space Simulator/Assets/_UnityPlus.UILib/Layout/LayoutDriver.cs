using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    /// <summary>
    /// Specifies how a UI element will respond to layout updates.
    /// </summary>
    public abstract class LayoutDriver
    {
        /// <summary>
        /// Runs the driver on the specified element.
        /// </summary>
        public abstract void DoLayout( IUILayoutDriven c );
    }
}