using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIText : UIElement
    {
        internal readonly TMPro.TextMeshProUGUI textComponent;

        public UIText( RectTransform transform, TMPro.TextMeshProUGUI textComponent ) : base(transform)
        {
            this.textComponent = textComponent;
        }

        public string text { get => textComponent.text; set => textComponent.text = value; }
    }
}