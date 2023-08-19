using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIInputField : UIElement, IUIElementChild
    {
        internal readonly TMPro.TMP_InputField inputFieldComponent;
        internal readonly TMPro.TextMeshProUGUI textComponent;
        internal readonly TMPro.TextMeshProUGUI placeholderComponent;
        internal readonly UnityEngine.UI.Image backgroundComponent;

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        internal UIInputField( RectTransform transform, IUIElementContainer parent, TMPro.TMP_InputField inputFieldComponent, TMPro.TextMeshProUGUI textComponent, TMPro.TextMeshProUGUI placeholderComponent ) : base( transform )
        {
            this._parent = parent;
            this.inputFieldComponent = inputFieldComponent;
            this.textComponent = textComponent;
            this.placeholderComponent = placeholderComponent;
        }

        public string Text { get => textComponent.text; set => textComponent.text = value; }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }
    }
}