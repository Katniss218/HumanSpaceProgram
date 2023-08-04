using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIValueBar : UIElement
    {
        public UnityPlus.UILib.ValueBar valueBarComponent;

        public UIValueBar( RectTransform transform, UnityPlus.UILib.ValueBar valueBarComponent ) : base( transform )
        {
            this.valueBarComponent = valueBarComponent;
        }
    }
}