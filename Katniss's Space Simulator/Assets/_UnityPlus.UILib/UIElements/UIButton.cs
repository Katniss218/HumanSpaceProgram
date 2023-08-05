using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIButton : UIElement
    {
        public readonly UnityEngine.UI.Button buttonComponent;

        public UIButton( RectTransform transform, UnityEngine.UI.Button buttonComponent ) : base( transform )
        {
            this.buttonComponent = buttonComponent;
        }
    }
}