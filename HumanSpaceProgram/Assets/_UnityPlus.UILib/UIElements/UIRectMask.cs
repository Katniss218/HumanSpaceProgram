using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a section of the canvas, or of a different UI element.
    /// </summary>
    public partial class UIRectMask : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layoutInfo ) where T : UIRectMask
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiMask) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.maskable = true;

            Mask maskComponent = rootGameObject.AddComponent<Mask>();
            maskComponent.showMaskGraphic = false;

            return uiMask;
        }
    }
}