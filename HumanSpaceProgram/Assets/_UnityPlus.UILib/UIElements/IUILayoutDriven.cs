using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Use this to specify that the UI element will respond to layout updates using a <see cref="Layout.LayoutDriver"/>.
    /// </summary>
    public interface IUILayoutDriven : IUIElement
    {
        LayoutDriver LayoutDriver { get; set; }
    }
}