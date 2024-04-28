using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that is a container for text.
    /// </summary>
    public partial class UIText : UIElement, IUIElementChild, IUILayoutSelf
    {
        protected TMPro.TextMeshProUGUI textComponent;

        public IUIElementContainer Parent { get; set; }

        public bool FitToContents { get; set; } = false;

        public virtual string Text
        {
            get => textComponent.text;
            set
            {
                textComponent.text = value;
                UILayoutManager.ForceLayoutUpdate( this );
            }
        }

        public void DoLayout()
        {
            if( !FitToContents )
            {
                return;
            }

            // Preferred size depends on how many line breaks exist in the text after wrapping to the size of the container.
            // `textComponent.GetPreferredValues` seems to use the current anchors and size when computing the preferred size of the text container.
            if( this.rectTransform.FillsWidth() && !this.rectTransform.FillsHeight() )
            {
                this.rectTransform.sizeDelta = new Vector2( this.rectTransform.sizeDelta.x, textComponent.GetPreferredValues().y );
                return;
            }
            if( !this.rectTransform.FillsWidth() && this.rectTransform.FillsHeight() )
            {
                this.rectTransform.sizeDelta = new Vector2( textComponent.GetPreferredValues().x, this.rectTransform.GetActualSize().y );
                return;
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layoutInfo, string text ) where T : UIText
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiText) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layoutInfo );

            TMPro.TextMeshProUGUI textComponent = rootGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.raycastTarget = false;
            textComponent.richText = false;
            textComponent.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            textComponent.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            textComponent.overflowMode = TMPro.TextOverflowModes.Truncate;

            textComponent.text = text;

            uiText.textComponent = textComponent;
            return uiText;
        }
    }
}