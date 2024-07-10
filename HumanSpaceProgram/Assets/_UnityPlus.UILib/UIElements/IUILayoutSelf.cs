using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.UILib.UIElements
{
    public interface IUILayoutSelf : IUIElementChild
    {
        public abstract void DoLayout();
    }
}