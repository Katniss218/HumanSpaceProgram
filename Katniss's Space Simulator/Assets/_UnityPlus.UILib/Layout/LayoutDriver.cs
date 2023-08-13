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
        //public void OnLayoutChange( UIElement target, IUIElementContainer parent );

        public abstract void OnSetProperty( UIElement target ); // if we include fit to size, this would need to know how to handle every ui element.
                                                                // Maybe we should have different UI elements that lay themselves out instead of this layout bs everywhere.
                                                                // And just a boolean to fit to size or something.

        public abstract void OnPositionChange( UIElement target );

        public abstract void OnSizeChange();
    }
}
