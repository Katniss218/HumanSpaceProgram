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

        public IUIElementContainer Parent { get; set; }

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

        public void DoLayout()
        {
            if( !FitToContents )
            {
                return;
            }
            if( this.rectTransform == null )
            {

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
            (GameObject rootGameObject, RectTransform rootTransform, UIText uiText) = UIElement.CreateUIGameObject<UIText>( parent, "uilib-text", layoutInfo );

            TMPro.TextMeshProUGUI textComponent = rootGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.raycastTarget = false;
            textComponent.richText = false;
            textComponent.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            textComponent.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;

            textComponent.text = text;

            uiText.textComponent = textComponent;
            return uiText;
        }
    }
}