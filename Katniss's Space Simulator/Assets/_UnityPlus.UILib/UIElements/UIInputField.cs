using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIInputField : UIElement
    {
        public TMPro.TMP_InputField inputFieldComponent;

        public UIInputField( RectTransform transform, TMPro.TMP_InputField inputFieldComponent ) : base( transform )
        {
            this.inputFieldComponent = inputFieldComponent;
        }
    }
}