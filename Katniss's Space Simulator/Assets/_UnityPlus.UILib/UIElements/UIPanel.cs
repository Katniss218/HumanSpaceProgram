using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIPanel : UIElement
    {
        public UnityEngine.UI.Image backgroundComponent;

        public UIPanel( RectTransform transform, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this.backgroundComponent = backgroundComponent;
        }
    }
}