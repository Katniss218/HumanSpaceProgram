using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.UILib.UIElements
{
    public interface IUILayoutSelf : IUIElementChild
    {
        /// <summary>
        /// Runs the driver on the specified container element.
        /// </summary>
        public abstract void DoLayout();
    }
}