using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIInputFieldEx
    {
        public static UIInputField AddInputField( UIElement parent, UILayoutInfo layout, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-inputfield", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            TMPro.TMP_InputField inputFieldComponent = rootGameObject.AddComponent<TMPro.TMP_InputField>();
            inputFieldComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };

            (GameObject textareaGameObject, RectTransform textareaTransform) = UIHelper.CreateUI( rootTransform, "uilib-inputfieldtextarea", new UILayoutInfo( Vector2.zero, Vector2.one, Vector2.zero, new Vector2( -10, -10 ) ) );
            
            (GameObject placeholderGameObject, RectTransform placeholderTransform) = UIHelper.CreateUI( textareaTransform, "uilib-inputfieldplaceholder", UILayoutInfo.Fill() );
            (GameObject textGameObject, RectTransform textTransform) = UIHelper.CreateUI( textareaTransform, "uilib-inputfieldtext", UILayoutInfo.Fill() );

            return new UIInputField( rootTransform, inputFieldComponent );
        }
    }
}