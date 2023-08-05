using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIWindow : UIElement
    {
        public readonly UnityEngine.UI.Image backgroundComponent;

        public UIWindow( RectTransform transform, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this.backgroundComponent = backgroundComponent;
        }
    }
}