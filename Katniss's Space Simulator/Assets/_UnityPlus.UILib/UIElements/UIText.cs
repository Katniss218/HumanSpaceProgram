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
        // possibly we could have different types of text elements that format themselves in different ways. Headers, paragraphs, etc.

        internal TMPro.TextMeshProUGUI textComponent;

        internal IUIElementContainer _parent;

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
            // `textComponent.GetPreferredValues` seems to use the current anchors and size when computing the preferred size of the text container.
            if( layout.FillsWidth && !layout.FillsHeight )
            {
                this.rectTransform.sizeDelta = new Vector2( this.rectTransform.sizeDelta.x, textComponent.GetPreferredValues().y );
                return;
            }
            if( layout.FillsHeight && !layout.FillsWidth )
            {
                this.rectTransform.sizeDelta = new Vector2( textComponent.GetPreferredValues().x, this.rectTransform.GetActualSize().y );
                return;
            }
        }

        public static UIText Create( IUIElementContainer parent, UILayoutInfo layoutInfo, string text )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-text", layoutInfo );

            TMPro.TextMeshProUGUI textComponent = rootGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.raycastTarget = false;
            textComponent.richText = false;
            textComponent.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            textComponent.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;

            textComponent.text = text;

            UIText uiText = rootGameObject.AddComponent<UIText>();
            uiText._parent = parent;
            uiText.Parent.Children.Add( uiText );
            uiText.textComponent = textComponent;
            return uiText;
        }
    }
}