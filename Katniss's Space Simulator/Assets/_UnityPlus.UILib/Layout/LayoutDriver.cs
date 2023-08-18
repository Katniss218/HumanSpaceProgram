using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    public abstract class LayoutDriver
    {
        public abstract void DoLayout( IUIElementContainer c );

        /*public abstract void OnSetProperty( UIElement target );

        public abstract void OnPositionChange( UIElement target );

        public abstract void OnSizeChange();*/
    }
}
