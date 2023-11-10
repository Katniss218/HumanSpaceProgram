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

        public List<IUIElementChild> Children { get; private set; }

        public IUIElementContainer Parent { get; private set; }

        public LayoutDriver LayoutDriver { get; set; }

        void OnDestroy()
        {
            this.Parent.Children.Remove( this );
        }

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

        public static UIButton Create( IUIElementContainer parent, UILayoutInfo layout, Sprite sprite, Action onClick )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-button", layout );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = sprite;
            backgroundComponent.type = Image.Type.Sliced;

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

            UIButton uiButton = rootGameObject.AddComponent<UIButton>();

            uiButton.Children = new List<IUIElementChild>();
            uiButton.Parent = parent;
            uiButton.Parent.Children.Add( uiButton );
            uiButton.buttonComponent = buttonComponent;
            uiButton.backgroundComponent = backgroundComponent;
            uiButton.onClick = onClick;
            return uiButton;
        }
    }
}