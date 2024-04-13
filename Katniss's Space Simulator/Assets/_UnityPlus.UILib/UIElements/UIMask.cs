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
    public partial class UIMask : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        protected Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite mask ) where T : UIMask
        {
            if( mask == null )
            {
                throw new ArgumentNullException( nameof( mask ), $"Mask can't be null." );
            }

            (GameObject rootGameObject, RectTransform rootTransform, T uiMask) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = mask;
            backgroundComponent.type = Image.Type.Simple;

            Mask maskComponent = rootGameObject.AddComponent<Mask>();
            maskComponent.showMaskGraphic = false;

            uiMask.backgroundComponent = backgroundComponent;
            return uiMask;
        }
    }
}