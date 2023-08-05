using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIInputField : UIElement
    {
        public readonly TMPro.TMP_InputField inputFieldComponent;
        public readonly TMPro.TextMeshProUGUI textComponent;
        public readonly TMPro.TextMeshProUGUI placeholderComponent;

        public UIInputField( RectTransform transform, TMPro.TMP_InputField inputFieldComponent, TMPro.TextMeshProUGUI textComponent, TMPro.TextMeshProUGUI placeholderComponent ) : base( transform )
        {
            this.inputFieldComponent = inputFieldComponent;
            this.textComponent = textComponent;
            this.placeholderComponent = placeholderComponent;
        }
    }
}