using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public interface IUILayoutDriven : IUIElementContainer
    {
        LayoutDriver LayoutDriver { get; set; }
    }
}