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

        internal IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        public override void Destroy()
        {
            base.Destroy();
            _parent.Children.Remove( this );
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public UnityEvent onClick => buttonComponent.onClick;

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

            if( onClick != null )
            {
                buttonComponent.onClick.AddListener( () => onClick() ); // Find a way to cast System.Action to UnityAction if possible (the signatures of both delegates match).
            }

            UIButton button = rootGameObject.AddComponent<UIButton>();

            button.Children = new List<IUIElementChild>();
            button._parent = parent;
            button.Parent.Children.Add( button );
            button.buttonComponent = buttonComponent;
            button.backgroundComponent = backgroundComponent;
            return button;
        }
    }
}