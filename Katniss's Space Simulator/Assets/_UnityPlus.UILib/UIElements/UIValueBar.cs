using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIValueBar : UIElement
    {
#warning TODO - merge this with ValueBar
        internal ValueBar valueBarComponent;

        internal IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        public void ClearSegments()
        {
            valueBarComponent.ClearSegments();
        }

        public ValueBarSegment AddSegment( float width )
        {
            return valueBarComponent.AddSegment( width );
        }

        public ValueBarSegment InsertSegment( int index, float width )
        {
            return valueBarComponent.InsertSegment( index, width );
        }

        public static UIValueBar Create( IUIElementContainer parent, UILayoutInfo layout, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-valuebar", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            ValueBar valueBarComponent = rootGameObject.AddComponent<ValueBar>();
            valueBarComponent.PaddingLeft = 1.0f;
            valueBarComponent.PaddingRight = 1.0f;
            valueBarComponent.Spacing = 1.0f;

            UIValueBar bar = rootGameObject.AddComponent<UIValueBar>();
            bar._parent = parent;
            bar.valueBarComponent = valueBarComponent;
            return bar;
        }

    }
}