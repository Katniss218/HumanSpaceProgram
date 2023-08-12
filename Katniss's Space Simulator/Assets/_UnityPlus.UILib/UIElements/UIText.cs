using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that is a container for text.
    /// </summary>
    public sealed class UIText : UIElement, IUIElementChild
    {
        internal readonly TMPro.TextMeshProUGUI textComponent;

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        //public LayoutDriver LayoutDriver { get; } = new FitToSizeDriver();

        internal UIText( RectTransform transform, IUIElementContainer parent, TMPro.TextMeshProUGUI textComponent ) : base( transform )
        {
            this._parent = parent;
            this.Parent.Children.Add( this );
            this.textComponent = textComponent;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }

        public string text
        {
            get => textComponent.text;
            set
            {
                textComponent.text = value;
                //this.LayoutDriver.RunSelf( this );
            }
        }

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