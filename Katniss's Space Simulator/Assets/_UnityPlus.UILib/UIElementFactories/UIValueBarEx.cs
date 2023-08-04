using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIValueBarEx
    {
        public static UIValueBar AddHorizontalValueBar( this UIElement parent, UILayoutInfo layout, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-valuebar", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            ValueBar valueBarComponent = rootGameObject.AddComponent<ValueBar>();
            valueBarComponent.PaddingLeft = 1.0f;
            valueBarComponent.PaddingRight = 1.0f;
            valueBarComponent.Spacing = 1.0f;

            return new UIValueBar( rootTransform, valueBarComponent );
        }

        public static UIValueBar WithPadding( UIValueBar valueBar, float paddingleft, float paddingRight, float spacing )
        {
            var valueBarComponent = valueBar.valueBarComponent;
            valueBarComponent.PaddingLeft = paddingleft;
            valueBarComponent.PaddingRight = paddingRight;
            valueBarComponent.Spacing = spacing;

            return valueBar;
        }
    }
}