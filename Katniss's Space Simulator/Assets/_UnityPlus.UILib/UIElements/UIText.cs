using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIText : UIElement
    {
        public TMPro.TextMeshProUGUI textComponent;

        public UIText( RectTransform transform, TMPro.TextMeshProUGUI textComponent ) : base(transform)
        {
            this.textComponent = textComponent;
        }
    }
}