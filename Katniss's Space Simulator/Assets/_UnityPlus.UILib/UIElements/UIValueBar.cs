using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public partial class UIValueBar : UIElement, IUIElementChild
    {
        protected ValueBar valueBarComponent;

        public IUIElementContainer Parent { get; set; }

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

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, Sprite background ) where T : UIValueBar
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiValueBar) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{nameof( T )}", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            ValueBar valueBarComponent = rootGameObject.AddComponent<ValueBar>();
            valueBarComponent.PaddingLeft = 1.0f;
            valueBarComponent.PaddingRight = 1.0f;
            valueBarComponent.Spacing = 1.0f;

            uiValueBar.valueBarComponent = valueBarComponent;
            return uiValueBar;
        }
    }
}