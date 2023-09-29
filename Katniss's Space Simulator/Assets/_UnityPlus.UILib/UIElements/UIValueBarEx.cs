using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIValueBarEx
    {
        public static UIValueBar AddHorizontalValueBar( this IUIElementContainer parent, UILayoutInfo layout, Sprite background )
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

            return new UIValueBar( rootTransform, parent, valueBarComponent );
        }

        public static UIValueBar WithPadding( this UIValueBar valueBar, float paddingleft, float paddingRight, float spacing )
        {
            var valueBarComponent = valueBar.valueBarComponent;
            valueBarComponent.PaddingLeft = paddingleft;
            valueBarComponent.PaddingRight = paddingRight;
            valueBarComponent.Spacing = spacing;

            return valueBar;
        }
    }
}