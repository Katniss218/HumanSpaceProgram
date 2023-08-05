using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIIcon : UIElement
    {
        public readonly UnityEngine.UI.Image imageComponent;

        public UIIcon( RectTransform transform, UnityEngine.UI.Image imageComponent ) : base( transform )
        {
            this.imageComponent = imageComponent;
        }
    }
}