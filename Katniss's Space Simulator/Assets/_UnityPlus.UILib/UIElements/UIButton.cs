using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIButton : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal Button buttonComponent;
        internal Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        Action _onClick;
        public Action onClick
        {
            get => _onClick;
            set
            {
                buttonComponent.onClick.RemoveAllListeners();
                _onClick = value;
                if( _onClick != null )
                {
                    buttonComponent.onClick.AddListener( () => _onClick() ); // Find a way to cast System.Action to UnityAction if possible (the signatures of both delegates match).
                }
            }
        }

        public static UIButton Create( IUIElementContainer parent, UILayoutInfo layout, Sprite background, Action onClick )
        {
            (GameObject rootGameObject, RectTransform rootTransform, UIButton uiButton) = UIElement.CreateUIGameObject<UIButton>( parent, "uilib-button", layout );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            Button buttonComponent = rootGameObject.AddComponent<Button>();
            buttonComponent.targetGraphic = backgroundComponent;
            buttonComponent.transition = Selectable.Transition.ColorTint;
            buttonComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };
            buttonComponent.targetGraphic = backgroundComponent;

            uiButton.buttonComponent = buttonComponent;
            uiButton.backgroundComponent = backgroundComponent;
            uiButton.onClick = onClick;
            return uiButton;
        }
    }
}