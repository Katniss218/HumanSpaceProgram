using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that is a container for text.
    /// </summary>
    public sealed class UIText : UIElement, IUIElementChild, IUILayoutSelf
    {
        internal readonly TMPro.TextMeshProUGUI textComponent;

        internal readonly IUIElementContainer _parent;

        public IUIElementContainer Parent { get => _parent; }

        public bool FitToContents { get; set; } = false;

        public string Text
        {
            get => textComponent.text;
            set
            {
                textComponent.text = value;
                UILayout.BroadcastLayoutUpdate( this );
            }
        }

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

        public void DoLayout()
        {
            if( !FitToContents )
            {
                return;
            }

            UILayoutInfo layout = this.rectTransform.GetLayoutInfo();

            // Preferred size depends on how many line breaks exist in the text after wrapping to the size of the container.
            if( layout.FillsWidth && !layout.FillsHeight )
            {
                this.rectTransform.sizeDelta = new Vector2( this.rectTransform.sizeDelta.x, textComponent.GetPreferredValues( this.rectTransform.sizeDelta.x, 0 ).y );
                return;
            }
            if( layout.FillsHeight && !layout.FillsWidth )
            {
                this.rectTransform.sizeDelta = new Vector2( textComponent.GetPreferredValues( 0, this.rectTransform.sizeDelta.y ).x, this.rectTransform.sizeDelta.y );
                return;
            }
        }
    }
}