using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that is a container for text.
    /// </summary>
    public sealed class UIText : UIElement
    {
        internal readonly TMPro.TextMeshProUGUI textComponent;

        internal readonly IUIElementParent _parent;
        public IUIElementParent parent { get => _parent; }

        internal UIText( RectTransform transform, IUIElementParent parent, TMPro.TextMeshProUGUI textComponent ) : base(transform)
        {
            this._parent = parent;
            this.textComponent = textComponent;
        }

        public string text { get => textComponent.text; set => textComponent.text = value; }

        public override Vector2 GetPreferredSize()
        {
            var layout = this.rectTransform.GetLayoutInfo();

            // Preferred size depends on how many line breaks exist in the text after wrapping to the size of the container.
            if( layout.FillsWidth && !layout.FillsHeight )
            {
                return new Vector2( this.rectTransform.sizeDelta.x, textComponent.GetPreferredValues( this.rectTransform.sizeDelta.x, 0 ).y );
            }
            if( layout.FillsHeight && !layout.FillsWidth )
            {
                return new Vector2( textComponent.GetPreferredValues( 0, this.rectTransform.sizeDelta.y ).x, this.rectTransform.sizeDelta.y );
            }

            // If the text wrapping can't grow its container in any direction.
            return layout.sizeDelta; // `sizeDelta` is wrong, should return the absolute size in xy instead of difference from anchor.
                                     // We can't use rectTransform.rect because it might be driven by other components.
        }
    }
}